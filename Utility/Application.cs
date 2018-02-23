using System;
using System.IO;
using UnityEngine;
namespace Utility
{
    public class Application
    {
        protected static string mDataParentPath = Path.Combine(UnityEngine.Application.dataPath, "/../");
        public static string customPath
        {
            get
            {
                if (UnityEngine.Application.isMobilePlatform)
                {
                    return UnityEngine.Application.persistentDataPath;
                }
                else if (UnityEngine.Application.isEditor)
                {
                    return mDataParentPath;
                }
                else
                {
                    return UnityEngine.Application.dataPath;
                }
            }
        }
    }
}
