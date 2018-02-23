using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AutoGen
{
    public interface IAutoGenerator
    {
        bool AddRes(string path);
        //bool AddRes(Assembly assembly);
        bool DelRes(string path);
        bool Belong(string path);
        string GenerateRClass();
        //List<AssetInfo> GenerateRInfo();
    }

    #region FileAutoGen

    public abstract class FileAutoGen : IAutoGenerator
    {
        protected Dictionary<string, AssetInfo> content = new Dictionary<string, AssetInfo>();

        public abstract bool Belong(string path);

        public virtual string GenerateRClass()
        {
            const string classFormat = @"public static class {0} {{ {1} }}";
            const string filedFormat = "\npublic const string {0} = @\"{1}\";";
            var stringBuilder = new StringBuilder();
            foreach (var e in content)
            {
                stringBuilder.AppendFormat(filedFormat, e.Key, e.Value.ResourcePath);
            }
            return string.Format(classFormat, ClassNameToGen(), stringBuilder);
        }

        public virtual List<AssetInfo> GenerateRInfo()
        {
            var infos = new List<AssetInfo>();
            foreach (var e in content)
            {
                if (e.Value.AssetType != AssetType.None)
                    infos.Add(e.Value);
            }
            return infos;
        }

        public bool AddRes(string path)
        {
            var key = AutoGenHelper.GetAssetNameFromPath(path);
            if (content.ContainsKey(key))
            {
                throw new ArgumentException("Duplicate Named Resources:" + key);
            }
            var type = AssetTypeToGen();
            var resPath = AutoGenHelper.RelativeResourcesPath(path);
            var assetPath = AutoGenHelper.RelativeAssetsPath(path);
            var bundleName = AutoGenHelper.GetAssetBundleNameAtPath(assetPath);
            var value = AssetInfo.Create(type, resPath, bundleName, assetPath);
            content.Add(key, value);
            return true;
        }

        public bool DelRes(string path)
        {
            var name = AutoGenHelper.GetAssetNameFromPath(path);
            if (content.ContainsKey(name))
            {
                content.Remove(name);
                return true;
            }
            return false;
        }

        public abstract string ClassNameToGen();
        public abstract AssetType AssetTypeToGen();
    }

    public class LuaAutoGen : FileAutoGen
    {
        public override AssetType AssetTypeToGen()
        {
            return AssetType.TextAsset;
        }

        public override bool Belong(string path)
        {
            return path.EndsWith(".lua.txt");
        }

        public override string ClassNameToGen()
        {
            return "Lua";
        }
    }
    public class TextureAutoGen : FileAutoGen
    {
        public override bool Belong(string path)
        {
            return path.EndsWith(".png");
        }

        public override string ClassNameToGen()
        {
            return "Texture";
        }

        public override AssetType AssetTypeToGen()
        {
            return AssetType.Texture2D;
        }
    }

    public class ConfigAutoGen : FileAutoGen
    {
        public override bool Belong(string path)
        {
            return path.EndsWith(".json");
        }

        public override string ClassNameToGen()
        {
            return "Json";
        }

        public override AssetType AssetTypeToGen()
        {
            return AssetType.TextAsset;
        }
    }

    public class PrefabAutoGen : FileAutoGen
    {
        public override bool Belong(string path)
        {
            return path.EndsWith(".prefab");
        }

        public override string ClassNameToGen()
        {
            return "Prefab";
        }

        public override AssetType AssetTypeToGen()
        {
            return AssetType.Prefab;
        }
    }

    public class AudioAutoGen : FileAutoGen
    {
        public override bool Belong(string path)
        {
            return path.EndsWith(".ogg") || path.EndsWith(".wav") || path.EndsWith(".wma");
        }

        public override string ClassNameToGen()
        {
            return "Audio";
        }

        public override AssetType AssetTypeToGen()
        {
            return AssetType.AudioClip;
        }
    }



    public class SceneAutoGen : FileAutoGen
    {
        public override bool Belong(string path)
        {
            return path.EndsWith(".unity");
        }

        public override string ClassNameToGen()
        {
            return "Scene";
        }

        public override AssetType AssetTypeToGen()
        {
            return AssetType.None;
        }

        public override string GenerateRClass()
        {
            const string classFormat = @"public static class {0} {{ {1} }}";
            const string filedFormat = "\npublic const string {0} = @\"{1}\";";
            var stringBuilder = new StringBuilder();
            foreach (var e in content)
            {
                var scenePath = e.Value.AssetPath.Replace(@"Assets\", "").Replace(@".unity", "").Replace(@"\", "/");
                stringBuilder.AppendFormat(filedFormat, e.Key, scenePath);
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(@"public static readonly string[] Scenes = new string[]{");
            foreach (var e in content)
            {
                stringBuilder.AppendFormat("{0},", e.Key);
                stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine(@"};");
            return string.Format(classFormat, ClassNameToGen(), stringBuilder);
        }
    }


    public class ShaderAutoGen : IAutoGenerator
    {
        protected Dictionary<string, string> content = new Dictionary<string, string>();

        public bool Belong(string path)
        {
            return path.EndsWith(".shader");
        }

        public string GenerateRClass()
        {
            const string classFormat = @"public static class {0} {{ {1} }}";
            const string filedFormat = "\npublic const string {0} = @\"{1}\";";
            var stringBuilder = new StringBuilder();
            foreach (var e in content)
            {
                stringBuilder.AppendFormat(filedFormat, e.Key, e.Value);
            }
            return string.Format(classFormat, ClassNameToGen(), stringBuilder);
        }

        public List<AssetInfo> GenerateRInfo()
        {
            var infos = new List<AssetInfo>();
            return infos;
        }

        public bool AddRes(string path)
        {
            var key = AutoGenHelper.GetAssetNameFromPath(path);
            if (content.ContainsKey(key))
            {
                throw new ArgumentException("Duplicate Named Resources:" + key);
            }
            else
            {
                Shader s = AutoGenHelper.LoadAssetAtFullPath<Shader>(path);
                if (s)
                {
                    content.Add(key, s.name);
                }
            }
            return true;
        }

        public bool DelRes(string path)
        {
            var name = AutoGenHelper.GetAssetNameFromPath(path);
            if (content.ContainsKey(name))
            {
                content.Remove(name);
                return true;
            }
            return false;
        }

        public string ClassNameToGen()
        {
            return "Shader";
        }

        public AssetType AssetTypeToGen()
        {
            return AssetType.Shader;
        }
    }

    #endregion

    #region IniAutoGen

    public abstract class IniAutoGen : IAutoGenerator
    {
        protected Dictionary<string, string> content = new Dictionary<string, string>();
        public abstract bool Belong(string path);

        public virtual bool AddRes(string path)
        {
            var list = GetIniData(path);
            for (var i = 0; i < list.Count; ++i)
            {
                if (list[i].Length == 2)
                {
                    var key = AutoGenHelper.GetAssetNameFromPath(list[i][0]);
                    var value = list[i][1].Trim();
                    if (content.ContainsKey(key))
                    {
                        throw new ArgumentException("Duplicate Named Resources:" + key);
                    }
                    content.Add(key, value);
                }
            }
            return true;
        }

        public bool DelRes(string path)
        {
            var list = File.ReadAllLines(path).Where(line => !(line.Trim().Length == 0 || string.IsNullOrEmpty(line))).Select(line => line.Split(new[] { '=' }, 2, 0)).ToList();
            for (var i = 0; i < list.Count; ++i)
            {
                if (list[i].Length != 2)
                {
                    continue;
                }
                var key = AutoGenHelper.GetAssetNameFromPath(list[i][0]);
                if (content.ContainsKey(key))
                {
                    content.Remove(key);
                }
            }
            return true;
        }

        public string GenerateRClass()
        {
            const string classFormat = @"public static class {0} {{ {1} }}";
            const string stringFormat = "\npublic static readonly string {0} = @\"{1}\";";
            const string intFormat = "\npublic static readonly int {0} = {1};";
            const string floatFormat = "\npublic static readonly float {0} = {1}f;";
            var stringBuilder = new StringBuilder();
            foreach (var e in content)
            {
                var valueInt = 0;
                var valueFloat = 0.0f;
                if (int.TryParse(e.Value, out valueInt))
                {
                    stringBuilder.AppendFormat(intFormat, e.Key, valueInt);
                }
                else if (float.TryParse(e.Value, out valueFloat))
                {
                    stringBuilder.AppendFormat(floatFormat, e.Key, valueFloat);
                }
                else
                {
                    stringBuilder.AppendFormat(stringFormat, e.Key, e.Value);
                }
            }
            return string.Format(classFormat, ClassNameToGen(), stringBuilder);
        }

        public List<AssetInfo> GenerateRInfo()
        {
            return new List<AssetInfo>();
        }

        public abstract string ClassNameToGen();

        public List<string[]> GetIniData(string path)
        {
            var list = File.ReadAllLines(path).Where(line => !(line.Trim().Length == 0 || string.IsNullOrEmpty(line))).Select(line => line.Split(new[] { '=' }, 2, 0)).ToList();
            return list;
        }
    }

    public class LanguageAutoGen : IniAutoGen
    {
        public override bool Belong(string path)
        {
            return path.EndsWith("Chinese.txt");
        }

        public override bool AddRes(string path)
        {
            var list = GetIniData(path);
            for (var i = 0; i < list.Count; ++i)
            {
                if (list[i].Length == 2)
                {
                    var key = AutoGenHelper.GetAssetNameFromPath(list[i][0]);
                    //string value = list[i][1].Trim();
                    if (content.ContainsKey(key))
                    {
                        throw new ArgumentException("Duplicate Named Resources:" + key);
                    }
                    content.Add(key, key);
                }
            }
            return true;
        }

        public override string ClassNameToGen()
        {
            return "Lang";
        }
    }

    #endregion
}