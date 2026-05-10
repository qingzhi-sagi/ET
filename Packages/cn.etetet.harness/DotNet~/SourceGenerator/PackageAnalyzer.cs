using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET
{
    /// <summary>
    /// ET包依赖分析器 - 检查包之间的访问权限和依赖关系
    /// 
    /// 【设计原理】：
    /// 每个分析器实例独立工作，通过ProjectReference机制可以直接访问所有依赖程序集的符号
    /// 分析器实例从当前编译上下文中获取所有符号（包括依赖程序集），构建自己的符号表
    /// 利用ISymbol.Locations获取源码位置，从文件路径中提取包名信息
    /// 
    /// 【核心优势】：
    /// - 每个分析器实例职责单一，避免全局状态复杂性
    /// - 直接利用编译器提供的符号信息，无需跨实例共享
    /// - 内存使用更高效，生命周期管理更简单
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PackageAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor FieldAccessForbiddenDescriptor = new DiagnosticDescriptor(
            "ET0101",
            "Field access forbidden",
            "Package '{0}' cannot access field '{1}' from package '{2}'",
            "Package",
            DiagnosticSeverity.Error,
            true
        );

        private static readonly DiagnosticDescriptor MethodCallForbiddenDescriptor = new DiagnosticDescriptor(
            "ET0102",
            "Method call forbidden",
            "Package '{0}' cannot call method '{1}' from package '{2}'",
            "Package",
            DiagnosticSeverity.Error,
            true
        );

        private static readonly DiagnosticDescriptor TypeAccessForbiddenDescriptor = new DiagnosticDescriptor(
            "ET0105",
            "Type access forbidden",
            "Package '{0}' cannot access type '{1}' from package '{2}'",
            "Package",
            DiagnosticSeverity.Error,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            MethodCallForbiddenDescriptor,
            TypeAccessForbiddenDescriptor,
            DebugInfoDescriptor
        );

        // 每个分析器实例独立的符号表
        private readonly Dictionary<string, string> _symbolTable = new();
        private readonly Dictionary<string, HashSet<string>> _symbolPackageCandidates = new();
        private readonly HashSet<string> _partialTypeNames = new();
        private readonly Dictionary<string, PackageInfo> _packageInfos = new();
        
        // 初始化锁，确保符号表构建完成后再开始分析
        private readonly object _initLock = new object();
        
        // 【性能优化】：预计算的扁平化依赖关系，O(1)查找
        private readonly Dictionary<string, HashSet<string>> _flatDependencies = new();
        
        // 【性能优化】：预编译正则表达式
        private static readonly Regex DependenciesRegex = new(
            @"""dependencies""\s*:\s*\{([^}]*)\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);
            
        private static readonly Regex PackageNameRegex = new(
            @"""(cn\.etetet\.[^""]+)""\s*:\s*""[^""]*""",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // 【分析器日志】：用于调试的诊断报告
        private static readonly DiagnosticDescriptor DebugInfoDescriptor = new DiagnosticDescriptor(
            "ET9999",
            "PackageAnalyzer Debug Info",
            "Debug: {0}",
            "Debug",
            DiagnosticSeverity.Info,
            true
        );

        /// <summary>
        /// 【改进】：在分析器中输出调试信息的正确方式
        /// </summary>
        private void LogDebugInfo(SyntaxNodeAnalysisContext context, string message)
        {
#if DEBUG
            // 方案1：通过诊断报告输出（会在编译输出中显示）
            var diagnostic = Diagnostic.Create(
                DebugInfoDescriptor,
                Location.None,
                message
            );
            context.ReportDiagnostic(diagnostic);
#endif
        }

        /// <summary>
        /// 【简化】：只使用临时目录写入日志
        /// </summary>
        private static void LogToFile(string message)
        {
#if DEBUG
            try
            {
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                var tempLogPath = Path.Combine(Path.GetTempPath(), "PackageAnalyzer.log");
                File.AppendAllText(tempLogPath, logMessage);
            }
            catch
            {
                // 静默失败，避免影响分析器正常工作
            }
#endif
        }


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(InitializeCompilation);
        }

        private void InitializeCompilation(CompilationStartAnalysisContext context)
        {
            // 【关键过滤】：只处理AnalyzeAssembly.All中定义的程序集
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.All))
            {
                return;
            }
            
            
            // 确保符号表完全构建完成后再注册分析动作
            lock (_initLock)
            {
                _symbolTable.Clear();
                _symbolPackageCandidates.Clear();
                _packageInfos.Clear();
                _flatDependencies.Clear();
                _processedFiles.Clear();
                _partialTypeNames.Clear();

                LoadPackageInfos(context.Compilation);
                
                // 构建当前编译上下文的符号表
                BuildSymbolTable(context.Compilation);
            }

            var snapshot = CreateSnapshot();
            context.RegisterSyntaxNodeAction(ctx => AnalyzeInvocationExpression(ctx, snapshot), SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(ctx => AnalyzeTypeReferenceContainer(ctx, snapshot),
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.VariableDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.Parameter,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.BaseList,
                SyntaxKind.TypeArgumentList,
                SyntaxKind.CastExpression,
                SyntaxKind.TypeOfExpression,
                SyntaxKind.DefaultExpression,
                SyntaxKind.ForEachStatement,
                SyntaxKind.DeclarationPattern,
                SyntaxKind.CatchDeclaration);
        }

        // 【修复重复处理】：记录已处理的文件路径，避免重复处理
        private readonly HashSet<string> _processedFiles = new();

        private sealed class AnalysisSnapshot
        {
            public Dictionary<string, string> SymbolTable { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, HashSet<string>> SymbolPackageCandidates { get; set; } = new Dictionary<string, HashSet<string>>();
            public HashSet<string> PartialTypeNames { get; set; } = new HashSet<string>();
            public Dictionary<string, PackageInfo> PackageInfos { get; set; } = new Dictionary<string, PackageInfo>();
            public Dictionary<string, HashSet<string>> FlatDependencies { get; set; } = new Dictionary<string, HashSet<string>>();
        }

        private AnalysisSnapshot CreateSnapshot()
        {
            return new AnalysisSnapshot
            {
                SymbolTable = new Dictionary<string, string>(_symbolTable),
                SymbolPackageCandidates = _symbolPackageCandidates.ToDictionary(
                    kv => kv.Key,
                    kv => new HashSet<string>(kv.Value)),
                PartialTypeNames = new HashSet<string>(_partialTypeNames),
                PackageInfos = new Dictionary<string, PackageInfo>(_packageInfos),
                FlatDependencies = _flatDependencies.ToDictionary(
                    kv => kv.Key,
                    kv => new HashSet<string>(kv.Value))
            };
        }

        private void RecordSymbolOwnership(string symbolKey, string packageName)
        {
            _symbolTable[symbolKey] = packageName;

            if (!_symbolPackageCandidates.TryGetValue(symbolKey, out var candidatePackages))
            {
                candidatePackages = new HashSet<string>();
                _symbolPackageCandidates[symbolKey] = candidatePackages;
            }

            candidatePackages.Add(packageName);
        }
        
        private void BuildSymbolTable(Compilation compilation)
        {
            try
            {
                _processedFiles.Clear();
                _partialTypeNames.Clear();
                // 处理当前编译的语法树
                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    if (syntaxTree.FilePath != null)
                    {
                        if (IsInTestScope(syntaxTree.FilePath))
                        {
                            continue;
                        }
                        var packageName = ExtractPackageNameFromFilePath(syntaxTree.FilePath);
                        if (!string.IsNullOrEmpty(packageName))
                        {
                            WriteSymbolsFromSyntaxTree(syntaxTree, packageName!);
                        }
                    }
                }

                // 处理仓库源码中的类型和成员符号。
                // 覆盖 MetadataReference 场景下的类型/字段/属性/方法归属。
                // Core/Loader 只作为被访问符号的归属来源，不作为包访问检查范围。
                BuildRepositoryMemberSymbolsFromSources(compilation);
                
                // 处理依赖程序集中的符号（通过ProjectReference可访问）
                BuildSymbolsFromReferencedAssemblies(compilation);
            }
            catch (Exception ex)
            {
                // 【改进错误处理】：使用分析器专用日志方式
                LogToFile($"BuildSymbolTable error: {ex.Message}");
#if DEBUG
                LogToFile($"Stack trace: {ex.StackTrace}");
#endif
            }
        }

        private void BuildRepositoryMemberSymbolsFromSources(Compilation compilation)
        {
            try
            {
                string? repoRoot = GetRepositoryRoot(compilation);
                if (string.IsNullOrEmpty(repoRoot))
                {
                    return;
                }

                string packagesRoot = Path.Combine(repoRoot, "Packages");
                if (!Directory.Exists(packagesRoot))
                {
                    return;
                }

                foreach (string filePath in Directory.EnumerateFiles(packagesRoot, "*.cs", SearchOption.AllDirectories))
                {
                    string normalizedPath = filePath.Replace("\\", "/");
                    if (!normalizedPath.Contains("/Packages/cn.etetet."))
                    {
                        continue;
                    }

                    if (!IsRepositorySymbolSourceScope(normalizedPath))
                    {
                        continue;
                    }

                    if (IsInTestScope(normalizedPath))
                    {
                        continue;
                    }

                    string? packageName = ExtractPackageNameFromFilePath(filePath);
                    if (string.IsNullOrEmpty(packageName))
                    {
                        continue;
                    }

                    var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), path: filePath);
                    WriteSymbolsFromSyntaxTree(syntaxTree, packageName!);
                }
            }
            catch (Exception ex)
            {
                LogToFile($"BuildRepositoryMemberSymbolsFromSources error: {ex.Message}");
            }
        }

        private string? GetRepositoryRoot(Compilation compilation)
        {
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                if (string.IsNullOrEmpty(syntaxTree.FilePath))
                {
                    continue;
                }

                string normalizedPath = syntaxTree.FilePath.Replace("\\", "/");
                int packagesIndex = normalizedPath.IndexOf("/Packages/", StringComparison.Ordinal);
                if (packagesIndex <= 0)
                {
                    continue;
                }

                return normalizedPath.Substring(0, packagesIndex);
            }

            return null;
        }
        
        private void BuildSymbolsFromReferencedAssemblies(Compilation compilation)
        {
            try
            {
                // 获取所有引用的程序集
                foreach (var reference in compilation.References)
                {
                    if (reference is CompilationReference compRef)
                    {
                        // 这是ProjectReference，可以访问源码
                        var referencedCompilation = compRef.Compilation;
                        foreach (var syntaxTree in referencedCompilation.SyntaxTrees)
                        {
                            if (syntaxTree.FilePath != null)
                            {
                                var packageName = ExtractPackageNameFromFilePath(syntaxTree.FilePath);
                                if (!string.IsNullOrEmpty(packageName))
                                {
                                    WriteSymbolsFromSyntaxTree(syntaxTree, packageName!);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 【改进错误处理】：记录错误但继续执行
                LogToFile($"BuildSymbolsFromReferencedAssemblies error: {ex.Message}");
            }
        }

        private string? ExtractPackageNameFromFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            
            var normalizedPath = filePath.Replace("\\", "/");
            
            if (normalizedPath.Contains("Packages"))
            {
                var packageStart = normalizedPath.IndexOf("Packages") + 9;
                var packageEnd = normalizedPath.IndexOf("/", packageStart);
                if (packageEnd > packageStart)
                {
                    var packageName = normalizedPath.Substring(packageStart, packageEnd - packageStart);
                    if (packageName.StartsWith("cn.etetet."))
                    {
                        return packageName;
                    }
                }
            }
            
            return null;
        }

        private void WriteSymbolsFromSyntaxTree(SyntaxTree syntaxTree, string packageName)
        {
            try
            {
                var root = syntaxTree.GetRoot();
                
                
                // 【关键修复】：只记录在当前文件中**定义**的符号，不记录使用的符号
                
                // 处理所有类型声明
                foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    if (IsInETNamespace(typeDecl))
                    {
                        var typeName = typeDecl.Identifier.ValueText;
                        var namespaceName = GetNamespaceName(typeDecl);
                        var fullTypeName = string.IsNullOrEmpty(namespaceName) ? typeName : $"{namespaceName}.{typeName}";
                        
                        if (!string.IsNullOrEmpty(typeName))
                        {
                            // 【修复partial类问题】：对于partial类，只有主要定义才记录到符号表
                            // 检查是否为partial类
                            bool isPartial = typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
                            if (isPartial)
                            {
                                _partialTypeNames.Add(fullTypeName);
                            }
                             
                            if (isPartial)
                            {
                                // 【特殊处理partial static class】：如PackageType, MailBoxType等
                                // 这些类的成员（常量字段）应该归属到定义它们的包，而不是某个"主要"包
                                if (IsSharedConstantContainer(typeName))
                                {
                                    // 共享常量容器视为全局共享，不参与包成员归属检查
                                    continue;
                                }
                                
                                // 对于普通partial类，只有主要包才能拥有该类型
                                // 判断标准：如果该类型已经在符号表中存在，且不是当前包，则跳过类型记录
                                if (_symbolTable.TryGetValue(fullTypeName, out string existingPackage) && existingPackage != packageName)
                                {
                                    // 已经有其他包定义了这个partial类，跳过类型记录
                                    // 但仍需要处理成员，且成员归属到当前包（定义成员的包）
                                    ProcessPartialClassMembers(typeDecl, fullTypeName, packageName);
                                    continue;
                                }
                            }
                            
                            // 记录完整类型名
                            RecordSymbolOwnership(fullTypeName, packageName);
                            
                            // 处理成员
                            foreach (var member in typeDecl.Members)
                            {
                                switch (member)
                                {
                                    case FieldDeclarationSyntax fieldDecl:
                                        foreach (var variable in fieldDecl.Declaration.Variables)
                                        {
                                            var fieldName = variable.Identifier.ValueText;
                                            if (!string.IsNullOrEmpty(fieldName))
                                            {
                                                var fieldKey = $"{fullTypeName}.{fieldName}";
                                                RecordSymbolOwnership(fieldKey, packageName);
                                            }
                                        }
                                        break;
                                    case MethodDeclarationSyntax methodDecl:
                                        var methodName = methodDecl.Identifier.ValueText;
                                        if (!string.IsNullOrEmpty(methodName))
                                        {
                                            // 构建完整的方法签名，包含泛型参数和方法参数
                                            var methodSignature = GetMethodSignature(fullTypeName, methodDecl);
                                            RecordMethodOwnership(fullTypeName, methodName, methodSignature, packageName);
                                        }
                                        break;
                                    case PropertyDeclarationSyntax propertyDecl:
                                        var propertyName = propertyDecl.Identifier.ValueText;
                                        if (!string.IsNullOrEmpty(propertyName))
                                        {
                                            var propertyKey = $"{fullTypeName}.{propertyName}";
                                            RecordSymbolOwnership(propertyKey, packageName);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                
                // 处理枚举
                foreach (var enumDecl in root.DescendantNodes().OfType<EnumDeclarationSyntax>())
                {
                    if (IsInETNamespace(enumDecl))
                    {
                        var enumName = enumDecl.Identifier.ValueText;
                        var namespaceName = GetNamespaceName(enumDecl);
                        var fullEnumName = string.IsNullOrEmpty(namespaceName) ? enumName : $"{namespaceName}.{enumName}";
                        
                        if (!string.IsNullOrEmpty(enumName))
                        {
                            RecordSymbolOwnership(fullEnumName, packageName);
                            
                            foreach (var enumMember in enumDecl.Members)
                            {
                                var memberName = enumMember.Identifier.ValueText;
                                if (!string.IsNullOrEmpty(memberName))
                                {
                                    // 使用完整的枚举成员名（Namespace.EnumName.MemberName）
                                    var fullMemberName = $"{fullEnumName}.{memberName}";
                                    RecordSymbolOwnership(fullMemberName, packageName);
                                }
                            }
                        }
                    }
                }
                
                // 处理委托
                foreach (var delegateDecl in root.DescendantNodes().OfType<DelegateDeclarationSyntax>())
                {
                    if (IsInETNamespace(delegateDecl))
                    {
                        var delegateName = delegateDecl.Identifier.ValueText;
                        var namespaceName = GetNamespaceName(delegateDecl);
                        var fullDelegateName = string.IsNullOrEmpty(namespaceName) ? delegateName : $"{namespaceName}.{delegateName}";
                        
                        if (!string.IsNullOrEmpty(delegateName))
                        {
                            RecordSymbolOwnership(fullDelegateName, packageName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 【改进错误处理】：记录错误但继续执行
                LogToFile($"WriteSymbolsFromSyntaxTree error in file {syntaxTree.FilePath}: {ex.Message}");
            }
        }

        private void WriteMemberSymbolsFromSyntaxTree(SyntaxTree syntaxTree, string packageName)
        {
            try
            {
                var root = syntaxTree.GetRoot();

                foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    if (!IsInETNamespace(typeDecl))
                    {
                        continue;
                    }

                    var typeName = typeDecl.Identifier.ValueText;
                    var namespaceName = GetNamespaceName(typeDecl);
                    var fullTypeName = string.IsNullOrEmpty(namespaceName) ? typeName : $"{namespaceName}.{typeName}";
                    bool isPartial = typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
                    if (isPartial)
                    {
                        _partialTypeNames.Add(fullTypeName);
                    }

                    if (IsSharedConstantContainer(typeName))
                    {
                        continue;
                    }

                    foreach (var member in typeDecl.Members)
                    {
                        switch (member)
                        {
                            case FieldDeclarationSyntax fieldDecl:
                                foreach (var variable in fieldDecl.Declaration.Variables)
                                {
                                    var fieldName = variable.Identifier.ValueText;
                                    if (!string.IsNullOrEmpty(fieldName))
                                    {
                                        RecordSymbolOwnership($"{fullTypeName}.{fieldName}", packageName);
                                    }
                                }
                                break;
                            case PropertyDeclarationSyntax propertyDecl:
                                var propertyName = propertyDecl.Identifier.ValueText;
                                if (!string.IsNullOrEmpty(propertyName))
                                {
                                    RecordSymbolOwnership($"{fullTypeName}.{propertyName}", packageName);
                                }
                                break;
                            case MethodDeclarationSyntax methodDecl:
                                var methodName = methodDecl.Identifier.ValueText;
                                if (!string.IsNullOrEmpty(methodName))
                                {
                                    var methodSignature = GetMethodSignature(fullTypeName, methodDecl);
                                    RecordMethodOwnership(fullTypeName, methodName, methodSignature, packageName);
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFile($"WriteMemberSymbolsFromSyntaxTree error in file {syntaxTree.FilePath}: {ex.Message}");
            }
        }

        private static bool IsSharedConstantContainer(string typeName)
        {
            return typeName == "PackageType" ||
                   typeName == "MailBoxType" ||
                   typeName == "SceneType" ||
                   typeName == "LocationType" ||
                   typeName == "TimerInvokeType" ||
                   typeName == "ErrorCode" ||
                   typeName == "ConsoleMode";
        }

        private static bool IsInTestScope(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            string normalizedPath = filePath.Replace("\\", "/");
            return normalizedPath.Contains("/Scripts/Hotfix/Test/") ||
                   normalizedPath.Contains("/Scripts/Model/Test/");
        }


        private bool IsInETNamespace(SyntaxNode node)
        {
            var parent = node.Parent;
            while (parent != null)
            {
                if (parent is NamespaceDeclarationSyntax ns)
                {
                    var namespaceName = ns.Name.ToString();
                    return namespaceName == "ET" || namespaceName.StartsWith("ET.");
                }
                parent = parent.Parent;
            }
            return false;
        }

        private string GetNamespaceName(SyntaxNode node)
        {
            var parent = node.Parent;
            while (parent != null)
            {
                if (parent is NamespaceDeclarationSyntax ns)
                {
                    return ns.Name.ToString();
                }
                parent = parent.Parent;
            }
            return string.Empty;
        }

        /// <summary>
        /// 处理partial static类的成员，将成员归属到当前包
        /// </summary>
        private void ProcessPartialStaticClassMembers(TypeDeclarationSyntax typeDecl, string fullTypeName, string currentPackage)
        {
            // 对于partial static类（如PackageType），每个包的成员都归属到当前包
            foreach (var member in typeDecl.Members)
            {
                switch (member)
                {
                    case FieldDeclarationSyntax fieldDecl:
                        foreach (var variable in fieldDecl.Declaration.Variables)
                        {
                            var fieldName = variable.Identifier.ValueText;
                            if (!string.IsNullOrEmpty(fieldName))
                            {
                                var fieldKey = $"{fullTypeName}.{fieldName}";
                                RecordSymbolOwnership(fieldKey, currentPackage);
                            }
                        }
                        break;
                    case PropertyDeclarationSyntax propertyDecl:
                        var propertyName = propertyDecl.Identifier.ValueText;
                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            var propertyKey = $"{fullTypeName}.{propertyName}";
                            RecordSymbolOwnership(propertyKey, currentPackage);
                        }
                        break;
                    case MethodDeclarationSyntax methodDecl:
                        var methodName = methodDecl.Identifier.ValueText;
                        if (!string.IsNullOrEmpty(methodName))
                        {
                            var methodSignature = GetMethodSignature(fullTypeName, methodDecl);
                            RecordMethodOwnership(fullTypeName, methodName, methodSignature, currentPackage);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 处理partial类的成员，将成员归属到定义它们的包
        /// </summary>
        private void ProcessPartialClassMembers(TypeDeclarationSyntax typeDecl, string fullTypeName, string memberPackage)
        {
            // 处理partial类的成员，将它们归属到定义成员的包
            foreach (var member in typeDecl.Members)
            {
                switch (member)
                {
                    case FieldDeclarationSyntax fieldDecl:
                        foreach (var variable in fieldDecl.Declaration.Variables)
                        {
                            var fieldName = variable.Identifier.ValueText;
                            if (!string.IsNullOrEmpty(fieldName))
                            {
                                var fieldKey = $"{fullTypeName}.{fieldName}";
                                RecordSymbolOwnership(fieldKey, memberPackage);
                            }
                        }
                        break;
                    case MethodDeclarationSyntax methodDecl:
                        var methodName = methodDecl.Identifier.ValueText;
                        if (!string.IsNullOrEmpty(methodName))
                        {
                            var methodSignature = GetMethodSignature(fullTypeName, methodDecl);
                            RecordMethodOwnership(fullTypeName, methodName, methodSignature, memberPackage);
                        }
                        break;
                    case PropertyDeclarationSyntax propertyDecl:
                        var propertyName = propertyDecl.Identifier.ValueText;
                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            var propertyKey = $"{fullTypeName}.{propertyName}";
                            RecordSymbolOwnership(propertyKey, memberPackage);
                        }
                        break;
                }
            }
        }

        private string GetMethodSignature(string fullTypeName, MethodDeclarationSyntax methodDecl)
        {
            var methodName = methodDecl.Identifier.ValueText;
            var signature = $"{fullTypeName}.{methodName}";
            
            // 添加泛型参数
            if (methodDecl.TypeParameterList != null && methodDecl.TypeParameterList.Parameters.Count > 0)
            {
                var genericParams = string.Join(",", methodDecl.TypeParameterList.Parameters.Select(p => p.Identifier.ValueText));
                signature += $"<{genericParams}>";
            }
            
            // 添加方法参数
            var parameters = methodDecl.ParameterList.Parameters;
            if (parameters.Count > 0)
            {
                var paramTypes = parameters.Select(p => {
                    var typeName = p.Type?.ToString() ?? "unknown";
                    return typeName;
                });
                signature += $"({string.Join(",", paramTypes)})";
            }
            else
            {
                signature += "()";
            }
            
            return signature;
        }

        private void RecordMethodOwnership(string fullTypeName, string methodName, string methodSignature, string packageName)
        {
            RecordSymbolOwnership(methodSignature, packageName);
            RecordSymbolOwnership(GetMethodNameKey(fullTypeName, methodName), packageName);
        }

        private static string GetMethodNameKey(string fullTypeName, string methodName)
        {
            return $"{fullTypeName}.{methodName}";
        }

        private string GetMethodSignatureFromSymbol(IMethodSymbol methodSymbol)
        {
            var fullTypeName = methodSymbol.ContainingType.ToDisplayString();
            var methodName = methodSymbol.Name;
            var signature = $"{fullTypeName}.{methodName}";
            
            // 添加泛型参数
            if (methodSymbol.TypeParameters.Length > 0)
            {
                var genericParams = string.Join(",", methodSymbol.TypeParameters.Select(tp => tp.Name));
                signature += $"<{genericParams}>";
            }
            
            // 添加方法参数
            if (methodSymbol.Parameters.Length > 0)
            {
                var paramTypes = methodSymbol.Parameters.Select(p => p.Type.ToDisplayString());
                signature += $"({string.Join(",", paramTypes)})";
            }
            else
            {
                signature += "()";
            }
            
            return signature;
        }

        private void LoadPackageInfos(Compilation compilation)
        {
            try
            {
                // 从编译上下文的文件路径中找到项目根目录
                var packagesPath = FindPackagesPathFromCompilation(compilation);
                if (string.IsNullOrEmpty(packagesPath)) 
                {
                    return;
                }

                var packageDirs = Directory.GetDirectories(packagesPath, "cn.etetet.*");
                
                foreach (var packageDir in packageDirs)
                {
                    var packageName = Path.GetFileName(packageDir);
                    var packageJsonPath = Path.Combine(packageDir, "package.json");
                    var packageGitJsonPath = Path.Combine(packageDir, "packagegit.json");

                    if (File.Exists(packageJsonPath))
                    {
                        var packageInfo = LoadPackageInfo(packageJsonPath, packageGitJsonPath);
                        if (packageInfo != null)
                        {
                            _packageInfos[packageName] = packageInfo;
                        }
                    }
                }
                
                // 【性能优化】：预计算所有包的完整依赖集合
                PrecomputeAllDependenciesFlat();
            }
            catch (Exception ex)
            {
                // 【改进错误处理】：记录错误但继续执行
                LogToFile($"LoadPackageInfos error: {ex.Message}");
            }
        }

        private string FindPackagesPathFromCompilation(Compilation compilation)
        {
            try
            {
                // 从编译中的语法树文件路径中寻找包含Packages目录的项目根目录
                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    if (syntaxTree.FilePath != null)
                    {
                        var filePath = syntaxTree.FilePath.Replace("\\", "/");
                        
                        // 寻找包含Packages的路径结构
                        if (filePath.Contains("/Packages/"))
                        {
                            var packagesIndex = filePath.IndexOf("/Packages/");
                            var projectRoot = filePath.Substring(0, packagesIndex);
                            var packagesPath = Path.Combine(projectRoot, "Packages");
                            
                            if (Directory.Exists(packagesPath))
                            {
                                return packagesPath;
                            }
                        }
                    }
                }
                
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private PackageInfo? LoadPackageInfo(string packageJsonPath, string packageGitJsonPath)
        {
            try
            {
                var packageJsonContent = File.ReadAllText(packageJsonPath);
                var packageName = ExtractJsonStringValue(packageJsonContent, "name");
                if (string.IsNullOrEmpty(packageName))
                    return null;

                var packageInfo = new PackageInfo
                {
                    Name = packageName,
                    Dependencies = new List<string>(),
                    Level = 0
                };

                var dependencies = ExtractJsonObjectKeys(packageJsonContent);
                packageInfo.Dependencies.AddRange(dependencies);

                // 读取 packagegit.json 文件获取层级配置
                if (File.Exists(packageGitJsonPath))
                {
                    var packageGitContent = File.ReadAllText(packageGitJsonPath);
                    var levelStr = ExtractJsonStringValue(packageGitContent, "Level");
                    if (int.TryParse(levelStr, out int level))
                    {
                        packageInfo.Level = level;
                    }
                }

                return packageInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 【优化】：改进JSON值提取的错误处理
        /// </summary>
        private string ExtractJsonStringValue(string json, string key)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
                return string.Empty;
                
            try
            {
                var pattern = $@"""{key}""\s*:\s*""([^""]+)""|""{key}""\s*:\s*([0-9]+)|""{key}""\s*:\s*(true|false)";
                var match = Regex.Match(json, pattern);
                if (match.Success)
                {
                    return match.Groups[1].Success ? match.Groups[1].Value : 
                           match.Groups[2].Success ? match.Groups[2].Value : 
                           match.Groups[3].Value;
                }
            }
            catch (Exception ex)
            {
                LogToFile($"ExtractJsonStringValue error for key '{key}': {ex.Message}");
            }
            
            return string.Empty;
        }

        /// <summary>
        /// 【优化】：使用预编译正则表达式提取依赖包列表
        /// </summary>
        private List<string> ExtractJsonObjectKeys(string json)
        {
            var keys = new List<string>();
            
            if (string.IsNullOrEmpty(json))
                return keys;
            
            try
            {
                // 使用预编译的正则表达式
                var dependenciesMatch = DependenciesRegex.Match(json);
                
                if (dependenciesMatch.Success)
                {
                    var dependenciesContent = dependenciesMatch.Groups[1].Value;
                    var matches = PackageNameRegex.Matches(dependenciesContent);
                    
                    foreach (Match match in matches)
                    {
                        var packageName = match.Groups[1].Value.Trim();
                        if (!string.IsNullOrEmpty(packageName))
                        {
                            keys.Add(packageName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFile($"ExtractJsonObjectKeys error: {ex.Message}");
            }
            
            return keys;
        }

        private void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context, AnalysisSnapshot snapshot)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                CheckMethodCall(context, invocation, methodSymbol, snapshot);
            }
        }

        private void AnalyzeTypeReferenceContainer(SyntaxNodeAnalysisContext context, AnalysisSnapshot snapshot)
        {
            foreach (var typeSyntax in GetTypeSyntaxesToAnalyze(context.Node))
            {
                AnalyzeTypeSyntax(context, typeSyntax, snapshot);
            }
        }

        private void AnalyzeTypeSyntax(SyntaxNodeAnalysisContext context, TypeSyntax typeSyntax, AnalysisSnapshot snapshot)
        {
            foreach (var simpleName in typeSyntax.DescendantNodesAndSelf().OfType<SimpleNameSyntax>())
            {
                if (!IsTypeReferenceName(simpleName))
                {
                    continue;
                }

                var typeSymbol = GetReferencedTypeSymbol(context.SemanticModel, simpleName);
                if (typeSymbol != null)
                {
                    CheckTypeAccess(context, simpleName, typeSymbol, snapshot);
                }
            }
        }

        private static IEnumerable<TypeSyntax> GetTypeSyntaxesToAnalyze(SyntaxNode node)
        {
            switch (node)
            {
                case ObjectCreationExpressionSyntax objectCreation:
                    yield return objectCreation.Type;
                    yield break;
                case VariableDeclarationSyntax variableDeclaration:
                    yield return variableDeclaration.Type;
                    yield break;
                case FieldDeclarationSyntax fieldDeclaration:
                    yield return fieldDeclaration.Declaration.Type;
                    yield break;
                case PropertyDeclarationSyntax propertyDeclaration:
                    yield return propertyDeclaration.Type;
                    yield break;
                case ParameterSyntax parameterSyntax when parameterSyntax.Type != null:
                    yield return parameterSyntax.Type;
                    yield break;
                case MethodDeclarationSyntax methodDeclaration:
                    yield return methodDeclaration.ReturnType;
                    yield break;
                case BaseListSyntax baseList:
                    foreach (var baseType in baseList.Types)
                    {
                        yield return baseType.Type;
                    }
                    yield break;
                case TypeArgumentListSyntax typeArgumentList:
                    foreach (var typeArgument in typeArgumentList.Arguments)
                    {
                        yield return typeArgument;
                    }
                    yield break;
                case CastExpressionSyntax castExpression:
                    yield return castExpression.Type;
                    yield break;
                case TypeOfExpressionSyntax typeOfExpression:
                    yield return typeOfExpression.Type;
                    yield break;
                case DefaultExpressionSyntax defaultExpression:
                    yield return defaultExpression.Type;
                    yield break;
                case ForEachStatementSyntax forEachStatement:
                    yield return forEachStatement.Type;
                    yield break;
                case DeclarationPatternSyntax declarationPattern:
                    yield return declarationPattern.Type;
                    yield break;
                case CatchDeclarationSyntax catchDeclaration when catchDeclaration.Type != null:
                    yield return catchDeclaration.Type;
                    yield break;
                default:
                    yield break;
            }
        }

        private static bool IsTypeReferenceName(SimpleNameSyntax simpleName)
        {
            if (simpleName.Parent is QualifiedNameSyntax qualifiedName && qualifiedName.Left == simpleName)
            {
                return false;
            }

            if (simpleName.Parent is AliasQualifiedNameSyntax aliasQualifiedName && aliasQualifiedName.Alias == simpleName)
            {
                return false;
            }

            if (simpleName.Parent is NamespaceDeclarationSyntax or FileScopedNamespaceDeclarationSyntax)
            {
                return false;
            }

            // 调用表达式继续走 MethodCallForbidden 诊断，避免对类型接收者重复报错。
            if (simpleName.Parent is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression == simpleName &&
                memberAccess.Parent is InvocationExpressionSyntax)
            {
                return false;
            }

            return true;
        }

        private static ITypeSymbol? GetReferencedTypeSymbol(SemanticModel semanticModel, SyntaxNode node)
        {
            ISymbol? symbol = semanticModel.GetSymbolInfo(node).Symbol;
            if (symbol is IAliasSymbol aliasSymbol)
            {
                symbol = aliasSymbol.Target;
            }

            if (symbol is INamespaceSymbol)
            {
                return null;
            }

            if (symbol is ITypeSymbol directTypeSymbol)
            {
                return directTypeSymbol;
            }

            var typeInfo = semanticModel.GetTypeInfo(node);
            if (typeInfo.Type == null || typeInfo.Type.TypeKind == TypeKind.Error)
            {
                return null;
            }

            return typeInfo.Type;
        }

        private void CheckMethodCall(SyntaxNodeAnalysisContext context, SyntaxNode node, IMethodSymbol methodSymbol, AnalysisSnapshot snapshot)
        {
            var currentPackage = GetPackageFromNamespace(node);
            var targetPackage = GetPackageFromSymbol(methodSymbol, snapshot, currentPackage);
            
            
            if (currentPackage != null && targetPackage != null && !CanAccessSymbol(currentPackage, targetPackage, snapshot))
            {
                
                var diagnostic = Diagnostic.Create(
                    MethodCallForbiddenDescriptor,
                    node.GetLocation(),
                    currentPackage,
                    methodSymbol.Name,
                    targetPackage
                );
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void CheckTypeAccess(SyntaxNodeAnalysisContext context, SyntaxNode node, ITypeSymbol typeSymbol, AnalysisSnapshot snapshot)
        {
            if (IsPartialType(typeSymbol, snapshot))
            {
                return;
            }

            var currentPackage = GetPackageFromNamespace(node);
            var targetPackage = GetPackageFromSymbol(typeSymbol, snapshot, currentPackage);
             
             
            if (currentPackage != null && targetPackage != null && currentPackage != targetPackage)
            {
                if (!CanAccessSymbol(currentPackage, targetPackage, snapshot))
                {
                    
                    var diagnostic = Diagnostic.Create(
                        TypeAccessForbiddenDescriptor,
                        node.GetLocation(),
                        currentPackage,
                        typeSymbol.Name,
                        targetPackage
                    );
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool IsPartialType(ITypeSymbol typeSymbol, AnalysisSnapshot snapshot)
        {
            if (typeSymbol == null)
            {
                return false;
            }

            return snapshot.PartialTypeNames.Contains(typeSymbol.ToDisplayString());
        }

        private string? GetPackageFromNamespace(SyntaxNode node)
        {
            // 【关键修复】：对于partial类，应该根据当前语法树文件的位置来确定包归属
            // 而不是根据符号表中的类型名，因为partial类可能分布在多个包中
            
            if (node.SyntaxTree?.FilePath != null)
            {
                if (IsInTestScope(node.SyntaxTree.FilePath))
                {
                    return null;
                }

                if (!IsInPackageAccessScope(node.SyntaxTree.FilePath))
                {
                    return null;
                }

                var packageFromPath = ExtractPackageNameFromFilePath(node.SyntaxTree.FilePath);
                if (!string.IsNullOrEmpty(packageFromPath))
                {
                    if (!ShouldCheckPackageAccessForPackage(packageFromPath!))
                    {
                        return null;
                    }

                    // 确保这确实是在ET命名空间中
                    var typeDeclaration = node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
                    if (typeDeclaration != null && IsInETNamespace(typeDeclaration))
                    {
                        return packageFromPath;
                    }
                }
            }
            
            // 没有找到类型声明，可能是全局代码，不需要包检查
            return null;
        }

        private static bool IsInPackageAccessScope(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            string normalizedPath = filePath.Replace("\\", "/");
            return IsPackageAccessCheckScope(normalizedPath);
        }

        private static bool IsRepositorySymbolSourceScope(string normalizedPath)
        {
            return normalizedPath.Contains("/Scripts/Core/") ||
                   normalizedPath.Contains("/Scripts/Loader/") ||
                   IsPackageAccessCheckScope(normalizedPath) ||
                   normalizedPath.Contains("/CodeMode/Model/") ||
                   normalizedPath.Contains("/CodeMode/ModelView/") ||
                   normalizedPath.Contains("/CodeMode/Hotfix/") ||
                   normalizedPath.Contains("/CodeMode/HotfixView/");
        }

        private static bool IsPackageAccessCheckScope(string normalizedPath)
        {
            return normalizedPath.Contains("/Scripts/Model/") ||
                   normalizedPath.Contains("/Scripts/ModelView/") ||
                   normalizedPath.Contains("/Scripts/Hotfix/") ||
                   normalizedPath.Contains("/Scripts/HotfixView/");
        }

        private static bool ShouldCheckPackageAccessForPackage(string packageName)
        {
            return packageName != "cn.etetet.core" &&
                   packageName != "cn.etetet.loader";
        }

        private string? ResolvePackageFromKey(string symbolKey, AnalysisSnapshot snapshot, string? currentPackage)
        {
            if (snapshot.SymbolPackageCandidates.TryGetValue(symbolKey, out var candidatePackages) && candidatePackages.Count > 0)
            {
                if (candidatePackages.Count == 1)
                {
                    return candidatePackages.First();
                }

                if (!string.IsNullOrEmpty(currentPackage))
                {
                    string currentPackageName = currentPackage!;

                    if (candidatePackages.Contains(currentPackageName))
                    {
                        return currentPackageName;
                    }

                    if (snapshot.PackageInfos.TryGetValue(currentPackageName, out var currentInfo) && currentInfo.Dependencies != null)
                    {
                        var directMatches = candidatePackages.Where(currentInfo.Dependencies.Contains).ToList();
                        if (directMatches.Count == 1)
                        {
                            return directMatches[0];
                        }
                    }

                    if (snapshot.FlatDependencies.TryGetValue(currentPackageName, out var dependencyPackages))
                    {
                        var flatMatches = candidatePackages.Where(dependencyPackages.Contains).ToList();
                        if (flatMatches.Count == 1)
                        {
                            return flatMatches[0];
                        }
                    }

                }
            }

            if (snapshot.SymbolTable.TryGetValue(symbolKey, out string? resolvedPackage))
            {
                return resolvedPackage;
            }

            return null;
        }

        private string? ResolvePackageFromCandidateKey(string symbolKey, AnalysisSnapshot snapshot, string? currentPackage)
        {
            if (!snapshot.SymbolPackageCandidates.TryGetValue(symbolKey, out var candidatePackages) || candidatePackages.Count == 0)
            {
                return null;
            }

            if (candidatePackages.Count == 1)
            {
                return candidatePackages.First();
            }

            if (string.IsNullOrEmpty(currentPackage))
            {
                return null;
            }

            string currentPackageName = currentPackage!;
            if (candidatePackages.Contains(currentPackageName))
            {
                return currentPackageName;
            }

            if (snapshot.PackageInfos.TryGetValue(currentPackageName, out var currentInfo) && currentInfo.Dependencies != null)
            {
                var directMatches = candidatePackages.Where(currentInfo.Dependencies.Contains).ToList();
                if (directMatches.Count == 1)
                {
                    return directMatches[0];
                }
            }

            if (snapshot.FlatDependencies.TryGetValue(currentPackageName, out var dependencyPackages))
            {
                var flatMatches = candidatePackages.Where(dependencyPackages.Contains).ToList();
                if (flatMatches.Count == 1)
                {
                    return flatMatches[0];
                }
            }

            return null;
        }

        private string? GetPackageFromSymbol(ISymbol symbol, AnalysisSnapshot snapshot, string? currentPackage = null)
        {
            if (symbol == null) return null;
            
            // 检查是否是ET类型
            var namespaceName = symbol.ContainingNamespace?.ToDisplayString();
            if (namespaceName == null || (namespaceName != "ET" && !namespaceName.StartsWith("ET.")))
            {
                return null; // 非ET类型允许自由访问
            }
            
            // 【跳过检查】：泛型类型参数、委托类型、局部函数等不需要包检查
            if (symbol is ITypeParameterSymbol || 
                symbol.Kind == SymbolKind.TypeParameter ||
                symbol.ContainingType?.TypeKind == TypeKind.Delegate)
            {
                return null;
            }
            
            // 【跳过检查】：局部函数不需要包检查
            if (symbol is IMethodSymbol methodSym && methodSym.MethodKind == MethodKind.LocalFunction)
            {
                return null;
            }

            // 统一只依赖分析器自建符号表判定归属，避免 Rider 和 dotnet build
            // 因 symbol.Locations 形态差异导致的包归属不一致。
            if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.ContainingType != null)
            {
                var fullTypeName = fieldSymbol.ContainingType.ToDisplayString();
                var fieldKey = $"{fullTypeName}.{fieldSymbol.Name}";
                var fieldPackage = ResolvePackageFromKey(fieldKey, snapshot, currentPackage);
                if (!string.IsNullOrEmpty(fieldPackage))
                {
                    return fieldPackage;
                }
            }
            
            if (symbol is IMethodSymbol methodSymbol && methodSymbol.ContainingType != null)
            {
                // 构建完整的方法签名进行查找
                var methodSignature = GetMethodSignatureFromSymbol(methodSymbol);
                var methodPackage = ResolvePackageFromKey(methodSignature, snapshot, currentPackage);
                if (!string.IsNullOrEmpty(methodPackage))
                {
                    return methodPackage;
                }

                var fullTypeName = methodSymbol.ContainingType.ToDisplayString();
                var methodNameKey = GetMethodNameKey(fullTypeName, methodSymbol.Name);
                methodPackage = ResolvePackageFromCandidateKey(methodNameKey, snapshot, currentPackage);
                if (!string.IsNullOrEmpty(methodPackage))
                {
                    return methodPackage;
                }
            }
            
            if (symbol is IPropertySymbol propertySymbol && propertySymbol.ContainingType != null)
            {
                var fullTypeName = propertySymbol.ContainingType.ToDisplayString();
                var propertyKey = $"{fullTypeName}.{propertySymbol.Name}";
                var propertyPackage = ResolvePackageFromKey(propertyKey, snapshot, currentPackage);
                if (!string.IsNullOrEmpty(propertyPackage))
                {
                    return propertyPackage;
                }
            }
            
            // 查询类型（使用FullName）
            var symbolFullName = symbol.ToDisplayString();
            var typePackage = ResolvePackageFromKey(symbolFullName, snapshot, currentPackage);
            if (!string.IsNullOrEmpty(typePackage))
            {
                return typePackage;
            }
            
            // 【特殊情况】：如果是系统定义的委托、方法等，允许自由访问
            if (symbol.ContainingNamespace?.ToDisplayString() == "System" || 
                symbol.ContainingAssembly?.Name?.StartsWith("System") == true ||
                symbol.ContainingAssembly?.Name?.StartsWith("Microsoft") == true ||
                symbol.ContainingAssembly?.Name?.StartsWith("mscorlib") == true ||
                symbol.ContainingAssembly?.Name?.StartsWith("netstandard") == true)
            {
                return null;
            }
            
            // 符号不是在ET框架中声明的，允许自由访问
            return null;
        }

        private bool CanAccessSymbol(string currentPackage, string targetPackage, AnalysisSnapshot snapshot)
        {
            // 【包依赖规范】：包中只能访问自己包或者依赖包的符号
            if (currentPackage == targetPackage) return true;
            
            if (!snapshot.PackageInfos.TryGetValue(currentPackage, out var currentInfo))
            {
                return false;
            }
            
            if (!snapshot.PackageInfos.TryGetValue(targetPackage, out var targetInfo))
            {
                return false;
            }

            if (currentInfo.Dependencies != null && currentInfo.Dependencies.Contains(targetPackage))
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// 【废弃方法】：已由PrecomputeAllDependenciesFlat和扁平化依赖关系替代
        /// </summary>
        [Obsolete("Use _flatDependencies for O(1) lookup instead")]
        private bool IsInDependencyChain(PackageInfo currentPackage, string targetPackage)
        {
            // 兼容性保留，但建议使用_flatDependencies
            if (_flatDependencies.TryGetValue(currentPackage.Name, out var dependencies))
            {
                return dependencies.Contains(targetPackage);
            }
            
            // 回退到原有逻辑
            return currentPackage.AllDependencies.Contains(targetPackage);
        }

        /// <summary>
        /// 【性能优化】：预计算所有包的完整依赖集合，使用扁平化HashSet实现O(1)查找
        /// </summary>
        private void PrecomputeAllDependenciesFlat()
        {
            try
            {
                // 先初始化所有包的依赖集合
                foreach (var packageName in _packageInfos.Keys)
                {
                    _flatDependencies[packageName] = new HashSet<string>();
                }
                
                // 使用拓扑排序计算传递依赖（不检测循环依赖）
                var computed = new HashSet<string>();
                var computing = new HashSet<string>();
                
                foreach (var packageName in _packageInfos.Keys)
                {
                    ComputePackageDependenciesFlatSafe(packageName, computed, computing);
                }
            }
            catch (Exception ex)
            {
                // 【改进错误处理】：记录错误但继续执行
                LogToFile($"PrecomputeAllDependenciesFlat failed: {ex.Message}");
#if DEBUG
                // Debug模式下输出更详细信息
                LogToFile($"Stack trace: {ex.StackTrace}");
#endif
            }
        }

        /// <summary>
        /// 递归计算单个包的所有依赖（安全版本，忽略循环依赖）
        /// </summary>
        private void ComputePackageDependenciesFlatSafe(string packageName, HashSet<string> computed, HashSet<string> computing)
        {
            if (computed.Contains(packageName) || !_packageInfos.TryGetValue(packageName, out var packageInfo))
                return;
            
            if (computing.Contains(packageName))
            {
                // 静默跳过循环依赖，不影响性能计算
                return;
            }
            
            computing.Add(packageName);
            
            var dependencies = _flatDependencies[packageName];
            
            foreach (var dependency in packageInfo.Dependencies)
            {
                // 递归计算依赖的依赖
                ComputePackageDependenciesFlatSafe(dependency, computed, computing);
                
                // 添加直接依赖
                dependencies.Add(dependency);
                
                // 添加传递依赖
                if (_flatDependencies.TryGetValue(dependency, out var transitiveDeps))
                {
                    dependencies.UnionWith(transitiveDeps);
                }
            }
            
            computing.Remove(packageName);
            computed.Add(packageName);
        }
        private class PackageInfo
        {
            public string Name { get; set; } = string.Empty;
            public List<string> Dependencies { get; set; } = new List<string>();
            public int Level { get; set; } = 0;
            
            /// <summary>
            /// 预计算的完整依赖集合（包括传递依赖）
            /// </summary>
            public HashSet<string> AllDependencies { get; set; } = new HashSet<string>();
        }
    }
}
