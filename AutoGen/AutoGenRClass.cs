using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AutoGen
{
    public class AutoGenRClass
    {
        protected const string ClassUFile = @"R.cs";
        protected const string ClassUContent = "/*This is generated automatically. DONT EDIT BY HAND*/ \n" + "public static partial class R {{\n  {0}  \n}};";

        protected readonly string ClassUPath;
        protected readonly List<string> SearchPaths;
        protected readonly List<IAutoGenerator> autoGens = new List<IAutoGenerator>();

        public AutoGenRClass(string genCodePath, IEnumerable<string> searchPaths, params IAutoGenerator[] gens)
        {
            ClassUPath = genCodePath;
            SearchPaths = new List<string>(searchPaths);
            for(var i = 0; i < gens.Length; ++i)
            {
                autoGens.Add(gens[i]);
            }
        }

        public bool AddRes(string path)
        {
            var reValue = false;
            for(var i = 0; i < autoGens.Count; ++i)
            {
                if(autoGens[i].Belong(path))
                {
                    var success = autoGens[i].AddRes(path);
                    if(!reValue && success)
                    {
                        reValue = true;
                    }
                }
            }
            return reValue;
        }

        public bool DelRes(string path)
        {
            var reValue = false;
            for(var i = 0; i < autoGens.Count; ++i)
            {
                if(autoGens[i].Belong(path))
                {
                    var success = autoGens[i].DelRes(path);
                    if(!reValue && success)
                    {
                        reValue = true;
                    }
                }
            }
            return reValue;
        }

        public void GenRClass()
        {
            if(!Directory.Exists(ClassUPath))
            {
                Directory.CreateDirectory(ClassUPath);
            }
            var path = Path.Combine(ClassUPath, ClassUFile);
            var builder = new StringBuilder();
            for(var i = 0; i < autoGens.Count; ++i)
            {
                builder.AppendLine(autoGens[i].GenerateRClass());
            }
            File.WriteAllText(path, string.Format(ClassUContent, builder));
            AssetDatabase.Refresh();
        }

        public void SearchAll()
        {
            var resDirs = new List<DirectoryInfo>();
            for(var index = 0; index < SearchPaths.Count; index++)
            {
                var path = SearchPaths[index];
                resDirs.Add(new DirectoryInfo(path));
            }
            for(var index = 0; index < resDirs.Count; index++)
            {
                var e = resDirs[index];
                var files = e.GetFiles("*", SearchOption.AllDirectories).Where(file => !file.Name.EndsWith(".meta"));
                foreach(var f in files)
                {
                    for(var i = 0; i < autoGens.Count; ++i)
                    {
                        if(autoGens[i].Belong(f.FullName))
                        {
                            try
                            {
                                autoGens[i].AddRes(f.FullName);
                            }
                            catch(Exception ex)
                            {
                                Debug.LogErrorFormat(AutoGenHelper.LoadAssetAtFullPath<Object>(f.FullName),"{0}\nAuto generate class R failed in: {1}." ,ex.Message,f.FullName);
                            }
                        }
                    }
                }
            }
        }

        [MenuItem("Auto Generate/Generate R Class", false, 2)]
        public static void SearchAllAndGen()
        {
            //if(!EditorPrefs.HasKey("AutoGenSearchPath"))
            //{
            //    SelectSearchFolder();
            //}
            if(!EditorPrefs.HasKey("AutoGenCodePath"))
            {
                SelectGenFolder();
            }
            if(EditorPrefs.HasKey("AutoGenCodePath"))
            {
                var search = Directory.GetDirectories(Application.dataPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith("\\Resources"));
                var gen = EditorPrefs.GetString("AutoGenCodePath");
                var u = new AutoGenRClass(gen, search, new ConfigAutoGen(), new TextureAutoGen(), new AudioAutoGen(), new PrefabAutoGen(), new LanguageAutoGen(), new ShaderAutoGen(), new LuaAutoGen(), new SceneAutoGen());
                u.SearchAll();
                u.GenRClass();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select folder where r.cs generated", "OK");
            }
        }

        [MenuItem("Auto Generate/Select Generate Folder", false, 1)]
        public static void SelectGenFolder()
        {
            var path = Application.dataPath;
            if(EditorPrefs.HasKey("AutoGenCodePath"))
            {
                path = EditorPrefs.GetString("AutoGenCodePath");
            }
            var newPath = EditorUtility.OpenFolderPanel("Generate Folder", path, "");
            if(Directory.Exists(newPath))
            {
                EditorPrefs.SetString("AutoGenCodePath", newPath);
            }
        }

        //    }
        //        EditorPrefs.SetString("AutoGenSearchPath", newPath);
        //    {
        //    if(Directory.Exists(newPath))
        //    var newPath = EditorUtility.OpenFolderPanel("Search Folder", path, "");
        //    }
        //        path = EditorPrefs.GetString("AutoGenSearchPath");
        //    {
        //    if(EditorPrefs.HasKey("AutoGenSearchPath"))
        //    var path = Application.dataPath;
        //{
        //public static void SelectSearchFolder()

        //[MenuItem("Auto Generate/Select Search Folder",false,0)]
        //}
    }
}