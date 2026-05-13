using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET.Test
{
    internal static class UnityBridgeHandlerTestSupport
    {
        private const string TempFolder = "Assets/__UnityBridgeHandlerTests";

        public static string UniqueName(string prefix)
        {
            return prefix + "_" + Guid.NewGuid().ToString("N");
        }

        public static GameObject CreateSceneObject(string name, Transform parent = null)
        {
            GameObject gameObject = new(name);
            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }

            return gameObject;
        }

        public static void DestroyObjects(params UnityEngine.Object[] objects)
        {
            foreach (UnityEngine.Object value in objects)
            {
                if (value != null)
                {
                    UnityEngine.Object.DestroyImmediate(value);
                }
            }
        }

        public static string GetPath(GameObject gameObject)
        {
            string path = gameObject.name;
            Transform parent = gameObject.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        public static BridgeVector3 CreateVector3(float x, float y, float z)
        {
            BridgeVector3 value = BridgeVector3.Create();
            value.X = x;
            value.Y = y;
            value.Z = z;
            return value;
        }

        public static bool NearlyEqual(float left, float right)
        {
            return Math.Abs(left - right) <= 0.001f;
        }

        public static bool VectorNearlyEqual(Vector3 actual, float x, float y, float z)
        {
            return NearlyEqual(actual.x, x) && NearlyEqual(actual.y, y) && NearlyEqual(actual.z, z);
        }

        public static string SaveTempPrefab(GameObject root)
        {
            EnsureTempFolder();
            string path = TempFolder + "/" + root.name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            AssetDatabase.ImportAsset(path);
            return path;
        }

        public static string CreateTempAssetPath(string fileName)
        {
            EnsureTempFolder();
            return TempFolder + "/" + fileName;
        }

        public static string WriteTempTextAsset(string fileName, string content)
        {
            string assetPath = CreateTempAssetPath(fileName);
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string fullPath = Path.Combine(projectRoot, assetPath);
            File.WriteAllText(fullPath, content);
            AssetDatabase.ImportAsset(assetPath);
            return assetPath;
        }

        public static void DeleteTempAsset(string assetPath)
        {
            if (!string.IsNullOrWhiteSpace(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            DeleteTempFolderIfEmpty();
        }

        private static void EnsureTempFolder()
        {
            if (!AssetDatabase.IsValidFolder(TempFolder))
            {
                AssetDatabase.CreateFolder("Assets", "__UnityBridgeHandlerTests");
            }
        }

        private static void DeleteTempFolderIfEmpty()
        {
            if (!AssetDatabase.IsValidFolder(TempFolder))
            {
                return;
            }

            string[] assets = AssetDatabase.FindAssets(string.Empty, new[] { TempFolder });
            if (assets.Length == 0)
            {
                AssetDatabase.DeleteAsset(TempFolder);
            }
        }
    }
}
