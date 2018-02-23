using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

namespace Utility
{
    
    public class AppSetting
    {
        public struct SettingDefine<T>
        {
            public T defaultValue;
            public string path;

            public SettingDefine(string k)
            {
                defaultValue = default(T);
                path = k;
            }

            public SettingDefine(string k, T dValue)
            {
                defaultValue = dValue;
                path = k;
            } 
        }

        public static T GetSetting<T>(SettingDefine<T> key)
        {

            if (PlayerPrefs.HasKey(key.path))
            {
                string value = PlayerPrefs.GetString(key.path);
                if (typeof (T).IsValueType || typeof(T) ==  typeof(string))
                {
                    var foo = TypeDescriptor.GetConverter(typeof(T));
                    return (T)(foo.ConvertFromInvariantString(value));
                }
                else
                {
                    T re = JsonUtility.FromJson<T>(value);
                    return re;
                }
            }
            else
            {
                return key.defaultValue;
            }
        }
        public static void SetSetting<T>(SettingDefine<T> key, T value)
        {
            var text = typeof(T).IsValueType ? value.ToString() : JsonUtility.ToJson(value);
            PlayerPrefs.SetString(key.path, text);
        }
    }

}
