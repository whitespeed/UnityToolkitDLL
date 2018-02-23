using System;
using System.Collections.Generic;
using UnityEngine;

namespace Localization
{
    public static class Localization
    {
        public delegate byte[] LoadFunction(string path);

        public delegate void OnLocalizeNotification();

        /// <summary>
        ///     Want to have Localization loading be custom instead of just Resources.Load? Set this function.
        /// </summary>
        public static LoadFunction loadFunction;

        public static string strDic = "Language";

        /// <summary>
        ///     Notification triggered when the localization data gets changed, such as when changing the language.
        ///     If you want to make modifications to the localization data after it was loaded, this is the place.
        /// </summary>
        public static OnLocalizeNotification onLocalize;

        /// <summary>
        ///     Whether the localization dictionary has been loaded.
        /// </summary>
        public static bool localizationHasBeenSet;

        // Loaded languages, if any
        private static string[] mLanguages;

        // Key = Value dictionary (single language)
        private static Dictionary<string, string> mOldDictionary = new Dictionary<string, string>();

        // Key = Values dictionary (multiple languages)
        private static Dictionary<string, string[]> mDictionary = new Dictionary<string, string[]>();

        // Replacement dictionary forces a specific value instead of the existing entry
        private static readonly Dictionary<string, string> mReplacement = new Dictionary<string, string>();

        // Index of the selected language within the multi-language dictionary
        private static int mLanguageIndex = -1;

        // Currently selected language
        private static string mLanguage;

        private static bool mMerging;

        /// <summary>
        ///     Localization dictionary. Dictionary key is the localization key.
        ///     Dictionary value is the list of localized values (columns in the CSV file).
        /// </summary>
        private static Dictionary<string, string[]> dictionary
        {
            get
            {
                if(!localizationHasBeenSet)
                {
                    LoadDictionary(DefaultLanguage);
                }
                return mDictionary;
            }
            set
            {
                localizationHasBeenSet = value != null;
                mDictionary = value;
            }
        }

        /// <summary>
        ///     List of loaded languages. Available if a single Localization.csv file was used.
        /// </summary>
        public static string[] KnownLanguages
        {
            get
            {
                if(!localizationHasBeenSet)
                {
                    LoadDictionary(DefaultLanguage);
                }
                return mLanguages;
            }
        }

        /// <summary>
        ///     Name of the currently active language.
        /// </summary>
        public static string Language
        {
            get
            {
                if(string.IsNullOrEmpty(mLanguage))
                {
                    mLanguage = DefaultLanguage;
                    LoadAndSelect(mLanguage);
                }
                return mLanguage;
            }
            set
            {
                if(mLanguage != value)
                {
                    mLanguage = value;
                    LoadAndSelect(value);
                }
            }
        }

        public static string DefaultLanguage { get; } = Application.systemLanguage.ToString();

        /// <summary>
        ///     Clear the replacement values.
        /// </summary>
        public static void ClearReplacements()
        {
            mReplacement.Clear();
        }

        /// <summary>
        ///     Returns whether the specified key is present in the localization dictionary.
        /// </summary>
        public static bool Exists(string key)
        {
            // Ensure we have a language to work with
            if(!localizationHasBeenSet)
            {
                Language = DefaultLanguage;
            }
#if UNITY_IPHONE || UNITY_ANDROID
	        string mobKey = key + " Mobile";
	        if (mDictionary.ContainsKey(mobKey)) return true;
	        else if (mOldDictionary.ContainsKey(mobKey)) return true;
#endif
            return mDictionary.ContainsKey(key) || mOldDictionary.ContainsKey(key);
        }

        /// <summary>
        ///     Localize the specified value and format it.
        /// </summary>
        public static string Format(string key, params object[] parameters)
        {
            return string.Format(Get(key), parameters);
        }

        /// <summary>
        ///     Localize the specified value.
        /// </summary>
        public static string Get(string key)
        {
            // Ensure we have a language to work with
            if(!localizationHasBeenSet)
            {
                LoadDictionary(DefaultLanguage);
            }
            if(mLanguages == null)
            {
                //Debug.LogError("No localization data present");
                return null;
            }
            var lang = Language;
            if(mLanguageIndex == -1)
            {
                for(var i = 0; i < mLanguages.Length; ++i)
                {
                    if(mLanguages[i] == lang)
                    {
                        mLanguageIndex = i;
                        break;
                    }
                }
            }
            if(mLanguageIndex == -1)
            {
                mLanguageIndex = 0;
                mLanguage = mLanguages[0];
                Debug.LogWarning("Language not found: " + lang);
            }
            string val;
            string[] vals;
            /*UICamera.ControlScheme scheme = UICamera.currentScheme;


            if (scheme == UICamera.ControlScheme.Touch)
            {
                string altKey = key + " Mobile";
                if (mReplacement.TryGetValue(altKey, out val)) return val;

                if (mLanguageIndex != -1 && mDictionary.TryGetValue(altKey, out vals))
                {
                    if (mLanguageIndex < vals.Length)
                        return vals[mLanguageIndex];
                }
                if (mOldDictionary.TryGetValue(altKey, out val)) return val;
            }
            else if (scheme == UICamera.ControlScheme.Controller)
            {
                string altKey = key + " Controller";
                if (mReplacement.TryGetValue(altKey, out val)) return val;

                if (mLanguageIndex != -1 && mDictionary.TryGetValue(altKey, out vals))
                {
                    if (mLanguageIndex < vals.Length)
                        return vals[mLanguageIndex];
                }
                if (mOldDictionary.TryGetValue(altKey, out val)) return val;
            }*/
            if(mReplacement.TryGetValue(key, out val))
            {
                return val;
            }
            if(mLanguageIndex != -1 && mDictionary.TryGetValue(key, out vals))
            {
                if(mLanguageIndex < vals.Length)
                {
                    var s = vals[mLanguageIndex];
                    if(string.IsNullOrEmpty(s))
                    {
                        s = vals[0];
                    }
                    return s;
                }
                return vals[0];
            }
            if(mOldDictionary.TryGetValue(key, out val))
            {
                return val;
            }
#if UNITY_EDITOR
            Debug.LogWarning("Localization key not found: '" + key + "' for language " + lang);
#endif
            return key;
        }

        /// <summary>
        ///     Load the specified asset and activate the localization.
        /// </summary>
        public static void Load(TextAsset asset)
        {
            var reader = new ByteReader(asset);
            Set(asset.name, reader.ReadDictionary());
        }

        /// <summary>
        ///     Load the specified CSV file.
        /// </summary>
        public static bool LoadCSV(TextAsset asset, bool merge = false)
        {
            return LoadCSV(asset.bytes, asset, merge);
        }

        /// <summary>
        ///     Load the specified CSV file.
        /// </summary>
        public static bool LoadCSV(byte[] bytes, bool merge = false)
        {
            return LoadCSV(bytes, null, merge);
        }

        /// <summary>
        ///     Forcefully replace the specified key with another value.
        /// </summary>
        public static void ReplaceKey(string key, string val)
        {
            if(!string.IsNullOrEmpty(val))
            {
                mReplacement[key] = val;
            }
            else
            {
                mReplacement.Remove(key);
            }
        }

        /// <summary>
        ///     Set the localization data directly.
        /// </summary>
        public static void Set(string languageName, byte[] bytes)
        {
            var reader = new ByteReader(bytes);
            Set(languageName, reader.ReadDictionary());
        }

        /// <summary>
        ///     Load the specified asset and activate the localization.
        /// </summary>
        public static void Set(string languageName, Dictionary<string, string> dictionary)
        {
            mLanguage = languageName;
            PlayerPrefs.SetString("Language", mLanguage);
            mOldDictionary = dictionary;
            localizationHasBeenSet = true;
            mLanguageIndex = -1;
            mLanguages = new[] {languageName};
            if(onLocalize != null)
            {
                onLocalize();
            }
            //UIRoot.Broadcast("OnLocalize");
        }

        /// <summary>
        ///     Change or set the localization value for the specified key.
        ///     Note that this method only supports one fallback language, and should
        ///     ideally be called from within Localization.onLocalize.
        ///     To set the multi-language value just modify Localization.dictionary directly.
        /// </summary>
        public static void Set(string key, string value)
        {
            if(mOldDictionary.ContainsKey(key))
            {
                mOldDictionary[key] = value;
            }
            else
            {
                mOldDictionary.Add(key, value);
            }
        }

        /// <summary>
        ///     Helper function that adds a single line from a CSV file to the localization list.
        /// </summary>
        private static void AddCSV(BetterList<string> newValues, string[] newLanguages, Dictionary<string, int> languageIndices)
        {
            if(newValues.size < 2)
            {
                return;
            }
            var key = newValues[0];
            if(string.IsNullOrEmpty(key))
            {
                return;
            }
            var copy = ExtractStrings(newValues, newLanguages, languageIndices);
            if(mDictionary.ContainsKey(key))
            {
                mDictionary[key] = copy;
                if(newLanguages == null)
                {
                    Debug.LogWarning("Localization key '" + key + "' is already present");
                }
            }
            else
            {
                try
                {
                    mDictionary.Add(key, copy);
                }
                catch(Exception ex)
                {
                    Debug.LogError("Unable to add '" + key + "' to the Localization dictionary.\n" + ex.Message);
                }
            }
        }

        /// <summary>
        ///     Used to merge separate localization files into one.
        /// </summary>
        private static string[] ExtractStrings(BetterList<string> added, string[] newLanguages, Dictionary<string, int> languageIndices)
        {
            if(newLanguages == null)
            {
                var values = new string[mLanguages.Length];
                for(int i = 1, max = Mathf.Min(added.size, values.Length + 1); i < max; ++i)
                {
                    values[i - 1] = added[i];
                }
                return values;
            }
            else
            {
                string[] values;
                var s = added[0];
                if(!mDictionary.TryGetValue(s, out values))
                {
                    values = new string[mLanguages.Length];
                }
                for(int i = 0, imax = newLanguages.Length; i < imax; ++i)
                {
                    var language = newLanguages[i];
                    var index = languageIndices[language];
                    values[index] = added[i + 1];
                }
                return values;
            }
        }

        /// <summary>
        ///     Whether the specified language is present in the localization.
        /// </summary>
        private static bool HasLanguage(string languageName)
        {
            for(int i = 0, imax = mLanguages.Length; i < imax; ++i)
            {
                if(mLanguages[i] == languageName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Load the specified language.
        /// </summary>
        private static bool LoadAndSelect(string value)
        {
            if(!string.IsNullOrEmpty(value))
            {
                if(mDictionary.Count == 0 && !LoadDictionary(value))
                {
                    return false;
                }
                if(SelectLanguage(value))
                {
                    return true;
                }
            }

            // Old style dictionary
            if(mOldDictionary.Count > 0)
            {
                return true;
            }

            // Either the language is null, or it wasn't found
            mOldDictionary.Clear();
            mDictionary.Clear();
            if(string.IsNullOrEmpty(value))
            {
                PlayerPrefs.DeleteKey("Language");
            }
            return false;
        }

        /// <summary>
        ///     Load the specified CSV file.
        /// </summary>
        private static bool LoadCSV(byte[] bytes, TextAsset asset, bool merge = false)
        {
            if(bytes == null)
            {
                return false;
            }
            var reader = new ByteReader(bytes);

            // The first line should contain "KEY", followed by languages.
            var header = reader.ReadCSV();

            // There must be at least two columns in a valid CSV file
            if(header.size < 2)
            {
                return false;
            }
            header.RemoveAt(0);
            string[] languagesToAdd = null;
            if(string.IsNullOrEmpty(mLanguage))
            {
                localizationHasBeenSet = false;
            }

            // Clear the dictionary
            if(!localizationHasBeenSet || (!merge && !mMerging) || mLanguages == null || mLanguages.Length == 0)
            {
                mDictionary.Clear();
                mLanguages = new string[header.size];
                if(!localizationHasBeenSet)
                {
                    mLanguage = DefaultLanguage;
                    localizationHasBeenSet = true;
                }
                for(var i = 0; i < header.size; ++i)
                {
                    mLanguages[i] = header[i];
                    if(mLanguages[i] == mLanguage)
                    {
                        mLanguageIndex = i;
                    }
                }
            }
            else
            {
                languagesToAdd = new string[header.size];
                for(var i = 0; i < header.size; ++i)
                {
                    languagesToAdd[i] = header[i];
                }

                // Automatically resize the existing languages and add the new language to the mix
                for(var i = 0; i < header.size; ++i)
                {
                    if(!HasLanguage(header[i]))
                    {
                        var newSize = mLanguages.Length + 1;
#if UNITY_FLASH
					string[] temp = new string[newSize];
					for (int b = 0, bmax = arr.Length; b < bmax; ++b) temp[b] = mLanguages[b];
					mLanguages = temp;
#else
                        Array.Resize(ref mLanguages, newSize);
#endif
                        mLanguages[newSize - 1] = header[i];
                        var newDict = new Dictionary<string, string[]>();
                        foreach(var pair in mDictionary)
                        {
                            var arr = pair.Value;
#if UNITY_FLASH
						temp = new string[newSize];
						for (int b = 0, bmax = arr.Length; b < bmax; ++b) temp[b] = arr[b];
						arr = temp;
#else
                            Array.Resize(ref arr, newSize);
#endif
                            arr[newSize - 1] = arr[0];
                            newDict.Add(pair.Key, arr);
                        }
                        mDictionary = newDict;
                    }
                }
            }
            var languageIndices = new Dictionary<string, int>();
            for(var i = 0; i < mLanguages.Length; ++i)
            {
                languageIndices.Add(mLanguages[i], i);
            }

            // Read the entire CSV file into memory
            for(;;)
            {
                var temp = reader.ReadCSV();
                if(temp == null || temp.size == 0)
                {
                    break;
                }
                if(string.IsNullOrEmpty(temp[0]))
                {
                    continue;
                }
                AddCSV(temp, languagesToAdd, languageIndices);
            }
            if(!mMerging && onLocalize != null)
            {
                mMerging = true;
                var note = onLocalize;
                onLocalize = null;
                note();
                onLocalize = note;
                mMerging = false;
            }
            return true;
        }

        /// <summary>
        ///     Load the specified localization dictionary.
        /// </summary>
        private static bool LoadDictionary(string value)
        {
            // Try to load the Localization CSV
            byte[] bytes = null;
            if(!localizationHasBeenSet)
            {
                if(loadFunction == null)
                {
                    var asset = Resources.Load<TextAsset>("Localization");
                    if(asset != null)
                    {
                        bytes = asset.bytes;
                    }
                }
                else
                {
                    bytes = loadFunction("Localization");
                }
                localizationHasBeenSet = true;
            }

            // Try to load the localization file
            if(LoadCSV(bytes))
            {
                return true;
            }
            if(string.IsNullOrEmpty(value))
            {
                value = mLanguage;
            }

            // If this point was reached, the localization file was not present
            if(string.IsNullOrEmpty(value))
            {
                return false;
            }

            // Not a referenced asset -- try to load it dynamically
            if(loadFunction == null)
            {
                var asset = Resources.Load<TextAsset>(strDic + "/" + value);
                if(asset != null)
                {
                    bytes = asset.bytes;
                }
            }
            else
            {
                bytes = loadFunction(value);
            }
            if(bytes != null)
            {
                Set(value, bytes);
                return true;
            }
            Debug.LogWarningFormat("[Localization] Can not find Localization at {0}", strDic + "/" + value);
            return false;
        }

        /// <summary>
        ///     Select the specified language from the previously loaded CSV file.
        /// </summary>
        private static bool SelectLanguage(string language)
        {
            mLanguageIndex = -1;
            if(mDictionary.Count == 0)
            {
                return false;
            }
            for(int i = 0, imax = mLanguages.Length; i < imax; ++i)
            {
                if(mLanguages[i] == language)
                {
                    mOldDictionary.Clear();
                    mLanguageIndex = i;
                    mLanguage = language;
                    PlayerPrefs.SetString("Language", mLanguage);
                    if(onLocalize != null)
                    {
                        onLocalize();
                    }
                    //UIRoot.Broadcast("OnLocalize");
                    return true;
                }
            }
            return false;
        }
    }
}