using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoGen
{
    public enum AssetType
    {
        None,
        Texture2D,
        TextAsset,
        Prefab,
        AudioClip,
        Shader,
    }

    [System.Serializable]
    public struct AssetInfo
    {
        public AssetType AssetType;
        public string AssetPath;
        public string ResourcePath;
        public string AssetBundleName;

        public static AssetInfo Create(AssetType type, string resPath, string abName, string assetPath)
        {
            AssetInfo info = new AssetInfo();
            info.AssetType = type;
            info.ResourcePath = resPath;
            info.AssetBundleName = abName;
            info.AssetPath = assetPath;
            return info;
        }

        public override string ToString()
        {
            return string.Format("\"{0}\", \"{1}\", \"{2}\", \"{3}\"", AssetType, AssetPath, ResourcePath, AssetBundleName);
        }
    }
}
