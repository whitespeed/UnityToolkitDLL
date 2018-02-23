using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.CodeDom.Compiler;

namespace AutoGen
{
    public static class AutoGenHelper
    {
        static readonly string ResourcesStr = "Resources";
        static readonly string AssetsStr = "Assets";
        public static string GetAssetBundleNameAtPath(string assetPath)
        {
            var import = AssetImporter.GetAtPath(assetPath);
            if (null != import)
            {
                return import.assetBundleName;
            }
            return String.Empty;
        }
        public static string GetAssetBundleVariantAtPath(string assetPath)
        {
            var import = AssetImporter.GetAtPath(assetPath);
            if (null != import)
            {
                return import.assetBundleVariant;
            }
            return String.Empty;
        }

        public static bool IsInResourcesFolder(string fullPath)
        {
            return fullPath.Contains(@"/Resources/");
        }

        public static bool IsInAssestFolder(string fullPath)
        {
            return fullPath.StartsWith(Application.dataPath);
        }

        public static string GetAssetNameFromPath(string path)
        {
            var key = Path.GetFileNameWithoutExtension(path.Trim()).Replace(" ", "_").Replace(".", "");
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Resource Name is INVALID: " + path);
            }
            var provider = CodeDomProvider.CreateProvider("C#");
            if (!provider.IsValidIdentifier(key))
            {
                throw new ArgumentException("Resource Name is INVALID: " + key);
            }
            return key;
        }


        public static string RelativeResourcesPath(string fullPath, bool keepExtension = false)
        {
            int resIdx = fullPath.Trim().LastIndexOf(ResourcesStr, StringComparison.Ordinal) + 1;
            if (keepExtension)
            {
                return fullPath.Substring(resIdx + ResourcesStr.Length);
            }
            else
            {
                int extId = fullPath.LastIndexOf(".", StringComparison.Ordinal);
                return fullPath.Substring(resIdx + ResourcesStr.Length, extId - resIdx - ResourcesStr.Length);
            }
        }

        public static string GetFullPathFromAssetsPath(string assetPath)
        {
            return Path.GetFullPath(assetPath);
        }

        public static string RelativeAssetsPath(string fullPath, bool keepExtension = true)
        {
            int idx = fullPath.Trim().IndexOf(AssetsStr, StringComparison.Ordinal);
            if (keepExtension)
            {
                return fullPath.Substring(idx);
            }
            else
            {
                int extId = fullPath.LastIndexOf(".", StringComparison.Ordinal);
                return fullPath.Substring(idx + ResourcesStr.Length, extId - idx - ResourcesStr.Length);
            }
        }
        public static T LoadAssetAtFullPath<T>(string fullPath) where T : UnityEngine.Object
        {
            string assetPath = RelativeAssetsPath(fullPath);
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
    }

}

