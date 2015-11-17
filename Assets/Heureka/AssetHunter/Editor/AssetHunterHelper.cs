using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HeurekaGames
{
    internal class AssetHunterHelper
    {

        private static int m_NumberOfDirectories;
        internal static bool HasBuildLogAvaliable()
        {
            string UnityEditorLogfile = GetLogFolderPath();
            string line = string.Empty;

            try
            {
                // Have to use FileStream to get around sharing violations!
                FileStream FS = new FileStream(UnityEditorLogfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader SR = new StreamReader(FS);

                while (!SR.EndOfStream && !(line = SR.ReadLine()).Contains("Mono dependencies included in the build")) ;
                while (!SR.EndOfStream && (line = SR.ReadLine()) != "")
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError(line + ": " + e);
                return false;
            }
        }

        public static string GetLogFolderPath()
        {
            string LocalAppData;
            string UnityEditorLogfile = string.Empty;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                LocalAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                UnityEditorLogfile = LocalAppData + "\\Unity\\Editor\\Editor.log";
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                LocalAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                UnityEditorLogfile = LocalAppData + "/Library/Logs/Unity/Editor.log";
            }
            else
                Debug.LogError("RuntimePlatform not known");

            return UnityEditorLogfile;
        }

        internal static AssetBuildReport AnalyzeBuildLog()
        {
            AssetBuildReport buildReport = new AssetBuildReport();
            string UnityEditorLogfile = GetLogFolderPath();

            try
            {
                // Have to use FileStream to get around sharing violations!
                FileStream FS = new FileStream(UnityEditorLogfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader SR = new StreamReader(FS);

                string line;
                int linesRead = 0;
                int lineIndex = 0;

                while (!SR.EndOfStream)
                {
                    line = SR.ReadLine();
                    linesRead++;
                    if ((line).Contains("Mono dependencies included in the build"))
                    {
                        lineIndex = linesRead;
                    }
                }

                FS.Position = 0;
                SR.DiscardBufferedData();

                //Start reading from log at the right line
                for (int i = 0; i < lineIndex - 1; i++)
                {

                    SR.ReadLine();
                }

                while (!SR.EndOfStream && !(line = SR.ReadLine()).Contains("Mono dependencies included in the build")) ;
                while (!SR.EndOfStream && (line = SR.ReadLine()) != "")
                {
                    int stringLength = line.Length;
                    int startIndex = line.LastIndexOf(" ");
                    buildReport.AddDependency(line.Substring(startIndex, stringLength - startIndex));
                }
                while (!SR.EndOfStream && !(line = SR.ReadLine()).Contains("Used Assets")) ;
                bool assetAnalysisComplete = false;
                while (!SR.EndOfStream && !assetAnalysisComplete)
                {
                    string curLine = SR.ReadLine();

                    if (curLine == "" || curLine.Contains("System memory in use before") || !curLine.Contains("% "))
                    {
                        assetAnalysisComplete = true;
                    }
                    else
                    {
                        if (!curLine.Contains("Built-in"))
                        {
                            string str = curLine.Substring(curLine.IndexOf("% ") + 2);
                            if (str.StartsWith("Assets/"))
                            {
                                EditorUtility.DisplayProgressBar(
                                    "Parsing build log",
                                    "Parsing build log to retrieve info",
                                    (float)SR.BaseStream.Position / (float)SR.BaseStream.Length);

                                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(str, typeof(UnityEngine.Object));

                                if (obj != null)
                                {
                                    BuildReportAsset asset = new BuildReportAsset();

                                    asset.SetAssetInfo(obj, str);
                                    //Split on whitespace
                                    string[] splitstring = curLine.Split(null);
                                    asset.SetSize(float.Parse(splitstring[1]), splitstring[2]);
                                    buildReport.AddAsset(asset);
                                }
                                else
                                {
                                    Debug.LogWarning(str + " is not a valid asset");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Debug.LogError("Error: " + E);
            }
            EditorUtility.ClearProgressBar();

            //TODO FIND AND ADD ICONS FOR BUILDTARGETS AND CERTIFICATES AND WHATEVER ELSE
            return buildReport;
        }

        internal static void PopulateUnusedList(AssetBuildReport buildLog, SortedDictionary<SerializableSystemType, bool> validTypeList)
        {
            //Count all directories in project for use in progress bar
            m_NumberOfDirectories = System.IO.Directory.GetDirectories(Application.dataPath, "*.*", System.IO.SearchOption.AllDirectories).Length;
            int directoriesTraversed = 0;

            //traverse directories
            traverseDirectory(-1, Application.dataPath, buildLog.m_BuildSizeList, 0, ref directoriesTraversed, validTypeList);
            EditorUtility.ClearProgressBar();
        }

        private static void traverseDirectory(int parentIndex, string path, List<BuildReportAsset> usedAssets, int heirarchyDepth, ref int directoriesTraversed, SortedDictionary<SerializableSystemType, bool> validTypeList)
        {
            directoriesTraversed++;

            EditorUtility.DisplayProgressBar(
                                "Traversing Directories",
                                "(" + directoriesTraversed + " of " + m_NumberOfDirectories + ") Analyzing " + path.Substring(path.IndexOf("/Assets") + 1),
                                (float)directoriesTraversed / (float)m_NumberOfDirectories);

            //Get the settings to exclude vertain folders or suffixes
            foreach (UnityEngine.Object dir in AssetHunterMainWindow.Instance.settings.m_DirectoryExcludes)
            {
                //TODO Can this be done more elegantly
                int startingIndex = Application.dataPath.Length - 6;
                string relativePath = path.Substring(startingIndex, path.Length - startingIndex);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(relativePath, typeof(UnityEngine.Object));

                if (dir == obj)
                {
                    //This folder was exluded
                    return;
                }
            }

            //Exclude types and folders that should not be reviewed
            string[] assetsInDirectory = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(name => !name.ToLowerInvariant().EndsWith(".meta")
                    && (!name.ToLowerInvariant().EndsWith(".unity"))
                    && (!name.ToLowerInvariant().EndsWith("thumbs.db"))
                    && (!name.ToLowerInvariant().EndsWith(".orig"))
                    && (!name.ToLowerInvariant().Contains(Path.DirectorySeparatorChar + "heureka" + Path.DirectorySeparatorChar))
                    && (!name.ToLowerInvariant().Contains(Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar))
                    && (!name.ToLowerInvariant().Contains(Path.DirectorySeparatorChar + "streamingassets" + Path.DirectorySeparatorChar))
                    && (!name.ToLowerInvariant().Contains(Path.DirectorySeparatorChar + "resources" + Path.DirectorySeparatorChar))
                    && (!name.ToLowerInvariant().Contains(Path.DirectorySeparatorChar + "editor default resources" + Path.DirectorySeparatorChar))
                    && (!name.ToLowerInvariant().Contains(Path.DirectorySeparatorChar + "editor" + Path.DirectorySeparatorChar))
                    && (!name.ToLowerInvariant().Contains(@".ds_store"))
                    && (!name.ToLowerInvariant().Contains(@".workspace.mel"))
                    && (!name.ToLowerInvariant().Contains(@".mayaswatches")))
                    .ToArray();

            for (int i = 0; i < assetsInDirectory.Length; i++)
            {
                assetsInDirectory[i] = assetsInDirectory[i].Substring(assetsInDirectory[i].IndexOf("/Assets") + 1);
                assetsInDirectory[i] = assetsInDirectory[i].Replace(@"\", "/");
            }

            //Find any assets that does not live in UsedAssets List
            var result = assetsInDirectory.Where(p => !usedAssets.Any(p2 => UnityEditor.AssetDatabase.GUIDToAssetPath(p2.GUID) == p));

            //Create new folder object
            ProjectFolderInfo afInfo = new ProjectFolderInfo();

            afInfo.DirectoryName = path.Substring(path.IndexOf("/Assets") + 1).Replace(@"\", "/");
            afInfo.ParentIndex = parentIndex;

            if (heirarchyDepth == 0)
                afInfo.FoldOut = true;

            //Add to static list
            AssetHunterMainWindow.Instance.AddProjectFolderInfo(afInfo);

            if (parentIndex != -1)
            {
                AssetHunterMainWindow.Instance.GetFolderList()[parentIndex].AddChildFolder(afInfo);
            }

            UnityEngine.Object objToFind;
            foreach (string assetName in result)
            {
                objToFind = AssetDatabase.LoadAssetAtPath(assetName, typeof(UnityEngine.Object));

                if (objToFind == null)
                {
                    Debug.LogWarning("Couldnt find " + assetName);
                    continue;
                }

                SerializableSystemType assetType = new SerializableSystemType(objToFind.GetType());

                if (assetType.SystemType != typeof(MonoScript) && (!AssetHunterMainWindow.Instance.settings.m_AssetTypeExcludes.Contains(assetType)))
                {
                    AssetObjectInfo newAssetInfo = new AssetObjectInfo(assetName, assetType);
                    afInfo.AddAsset(newAssetInfo);
                }

                objToFind = null;

                //Memory leak safeguard
                UnloadUnused();
            }

            string[] nextLevelDirectories = System.IO.Directory.GetDirectories(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);

            foreach (string nld in nextLevelDirectories)
            {
                traverseDirectory(AssetHunterMainWindow.Instance.GetFolderList().IndexOf(afInfo), nld, usedAssets, (heirarchyDepth + 1), ref directoriesTraversed, validTypeList);
            }
        }

        public static void UnloadUnused()
        {
#if UNITY_5
            EditorUtility.UnloadUnusedAssetsImmediate();
#else
            EditorUtility.UnloadUnusedAssets();
#endif

        }

        public static string GetUnityEngineAssemblyPath()
        {
            System.Reflection.Assembly assembly = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(val => val.Location.EndsWith("UnityEngine.dll"));
            return assembly.Location;
        }

        public static string GetScriptAssemblyPath()
        {
            DirectoryInfo rootDirInfo = Directory.GetParent(Application.dataPath);

            string[] assemblyDirectories = Directory.GetDirectories(@rootDirInfo.FullName, "ScriptAssemblies", SearchOption.AllDirectories);

            string scriptAssembly = string.Empty;

            foreach (string dir in assemblyDirectories)
            { 
                scriptAssembly = (Directory.GetFiles(dir, "*.dll").FirstOrDefault(val => val.EndsWith("Assembly-CSharp.dll")));

                if (!string.IsNullOrEmpty(scriptAssembly))
                    return scriptAssembly;
            }

            Debug.LogError("Could not find script assembly");
            return string.Empty;
        }

        //TODO Complete unused scripts analysis

       /* public static void GetAddedComponents()
        {
            string unityEngineAssemblyPath = GetUnityEngineAssemblyPath();
            string scriptAssemblyPath = GetScriptAssemblyPath();

            if (string.IsNullOrEmpty(scriptAssemblyPath))
            {
                Debug.LogError("ScriptAssembly could not be found");
                return;
            }

            Mono.Cecil.AssemblyDefinition scriptAssemblyDef = Mono.Cecil.AssemblyDefinition.ReadAssembly(scriptAssemblyPath);
            Mono.Cecil.AssemblyDefinition unityEngineAssemblyDef = Mono.Cecil.AssemblyDefinition.ReadAssembly(unityEngineAssemblyPath);

            Mono.Cecil.ModuleDefinition scriptModuleDef = scriptAssemblyDef.MainModule;
            Mono.Cecil.ModuleDefinition unityEngineModuleDef = unityEngineAssemblyDef.MainModule;

            //Find the MethodDefinition for the method you're interested in:
            Mono.Cecil.TypeDefinition unityGameObjectTypeDef = unityEngineModuleDef.Types.FirstOrDefault(x => x.Name == "GameObject"); // this is LINQ
            List<Mono.Cecil.MethodDefinition> addComponentMethods = unityGameObjectTypeDef.Methods.Where(x => x.Name == "AddComponent").ToList<Mono.Cecil.MethodDefinition>(); //if your method is void ShowTooltip(), just type in "ShowTooltip"

            foreach (Mono.Cecil.MethodDefinition def in addComponentMethods)
            {
                Debug.Log("************Method: " + def.FullName);
            }

            //Now parse all Classes (types) in your assembly, parse all their Methods, parse all their Instructions, parse all their Operands and see if it's referencing yourMethod.
            foreach (Mono.Cecil.TypeDefinition td in scriptModuleDef.Types)
                foreach (Mono.Cecil.MethodDefinition md in td.Methods)
                    if (md.HasBody)
                        foreach (Mono.Cecil.Cil.Instruction inst in md.Body.Instructions)
                            foreach (Mono.Cecil.MethodDefinition mDef in addComponentMethods)
                            {
                                //Perhaps some info on some to fix the generics issue http://stackoverflow.com/questions/4968755/mono-cecil-call-generic-base-class-method-from-other-assembly
                                if(inst!=null && inst.Operand!=null)
                                    if (inst.Operand.ToString().Contains("AddComponent"))
                                        if (inst.Operand.ToString() == mDef.FullName) //TODO: Need a better way to compare. Generics doesnt work
                                            Debug.Log("your method is being called here");
                            }
        }

        public static void PrintMethods(Mono.Cecil.MethodDefinition methodDef)
        {
            Console.WriteLine(methodDef.Name);
            foreach (var instruction in methodDef.Body.Instructions)
            {
                if (instruction.OpCode == Mono.Cecil.Cil.OpCodes.Ldfld)
                {
                    Mono.Cecil.MethodReference methodRef = instruction.Operand as Mono.Cecil.MethodReference;

                    if (methodRef != null)
                        Console.WriteLine("\t" + methodRef.Name);
                }
            }
        }*/
    }
}