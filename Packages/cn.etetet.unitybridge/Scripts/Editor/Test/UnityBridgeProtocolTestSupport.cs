using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MemoryPack;
using MongoDB.Bson;

namespace ET.Test
{
    internal static class UnityBridgeProtocolTestSupport
    {
        public static T RoundTrip<T>(T value) where T : class
        {
            return MongoHelper.FromJson<T>(UnityBridgeMongoJsonHelper.ToJson(value));
        }

        public static string AssertRequestType(IRequest request, string expectedFullName)
        {
            string commandJson = UnityBridgeMongoJsonHelper.ToCommandJson(request);
            BsonDocument commandDocument = BsonDocument.Parse(commandJson);
            if (!commandDocument.TryGetValue("_t", out BsonValue commandTypeValue))
            {
                return $"{request.GetType().Name} should include _t discriminator";
            }

            if (commandTypeValue.AsString != expectedFullName)
            {
                return $"{request.GetType().Name} should persist full request type";
            }

            return null;
        }

        public static BridgeVector3 CreateVector3(float x, float y, float z)
        {
            BridgeVector3 value = BridgeVector3.Create();
            value.X = x;
            value.Y = y;
            value.Z = z;
            return value;
        }

        public static BridgeQuaternion CreateQuaternion(float x, float y, float z, float w)
        {
            BridgeQuaternion value = BridgeQuaternion.Create();
            value.X = x;
            value.Y = y;
            value.Z = z;
            value.W = w;
            return value;
        }

        public static BridgeTransformInfo CreateTransformInfo()
        {
            BridgeTransformInfo value = BridgeTransformInfo.Create();
            value.LocalPosition = CreateVector3(1.25f, 2.5f, 3.75f);
            value.LocalEulerAngles = CreateVector3(0f, 90f, 0f);
            value.LocalRotation = CreateQuaternion(0f, 0f, 0f, 1f);
            value.LocalScale = CreateVector3(1f, 1f, 2f);
            value.ParentPath = "Root";
            value.SiblingIndex = 2;
            value.Position = CreateVector3(4.25f, 5.5f, 6.75f);
            value.EulerAngles = CreateVector3(10f, 20f, 30f);
            value.ChildCount = 3;
            return value;
        }

        public static BridgeObjectInfo CreateObjectInfo(string name = "Player", string path = "Root/Player", int instanceId = 3001)
        {
            BridgeObjectInfo value = BridgeObjectInfo.Create();
            value.Name = name;
            value.Path = path;
            value.InstanceId = instanceId;
            value.ActiveSelf = true;
            value.ActiveInHierarchy = true;
            value.Tag = "Player";
            value.LayerName = "Default";
            value.Layer = 0;
            value.Transform = CreateTransformInfo();
            value.Components.Add(CreateComponentInfo());
            return value;
        }

        public static BridgeAssetInfo CreateAssetInfo()
        {
            BridgeAssetInfo value = BridgeAssetInfo.Create();
            value.AssetPath = "Assets/Prefabs/Player.prefab";
            value.Guid = "guid-player";
            value.TypeName = "GameObject";
            value.Name = "Player";
            value.Extension = ".prefab";
            value.FileSize = 1024;
            value.InstanceId = 4001;
            return value;
        }

        public static BridgeComponentInfo CreateComponentInfo()
        {
            BridgeComponentInfo value = BridgeComponentInfo.Create();
            value.TypeName = "Transform";
            value.FullTypeName = "UnityEngine.Transform";
            value.ComponentIndex = 0;
            value.InstanceId = 5001;
            value.Enabled = true;
            return value;
        }

        public static BridgeSceneNode CreateSceneNode()
        {
            BridgeSceneNode value = BridgeSceneNode.Create();
            value.Object = CreateObjectInfo();

            BridgeSceneNode child = BridgeSceneNode.Create();
            child.Object = CreateObjectInfo("Child", "Root/Player/Child", 3002);
            value.Children.Add(child);
            return value;
        }

        public static bool NearlyEqual(float a, float b)
        {
            return Math.Abs(a - b) <= 0.0001f;
        }

        public static int AssertRequestRoundTrip<T>() where T : class, IRequest
        {
            T value = CreateMessage<T>();
            FillMessage(value);
            value.RpcId = 1001;

            string typeError = AssertRequestType(value, typeof(T).FullName);
            if (typeError != null)
            {
                return Fail(1, typeError);
            }

            T roundTrip = RoundTrip(value);
            string roundTripError = AssertSerializableEqual(value, roundTrip, typeof(T).Name);
            if (roundTripError != null)
            {
                return Fail(2, roundTripError);
            }

            return ErrorCode.ERR_Success;
        }

        public static int AssertResponseRoundTrip<T>() where T : class, IResponse
        {
            T value = CreateMessage<T>();
            FillMessage(value);
            value.RpcId = 1002;
            value.Error = 0;
            value.Message = "ok";

            T roundTrip = RoundTrip(value);
            string roundTripError = AssertSerializableEqual(value, roundTrip, typeof(T).Name);
            if (roundTripError != null)
            {
                return Fail(1, roundTripError);
            }

            return ErrorCode.ERR_Success;
        }

        public static int Fail(int code, string message)
        {
            Log.Console(message);
            return code;
        }

        private static T CreateMessage<T>() where T : class
        {
            MethodInfo createMethod = typeof(T).GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(bool) }, null);
            if (createMethod == null)
            {
                throw new InvalidOperationException($"{typeof(T).FullName} has no Create(bool) method");
            }

            return (T)createMethod.Invoke(null, new object[] { false });
        }

        private static void FillMessage(object value)
        {
            if (value == null)
            {
                return;
            }

            foreach (PropertyInfo property in GetSerializableProperties(value.GetType()))
            {
                if (!property.CanRead || !property.CanWrite)
                {
                    continue;
                }

                if (property.Name is "RpcId" or "Error" or "Message")
                {
                    continue;
                }

                object sample = CreateSampleValue(property.PropertyType, property.Name);
                if (sample != null)
                {
                    property.SetValue(value, sample);
                }
            }
        }

        private static object CreateSampleValue(Type type, string name)
        {
            if (type == typeof(string))
            {
                return $"{name}Value";
            }

            if (type == typeof(int))
            {
                return 101;
            }

            if (type == typeof(long))
            {
                return 1001L;
            }

            if (type == typeof(float))
            {
                return 1.25f;
            }

            if (type == typeof(double))
            {
                return 2.5d;
            }

            if (type == typeof(bool))
            {
                return true;
            }

            if (type == typeof(BridgeVector2))
            {
                BridgeVector2 vector = BridgeVector2.Create();
                vector.X = 1.25f;
                vector.Y = 2.5f;
                return vector;
            }

            if (type == typeof(BridgeVector3))
            {
                return CreateVector3(1.25f, 2.5f, 3.75f);
            }

            if (type == typeof(BridgeQuaternion))
            {
                return CreateQuaternion(0f, 0f, 0f, 1f);
            }

            if (type == typeof(BridgeTransformInfo))
            {
                return CreateTransformInfo();
            }

            if (type == typeof(BridgeObjectInfo))
            {
                return CreateObjectInfo();
            }

            if (type == typeof(BridgeAssetInfo))
            {
                return CreateAssetInfo();
            }

            if (type == typeof(BridgeSceneNode))
            {
                return CreateSceneNode();
            }

            if (type == typeof(BridgeComponentInfo))
            {
                return CreateComponentInfo();
            }

            if (type == typeof(BridgePropertyInfo))
            {
                BridgePropertyInfo property = BridgePropertyInfo.Create();
                property.Name = "m_Name";
                property.DisplayName = "Name";
                property.Type = "String";
                property.StringValue = "Player";
                property.IntValue = 42;
                property.FloatValue = 3.5f;
                property.BoolValue = true;
                property.Vector2Value = (BridgeVector2)CreateSampleValue(typeof(BridgeVector2), nameof(BridgePropertyInfo.Vector2Value));
                property.Vector3Value = CreateVector3(7f, 8f, 9f);
                property.ObjectReferencePath = "Assets/Prefabs/Player.prefab";
                property.ObjectReferenceType = "GameObject";
                property.IsArray = true;
                property.IsEditable = true;
                property.PropertyPath = "m_Name";
                property.IsExpanded = true;
                property.HasChildren = true;
                property.Depth = 1;
                return property;
            }

            if (type == typeof(BridgeConsoleLog))
            {
                BridgeConsoleLog log = BridgeConsoleLog.Create();
                log.LogType = "Log";
                log.Message = "message";
                log.StackTrace = "stack";
                log.Time = "2026-05-14T00:00:00Z";
                return log;
            }

            if (type == typeof(BridgeGameViewResolution))
            {
                BridgeGameViewResolution resolution = BridgeGameViewResolution.Create();
                resolution.Width = 1920;
                resolution.Height = 1080;
                resolution.Label = "HD";
                resolution.IsCurrent = true;
                return resolution;
            }

            if (type == typeof(BridgeScreenshotInfo))
            {
                BridgeScreenshotInfo screenshot = BridgeScreenshotInfo.Create();
                screenshot.Path = "/tmp/game.png";
                screenshot.FileName = "game.png";
                screenshot.Width = 1920;
                screenshot.Height = 1080;
                screenshot.FileSize = 2048;
                screenshot.MediaType = "image/png";
                return screenshot;
            }

            if (type == typeof(BridgeBatchStepResult))
            {
                BridgeBatchStepResult step = BridgeBatchStepResult.Create();
                step.Name = "step";
                step.Command = "Ping";
                step.Error = 0;
                step.Message = "ok";
                return step;
            }

            if (type == typeof(BridgeTestResult))
            {
                BridgeTestResult result = BridgeTestResult.Create();
                result.Name = "Test";
                result.FullName = "ET.Test.Test";
                result.Passed = true;
                result.Error = 0;
                result.Message = "ok";
                result.DurationMs = 1;
                return result;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                IList list = (IList)Activator.CreateInstance(type);
                Type itemType = type.GetGenericArguments()[0];
                object item = CreateSampleValue(itemType, name);
                if (item != null)
                {
                    list.Add(item);
                }

                return list;
            }

            return null;
        }

        private static List<PropertyInfo> GetSerializableProperties(Type type)
        {
            List<PropertyInfo> properties = new();
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.GetCustomAttribute<MemoryPackOrderAttribute>() == null)
                {
                    continue;
                }

                properties.Add(property);
            }

            properties.Sort((a, b) =>
                    a.GetCustomAttribute<MemoryPackOrderAttribute>().Order.CompareTo(
                        b.GetCustomAttribute<MemoryPackOrderAttribute>().Order));
            return properties;
        }

        private static string AssertSerializableEqual(object expected, object actual, string path)
        {
            if (ReferenceEquals(expected, actual))
            {
                return null;
            }

            if (expected == null || actual == null)
            {
                return $"{path} should round-trip null state. Expected={FormatValue(expected)}, Actual={FormatValue(actual)}";
            }

            if (IsScalar(expected.GetType()))
            {
                return ScalarEquals(expected, actual)
                        ? null
                        : $"{path} should round-trip value. Expected={FormatValue(expected)}, Actual={FormatValue(actual)}";
            }

            if (expected is IList expectedList && actual is IList actualList)
            {
                if (expectedList.Count != actualList.Count)
                {
                    return $"{path} should round-trip list count. Expected={expectedList.Count}, Actual={actualList.Count}";
                }

                for (int i = 0; i < expectedList.Count; ++i)
                {
                    string itemError = AssertSerializableEqual(expectedList[i], actualList[i], $"{path}[{i}]");
                    if (itemError != null)
                    {
                        return itemError;
                    }
                }

                return null;
            }

            foreach (PropertyInfo property in GetSerializableProperties(expected.GetType()))
            {
                string propertyError = AssertSerializableEqual(
                    property.GetValue(expected),
                    property.GetValue(actual),
                    $"{path}.{property.Name}");
                if (propertyError != null)
                {
                    return propertyError;
                }
            }

            return null;
        }

        private static bool IsScalar(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal);
        }

        private static bool ScalarEquals(object expected, object actual)
        {
            return expected switch
            {
                float expectedFloat when actual is float actualFloat => NearlyEqual(expectedFloat, actualFloat),
                double expectedDouble when actual is double actualDouble => Math.Abs(expectedDouble - actualDouble) <= 0.0001d,
                _ => Equals(expected, actual)
            };
        }

        private static string FormatValue(object value)
        {
            return value == null ? "null" : value.ToString();
        }
    }
}
