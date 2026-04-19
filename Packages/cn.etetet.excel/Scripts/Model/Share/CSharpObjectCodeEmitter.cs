using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    public sealed class CSharpObjectCodeEmitter : Object
    {
        private readonly Dictionary<string, Type> runtimeTypes;

        public CSharpObjectCodeEmitter()
        {
            this.runtimeTypes = this.LoadRuntimeTypes();
        }

        public string Emit(object value)
        {
            EmitterContext context = new();
            return this.EmitValue(value, 0, context);
        }

        private string EmitValue(object value, int indentLevel, EmitterContext context)
        {
            if (value == null)
            {
                return "null";
            }

            Type runtimeType = value.GetType();
            if (this.IsUnityObject(runtimeType))
            {
                throw new NotSupportedException($"不支持导出 UnityEngine.Object 类型: {runtimeType.FullName}");
            }

            if (this.TryFormatInlineValue(runtimeType, value, out string inlineValue))
            {
                return inlineValue;
            }

            if (!runtimeType.IsValueType)
            {
                context.Enter(value, runtimeType);
            }

            try
            {
                if (this.TryGetDictionaryInfo(runtimeType, out DictionaryInfo dictionaryInfo))
                {
                    return this.EmitDictionary(runtimeType, dictionaryInfo, value, indentLevel, context);
                }

                if (this.TryGetListElementType(runtimeType, out Type listElementType))
                {
                    return this.EmitList(runtimeType, listElementType, (IEnumerable)value, indentLevel, context);
                }

                if (this.TryGetHashSetElementType(runtimeType, out Type hashSetElementType))
                {
                    return this.EmitHashSet(runtimeType, hashSetElementType, (IEnumerable)value, indentLevel, context);
                }

                if (runtimeType.IsArray)
                {
                    return this.EmitArray(runtimeType.GetElementType()!, (IEnumerable)value, indentLevel, context);
                }

                return this.EmitObject(runtimeType, value, indentLevel, context);
            }
            finally
            {
                if (!runtimeType.IsValueType)
                {
                    context.Exit(value);
                }
            }
        }

        private string EmitObject(Type runtimeType, object value, int indentLevel, EmitterContext context)
        {
            List<SerializableMember> members = this.EnumerateSerializableMembers(runtimeType)
                    .Where(member => !this.ShouldIgnoreValue(member, value))
                    .ToList();

            if (members.Count == 0)
            {
                return $"new {this.ToCSharpTypeName(runtimeType)}()";
            }

            List<SerializableMember> ctorMembers = members.Where(member => member.RequiresConstructor).ToList();
            ConstructorMatch constructorMatch = this.ResolveConstructor(runtimeType, ctorMembers);

            string ctorExpression = constructorMatch == null
                ? $"new {this.ToCSharpTypeName(runtimeType)}()"
                : $"{this.GetConstructorTypeName(runtimeType, constructorMatch.Constructor)}({string.Join(", ", constructorMatch.Arguments.Select(argument => this.EmitMemberValue(argument.Member, value, indentLevel + 1, context)))})";

            List<string> initializerAssignments = members
                    .Except(ctorMembers)
                    .Select(member => $"{this.Indent(indentLevel + 1)}{member.Name} = {this.EmitMemberValue(member, value, indentLevel + 1, context)},")
                    .ToList();

            if (initializerAssignments.Count == 0)
            {
                return ctorExpression;
            }

            return
$@"{ctorExpression}
{{
{string.Join(Environment.NewLine, initializerAssignments)}
{this.Indent(indentLevel)}}}";
        }

        private string EmitList(Type listType, Type elementType, IEnumerable values, int indentLevel, EmitterContext context)
        {
            List<object> items = values.Cast<object>().ToList();
            if (items.Count == 0)
            {
                return $"new {this.ToCSharpTypeName(listType)}()";
            }

            List<string> members = items
                    .Select(item => $"{this.Indent(indentLevel + 1)}{this.EmitValue(item, indentLevel + 1, context)},")
                    .ToList();

            return
$@"new {this.ToCSharpTypeName(listType)}
{{
{string.Join(Environment.NewLine, members)}
{this.Indent(indentLevel)}}}";
        }

        private string EmitHashSet(Type hashSetType, Type elementType, IEnumerable values, int indentLevel, EmitterContext context)
        {
            List<object> items = values.Cast<object>().OrderBy(this.GetStableOrderKey).ToList();
            if (items.Count == 0)
            {
                return $"new {this.ToCSharpTypeName(hashSetType)}()";
            }

            List<string> members = items
                    .Select(item => $"{this.Indent(indentLevel + 1)}{this.EmitValue(item, indentLevel + 1, context)},")
                    .ToList();

            return
$@"new {this.ToCSharpTypeName(hashSetType)}
{{
{string.Join(Environment.NewLine, members)}
{this.Indent(indentLevel)}}}";
        }

        private string EmitArray(Type elementType, IEnumerable values, int indentLevel, EmitterContext context)
        {
            List<object> items = values.Cast<object>().ToList();
            if (items.Count == 0)
            {
                return $"new {this.ToCSharpTypeName(elementType)}[0]";
            }

            List<string> members = items
                    .Select(item => $"{this.Indent(indentLevel + 1)}{this.EmitValue(item, indentLevel + 1, context)},")
                    .ToList();

            return
$@"new {this.ToCSharpTypeName(elementType)}[]
{{
{string.Join(Environment.NewLine, members)}
{this.Indent(indentLevel)}}}";
        }

        private string EmitDictionary(Type dictionaryType, DictionaryInfo dictionaryInfo, object value, int indentLevel, EmitterContext context)
        {
            List<DictionaryEntryData> entries = dictionaryInfo.GetEntries(value)
                    .OrderBy(entry => this.GetStableOrderKey(entry.Key))
                    .ToList();

            if (entries.Count == 0)
            {
                return $"new {this.ToCSharpTypeName(dictionaryType)}()";
            }

            List<string> members = entries
                    .Select(entry => $"{this.Indent(indentLevel + 1)}{{ {this.EmitValue(entry.Key, indentLevel + 1, context)}, {this.EmitValue(entry.Value, indentLevel + 1, context)} }},")
                    .ToList();

            return
$@"new {this.ToCSharpTypeName(dictionaryType)}
{{
{string.Join(Environment.NewLine, members)}
{this.Indent(indentLevel)}}}";
        }

        private string EmitMemberValue(SerializableMember member, object owner, int indentLevel, EmitterContext context)
        {
            object memberValue = member.GetValue(owner);
            return this.EmitValue(memberValue, indentLevel, context);
        }

        private IEnumerable<SerializableMember> EnumerateSerializableMembers(Type type)
        {
            if (type.BaseType != null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType))
            {
                foreach (SerializableMember member in this.EnumerateSerializableMembers(type.BaseType))
                {
                    yield return member;
                }
            }

            List<SerializableMember> declaredMembers = new();

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            Array.Sort(fields, (left, right) => left.MetadataToken.CompareTo(right.MetadataToken));
            foreach (FieldInfo field in fields)
            {
                if (!this.ShouldSerializeField(field))
                {
                    continue;
                }

                if (!this.RuntimeMemberExists(type, field.Name, MemberKind.Field))
                {
                    continue;
                }

                declaredMembers.Add(this.CreateSerializableField(field));
            }

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            Array.Sort(properties, (left, right) => left.MetadataToken.CompareTo(right.MetadataToken));
            foreach (PropertyInfo property in properties)
            {
                if (!this.ShouldSerializeProperty(property))
                {
                    continue;
                }

                if (!this.RuntimeMemberExists(type, property.Name, MemberKind.Property))
                {
                    continue;
                }

                declaredMembers.Add(this.CreateSerializableProperty(property));
            }

            foreach (SerializableMember member in declaredMembers)
            {
                yield return member;
            }
        }

        private bool ShouldSerializeField(FieldInfo field)
        {
            if (field.IsStatic || field.IsLiteral)
            {
                return false;
            }

            if (field.IsDefined(typeof(NonSerializedAttribute), false))
            {
                return false;
            }

            if (field.GetCustomAttribute<BsonIgnoreAttribute>() != null)
            {
                return false;
            }

            if (field.IsPublic)
            {
                return true;
            }

            return this.HasForceIncludeAttribute(field);
        }

        private bool ShouldSerializeProperty(PropertyInfo property)
        {
            if (property.GetIndexParameters().Length > 0)
            {
                return false;
            }

            MethodInfo getter = property.GetGetMethod(true);
            if (getter == null || getter.IsStatic)
            {
                return false;
            }

            if (property.GetCustomAttribute<BsonIgnoreAttribute>() != null)
            {
                return false;
            }

            bool forceInclude = this.HasForceIncludeAttribute(property);
            MethodInfo setter = property.GetSetMethod(true);
            if (property.GetMethod?.IsPublic == true || setter?.IsPublic == true)
            {
                return setter != null;
            }

            return forceInclude;
        }

        private bool HasForceIncludeAttribute(MemberInfo member)
        {
            return member.GetCustomAttribute<BsonElementAttribute>() != null || member.GetCustomAttribute<BsonIdAttribute>() != null;
        }

        private SerializableMember CreateSerializableField(FieldInfo field)
        {
            return new SerializableMember(
                field.Name,
                this.GetSerializationName(field),
                field.FieldType,
                !field.IsPublic || field.IsInitOnly,
                field.GetCustomAttribute<BsonIgnoreIfNullAttribute>() != null,
                field.GetCustomAttribute<BsonIgnoreIfDefaultAttribute>() != null,
                field.GetCustomAttribute<BsonDefaultValueAttribute>()?.DefaultValue,
                owner => field.GetValue(owner));
        }

        private SerializableMember CreateSerializableProperty(PropertyInfo property)
        {
            MethodInfo setter = property.GetSetMethod(true);
            bool canInitialize = setter?.IsPublic == true;
            return new SerializableMember(
                property.Name,
                this.GetSerializationName(property),
                property.PropertyType,
                !canInitialize,
                property.GetCustomAttribute<BsonIgnoreIfNullAttribute>() != null,
                property.GetCustomAttribute<BsonIgnoreIfDefaultAttribute>() != null,
                property.GetCustomAttribute<BsonDefaultValueAttribute>()?.DefaultValue,
                owner => property.GetValue(owner));
        }

        private string GetSerializationName(MemberInfo member)
        {
            BsonElementAttribute bsonElement = member.GetCustomAttribute<BsonElementAttribute>();
            if (!string.IsNullOrWhiteSpace(bsonElement?.ElementName))
            {
                return bsonElement.ElementName;
            }

            return member.Name;
        }

        private bool ShouldIgnoreValue(SerializableMember member, object owner)
        {
            object value = member.GetValue(owner);

            if (value == null && member.IgnoreIfNull)
            {
                return true;
            }

            if (member.IgnoreIfDefault)
            {
                object defaultValue = member.DefaultValue ?? this.GetDefaultValue(member.MemberType);
                if (this.AreValuesEqual(value, defaultValue))
                {
                    return true;
                }
            }

            return false;
        }

        private bool AreValuesEqual(object left, object right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            return left.Equals(right);
        }

        private ConstructorMatch ResolveConstructor(Type runtimeType, List<SerializableMember> ctorMembers)
        {
            if (ctorMembers.Count == 0)
            {
                return null;
            }

            ConstructorInfo[] publicConstructors = runtimeType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            List<ConstructorMatch> matches = new();
            foreach (ConstructorInfo constructor in publicConstructors)
            {
                if (!this.TryMatchConstructor(constructor, ctorMembers, out ConstructorMatch match))
                {
                    continue;
                }

                matches.Add(match);
            }

            ConstructorMatch attributeMatch = matches.SingleOrDefault(match => match.Constructor.GetCustomAttribute<BsonConstructorAttribute>() != null);
            if (attributeMatch != null)
            {
                return attributeMatch;
            }

            if (matches.Count == 1)
            {
                return matches[0];
            }

            throw new NotSupportedException(this.BuildMissingConstructorMessage(runtimeType, ctorMembers));
        }

        private bool TryMatchConstructor(ConstructorInfo constructor, List<SerializableMember> ctorMembers, out ConstructorMatch match)
        {
            match = null;

            ParameterInfo[] parameters = constructor.GetParameters();
            Dictionary<string, SerializableMember> membersByKey = this.BuildConstructorMemberLookup(ctorMembers);
            List<ConstructorArgument> arguments = new();
            HashSet<SerializableMember> boundMembers = new();

            foreach (ParameterInfo parameter in parameters)
            {
                if (!membersByKey.TryGetValue(parameter.Name ?? string.Empty, out SerializableMember member))
                {
                    if (parameter.IsOptional)
                    {
                        continue;
                    }

                    return false;
                }

                if (member.MemberType != parameter.ParameterType)
                {
                    return false;
                }

                boundMembers.Add(member);
                arguments.Add(new ConstructorArgument(parameter, member));
            }

            if (boundMembers.Count != ctorMembers.Count)
            {
                return false;
            }

            match = new ConstructorMatch(constructor, arguments);
            return true;
        }

        private Dictionary<string, SerializableMember> BuildConstructorMemberLookup(List<SerializableMember> ctorMembers)
        {
            Dictionary<string, SerializableMember> lookup = new(StringComparer.OrdinalIgnoreCase);
            foreach (SerializableMember member in ctorMembers)
            {
                lookup[member.Name] = member;
                lookup[member.SerializationName] = member;
            }

            return lookup;
        }

        private string BuildMissingConstructorMessage(Type runtimeType, List<SerializableMember> ctorMembers)
        {
            string parameters = string.Join(", ", ctorMembers.Select(member => $"{this.ToCSharpTypeName(member.MemberType)} {member.SerializationName}"));
            string names = string.Join(", ", ctorMembers.Select(member => member.Name));
            return $"{runtimeType.FullName} 缺少可用于导出的公共构造函数。必须通过构造函数提供的成员: {names}。请补充例如 [BsonConstructor] public {runtimeType.Name}({parameters})";
        }

        private bool TryGetListElementType(Type type, out Type elementType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            elementType = null;
            return false;
        }

        private bool TryGetHashSetElementType(Type type, out Type elementType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            elementType = null;
            return false;
        }

        private bool TryGetDictionaryInfo(Type type, out DictionaryInfo info)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                info = new DictionaryInfo(
                    entrySource =>
                    {
                        List<DictionaryEntryData> entries = new();
                        foreach (DictionaryEntry entry in (IDictionary)entrySource)
                        {
                            entries.Add(new DictionaryEntryData(entry.Key, entry.Value));
                        }

                        return entries;
                    });
                return true;
            }

            Type dictionaryInterface = type.GetInterfaces()
                    .Concat(new[] { type })
                    .FirstOrDefault(current => current.IsGenericType && current.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            if (dictionaryInterface != null)
            {
                PropertyInfo keyProperty = dictionaryInterface.GenericTypeArguments.Length > 0
                    ? dictionaryInterface.GetMethod("GetEnumerator")?.ReturnType.GetProperty("Current")?.PropertyType.GetProperty("Key")
                    : null;

                info = new DictionaryInfo(entrySource =>
                {
                    List<DictionaryEntryData> entries = new();
                    foreach (object item in (IEnumerable)entrySource)
                    {
                        Type itemType = item.GetType();
                        object key = itemType.GetProperty("Key")?.GetValue(item);
                        object val = itemType.GetProperty("Value")?.GetValue(item);
                        entries.Add(new DictionaryEntryData(key, val));
                    }

                    return entries;
                });
                return true;
            }

            info = null;
            return false;
        }

        private string GetStableOrderKey(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            Type type = value.GetType();
            if (TryFormatInlineValue(type, value, out string inlineValue))
            {
                return inlineValue;
            }

            throw new NotSupportedException($"无法为类型 {type.FullName} 生成稳定排序键");
        }

        private bool TryFormatInlineValue(Type type, object value, out string code)
        {
            code = null;

            if (value == null)
            {
                code = "null";
                return true;
            }

            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return this.TryFormatInlineValue(underlyingType, value, out code);
            }

            if (type == typeof(string))
            {
                code = "@\"" + ((string)value).Replace("\"", "\"\"") + "\"";
                return true;
            }

            if (type == typeof(bool))
            {
                code = (bool)value ? "true" : "false";
                return true;
            }

            if (type == typeof(char))
            {
                code = "'" + EscapeChar((char)value) + "'";
                return true;
            }

            if (type == typeof(byte))
            {
                code = ((byte)value).ToString(CultureInfo.InvariantCulture);
                return true;
            }

            if (type == typeof(sbyte))
            {
                code = $"(sbyte){((sbyte)value).ToString(CultureInfo.InvariantCulture)}";
                return true;
            }

            if (type == typeof(short))
            {
                code = $"(short){((short)value).ToString(CultureInfo.InvariantCulture)}";
                return true;
            }

            if (type == typeof(ushort))
            {
                code = $"(ushort){((ushort)value).ToString(CultureInfo.InvariantCulture)}";
                return true;
            }

            if (type == typeof(int))
            {
                code = ((int)value).ToString(CultureInfo.InvariantCulture);
                return true;
            }

            if (type == typeof(uint))
            {
                code = ((uint)value).ToString(CultureInfo.InvariantCulture) + "U";
                return true;
            }

            if (type == typeof(long))
            {
                code = ((long)value).ToString(CultureInfo.InvariantCulture) + "L";
                return true;
            }

            if (type == typeof(ulong))
            {
                code = ((ulong)value).ToString(CultureInfo.InvariantCulture) + "UL";
                return true;
            }

            if (type == typeof(float))
            {
                float number = (float)value;
                code = float.IsNaN(number)
                    ? "float.NaN"
                    : float.IsPositiveInfinity(number)
                        ? "float.PositiveInfinity"
                        : float.IsNegativeInfinity(number)
                            ? "float.NegativeInfinity"
                            : number.ToString("R", CultureInfo.InvariantCulture) + "f";
                return true;
            }

            if (type == typeof(double))
            {
                double number = (double)value;
                code = double.IsNaN(number)
                    ? "double.NaN"
                    : double.IsPositiveInfinity(number)
                        ? "double.PositiveInfinity"
                        : double.IsNegativeInfinity(number)
                            ? "double.NegativeInfinity"
                            : number.ToString("R", CultureInfo.InvariantCulture) + "d";
                return true;
            }

            if (type == typeof(decimal))
            {
                code = ((decimal)value).ToString(CultureInfo.InvariantCulture) + "m";
                return true;
            }

            if (type.IsEnum)
            {
                string enumName = Enum.GetName(type, value);
                if (!string.IsNullOrEmpty(enumName))
                {
                    code = $"{this.ToCSharpTypeName(type)}.{enumName}";
                    return true;
                }

                Type enumUnderlyingType = Enum.GetUnderlyingType(type);
                object numericValue = Convert.ChangeType(value, enumUnderlyingType, CultureInfo.InvariantCulture);
                code = $"({this.ToCSharpTypeName(type)}){Convert.ToString(numericValue, CultureInfo.InvariantCulture)}";
                return true;
            }

            return false;
        }

        private string EscapeChar(char value)
        {
            return value switch
            {
                '\'' => "\\'",
                '\\' => "\\\\",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '\0' => "\\0",
                _ => value.ToString(),
            };
        }

        private string ToCSharpTypeName(Type type)
        {
            if (type == typeof(void)) return "void";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(sbyte)) return "sbyte";
            if (type == typeof(short)) return "short";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(int)) return "int";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(long)) return "long";
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(string)) return "string";
            if (type == typeof(object)) return "object";

            if (type.IsByRef)
            {
                type = type.GetElementType()!;
            }

            if (type.IsArray)
            {
                return $"{this.ToCSharpTypeName(type.GetElementType()!)}[]";
            }

            if (type.IsGenericType)
            {
                string genericTypeName = type.GetGenericTypeDefinition().FullName!;
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                Type[] args = type.GetGenericArguments();
                return $"global::{genericTypeName}<{string.Join(", ", Array.ConvertAll(args, this.ToCSharpTypeName))}>";
            }

            if (type.IsNested && type.DeclaringType != null)
            {
                return $"{this.ToCSharpTypeName(type.DeclaringType)}.{type.Name}";
            }

            return $"global::{type.FullName}";
        }

        private string GetConstructorTypeName(Type runtimeType, ConstructorInfo constructor)
        {
            return $"new {this.ToCSharpTypeName(runtimeType)}";
        }

        private object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private bool RuntimeMemberExists(Type declaringType, string memberName, MemberKind kind)
        {
            if (!this.runtimeTypes.TryGetValue(declaringType.FullName ?? string.Empty, out Type runtimeType))
            {
                return true;
            }

            return kind switch
            {
                MemberKind.Field => runtimeType.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) != null,
                MemberKind.Property => runtimeType.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) != null,
                _ => true,
            };
        }

        private Dictionary<string, Type> LoadRuntimeTypes()
        {
            Dictionary<string, Type> result = new(StringComparer.Ordinal);
            foreach (Assembly assembly in this.LoadRuntimeAssemblies())
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.FullName == null)
                        {
                            continue;
                        }

                        result[type.FullName] = type;
                    }
                }
                catch
                {
                }
            }

            return result;
        }

        private IEnumerable<Assembly> LoadRuntimeAssemblies()
        {
            HashSet<string> loadedNames = new(StringComparer.OrdinalIgnoreCase);
            List<Assembly> assemblies = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                if (name is not ("ET.Core" or "ET.Model" or "ET.Hotfix"))
                {
                    continue;
                }

                if (loadedNames.Add(assembly.FullName))
                {
                    assemblies.Add(assembly);
                }
            }

            string projectRoot = this.FindProjectRoot();
            if (string.IsNullOrEmpty(projectRoot))
            {
                return assemblies;
            }

            string[] assemblyPaths =
            {
                Path.Combine(projectRoot, "Bin/ET.Core.dll"),
                Path.Combine(projectRoot, "Bin/ET.Model.dll"),
                Path.Combine(projectRoot, "Bin/ET.Hotfix.dll"),
            };

            foreach (string assemblyPath in assemblyPaths)
            {
                if (!File.Exists(assemblyPath))
                {
                    continue;
                }

                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    if (loadedNames.Add(assembly.FullName))
                    {
                        assemblies.Add(assembly);
                    }
                }
                catch
                {
                }
            }

            return assemblies;
        }

        private string FindProjectRoot()
        {
            DirectoryInfo current = new(Directory.GetCurrentDirectory());
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, "ET.sln")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            return null;
        }

        private bool IsUnityObject(Type type)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                if (current.FullName == "UnityEngine.Object")
                {
                    return true;
                }
            }

            return false;
        }

        private string Indent(int level)
        {
            return new string(' ', level * 4);
        }

        [EnableClass]
        private sealed class EmitterContext
        {
            private readonly HashSet<object> visited = new(new ObjectReferenceComparer());

            public void Enter(object value, Type runtimeType)
            {
                if (!this.visited.Add(value))
                {
                    throw new NotSupportedException($"检测到循环引用，类型: {runtimeType.FullName}");
                }
            }

            public void Exit(object value)
            {
                this.visited.Remove(value);
            }
        }

        [EnableClass]
        private sealed class ObjectReferenceComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        [EnableClass]
        private sealed class SerializableMember
        {
            public SerializableMember(
                string name,
                string serializationName,
                Type memberType,
                bool requiresConstructor,
                bool ignoreIfNull,
                bool ignoreIfDefault,
                object defaultValue,
                Func<object, object> getter)
            {
                this.Name = name;
                this.SerializationName = serializationName;
                this.MemberType = memberType;
                this.RequiresConstructor = requiresConstructor;
                this.IgnoreIfNull = ignoreIfNull;
                this.IgnoreIfDefault = ignoreIfDefault;
                this.DefaultValue = defaultValue;
                this.getter = getter;
            }

            private readonly Func<object, object> getter;

            public string Name { get; }

            public string SerializationName { get; }

            public Type MemberType { get; }

            public bool RequiresConstructor { get; }

            public bool IgnoreIfNull { get; }

            public bool IgnoreIfDefault { get; }

            public object DefaultValue { get; }

            public object GetValue(object owner)
            {
                return this.getter(owner);
            }
        }

        [EnableClass]
        private sealed class ConstructorArgument
        {
            public ConstructorArgument(ParameterInfo parameter, SerializableMember member)
            {
                this.Parameter = parameter;
                this.Member = member;
            }

            public ParameterInfo Parameter { get; }

            public SerializableMember Member { get; }
        }

        [EnableClass]
        private sealed class ConstructorMatch
        {
            public ConstructorMatch(ConstructorInfo constructor, List<ConstructorArgument> arguments)
            {
                this.Constructor = constructor;
                this.Arguments = arguments;
            }

            public ConstructorInfo Constructor { get; }

            public List<ConstructorArgument> Arguments { get; }
        }

        [EnableClass]
        private sealed class DictionaryInfo
        {
            public DictionaryInfo(Func<object, List<DictionaryEntryData>> getEntries)
            {
                this.GetEntries = getEntries;
            }

            public Func<object, List<DictionaryEntryData>> GetEntries { get; }
        }

        [EnableClass]
        private sealed class DictionaryEntryData
        {
            public DictionaryEntryData(object key, object value)
            {
                this.Key = key;
                this.Value = value;
            }

            public object Key { get; }

            public object Value { get; }
        }

        private enum MemberKind
        {
            Field,
            Property,
        }
    }
}
