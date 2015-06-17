using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HeurekaGames
{
    [System.Serializable]
    public class AssetBuildReport
    {
        [SerializeField]
        private List<string> m_includedDependencies = new List<string>();
        [SerializeField]
        public List<BuildReportAsset> m_BuildSizeList = new List<BuildReportAsset>();

        internal void AddDependency(string assemblyName)
        {
            m_includedDependencies.Add(assemblyName);
        }

        internal bool IsEmpty()
        {
            return m_BuildSizeList.Count == 0;
        }

        public List<string> IncludedDependencies
        {
            get { return m_includedDependencies; }
            set { m_includedDependencies = value; }
        }

        internal void AddAsset(BuildReportAsset asset)
        {
            m_BuildSizeList.Add(asset);
        }

        public int AssetCount
        {
            get { return m_BuildSizeList.Count; }
        }

        internal BuildReportAsset GetAssetAtIndex(int i)
        {
            return m_BuildSizeList[i];
        }

        internal void AddPrefabs(List<string> usedPrefabsInScenes)
        {
            foreach (string path in usedPrefabsInScenes)
            {
                EditorUtility.DisplayProgressBar(
                                "Adding prefabs",
                                "Analyzing scenes to get prefabs",
                                (float)usedPrefabsInScenes.IndexOf(path) / (float)usedPrefabsInScenes.Count);
                //Early out
                if (m_BuildSizeList.Exists(val => val.Path == path))
                    continue;

                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

                if (obj != null)
                {
                    BuildReportAsset newAsset = new BuildReportAsset();

                    newAsset.SetAssetInfo(obj, path);
                    newAsset.SetSize(0.0f, "--");
                    m_BuildSizeList.Add(newAsset);
                }
                else
                {
                    Debug.LogWarning(path + " is not a valid asset");
                }
            }

            EditorUtility.ClearProgressBar();
        }

        internal void AddPlatformSpecificAssets()
        {
            int counter = 0;
            int countTo = Enum.GetValues(typeof(BuildTargetGroup)).Length;

            //TODO Get all the different splash screens and config files somehow
            List<UnityEngine.Object> splash = new List<UnityEngine.Object>();    
            splash.Add(UnityEditor.PlayerSettings.xboxSplashScreen);

            //Loop the entries
            foreach (UnityEngine.Object obj in splash)
            {
                //Early out if it already exist
                if (obj == null || m_BuildSizeList.Exists(val => val.Path == AssetDatabase.GetAssetPath(obj)))
                    continue;

                BuildReportAsset newAsset = new BuildReportAsset();

                newAsset.SetAssetInfo(obj, AssetDatabase.GetAssetPath(obj));
                newAsset.SetSize(0.0f, "--");
                m_BuildSizeList.Add(newAsset);
            }

            //TODO And the icons from METRO as well

#if !UNITY_5
            List<string> targetResourcePaths = new List<string>();
            targetResourcePaths.Add(UnityEditor.PlayerSettings.Metro.certificatePath);
            targetResourcePaths.AddRange(getWin8AssetPaths());

            //Loop the paths
            foreach (string path in targetResourcePaths)
            {
                //Early out if it already exist
                if (string.IsNullOrEmpty(path) || m_BuildSizeList.Exists(val => val.Path == path))
                    continue;

                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

                if (obj != null)
                {
                    BuildReportAsset newAsset = new BuildReportAsset();
                    newAsset.SetAssetInfo(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), path);
                    newAsset.SetSize(0.0f, "--");
                    m_BuildSizeList.Add(newAsset);
                }
            }
#endif

            //Loop icons in buildtargetgroups
            foreach (BuildTargetGroup btg in (BuildTargetGroup[]) Enum.GetValues(typeof(BuildTargetGroup)))
            {
                EditorUtility.DisplayProgressBar(
                                "Add Target Specifics for " + btg.ToString(),
                                "Looking at icons and splash screens for targetgroups",
                                (float)counter / (float)countTo);

                Texture2D[] buildTargetGroupTextures = UnityEditor.PlayerSettings.GetIconsForTargetGroup(btg);

                foreach (Texture2D curIcon in buildTargetGroupTextures)
                { 
                    //Early out if it already exist
                    if (curIcon == null || m_BuildSizeList.Exists(val => val.Path == AssetDatabase.GetAssetPath(curIcon)))
                        continue;

                    BuildReportAsset newAsset = new BuildReportAsset();

                    newAsset.SetAssetInfo(curIcon, AssetDatabase.GetAssetPath(curIcon));
                    newAsset.SetSize(0.0f, "--");
                    m_BuildSizeList.Add(newAsset);
                }
                AssetHunterHelper.UnloadUnused();
            }

            EditorUtility.ClearProgressBar();
        }

#if !UNITY_5
        //Run through win8 assets to get icons etc, getting paths through reflection
        private List<string> getWin8AssetPaths()
        {

            List<String> reflectedPaths = new List<string>();

            PropertyInfo[] properties = typeof(UnityEditor.PlayerSettings.Metro).GetProperties(BindingFlags.Public | BindingFlags.Static);

            //From UnityEditor.PlayerSettings.Metro I get these property names
            string[] validProperties = new string[5]
                {
                    "tile",
                    "splash",
                    "logo",
                    "icon",
                    "certificatePath"
                };

            //TODO, run through these and find the ones with paths to icons etc
            foreach (PropertyInfo p in properties)
            {
                // Only work with strings
                if (p.PropertyType != typeof(string)) { continue; }

                // If not writable then cannot null it; if not readable then cannot check it's value
                if (!p.CanRead) { continue; }

                //See if the property name is similar to the predefined properties found in UnityEditor.PlayerSettings.Metro
                if (validProperties.Any(s => p.ToString().ToLowerInvariant().Contains(s))) continue;
                    
                MethodInfo mget = p.GetGetMethod(false);
                MethodInfo mset = p.GetSetMethod(false);

                // Get and set methods have to be public
                if (mget == null) { continue; }
                if (mset == null) { continue; }

                string path = p.GetValue(properties, null).ToString();

                reflectedPaths.Add(path);                
            }

            return reflectedPaths;
        }
#endif
    }


    [System.Serializable]
    public class BuildReportAsset : IEquatable<BuildReportAsset>
    {
        [SerializeField]
        private string m_name;
        [SerializeField]
        private string m_path;
        [SerializeField]
        private string m_assetGUID;
        [SerializeField]
        private SerializableSystemType m_type;
        [SerializeField]
        private float m_assetSize;
        [SerializeField]
        private string m_sizePostFix;
        [SerializeField]
        private bool m_bShowSceneDependency;
        [SerializeField]
        private UnityEngine.Object[] m_sceneDependencies;
        [SerializeField]
        private bool m_bFoldOut;

        internal void SetAssetInfo(UnityEngine.Object obj, string path)
        {
            this.m_path = path;
            string[] parts = path.Split('/');
            this.m_name = parts[parts.Length - 1];

            m_assetGUID = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            m_type = new SerializableSystemType(obj.GetType());

            AssetHunterHelper.UnloadUnused();     
        }


        internal void SetSize(float assetSize, string postFix)
        {
            m_assetSize = assetSize;
            m_sizePostFix = postFix;
        }

        public bool Equals(BuildReportAsset other)
        {
            if (other!=null)
            return this.m_assetGUID == other.m_assetGUID &&
                   this.m_name == other.m_name &&
                   this.m_path == other.m_path;

            Debug.LogWarning("Something when wrong in parsing, compare object is null");
            return false;
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public SerializableSystemType Type
        {
            get { return m_type; }
        }

        public string GUID
        {
            get { return m_assetGUID; }
        }

        public string Path
        {
            get { return m_path; }
        }

        public float Size
        {
            get { return m_assetSize; }
        }

        public string SizePostFix
        {
            get { return m_sizePostFix; }
        }

        internal void ToggleShowSceneDependency()
        {
            m_bShowSceneDependency = !m_bShowSceneDependency;
            m_bFoldOut = m_bShowSceneDependency;

            if (m_bShowSceneDependency == false)
                m_sceneDependencies = null;
        }

        internal void SetSceneDependencies(UnityEngine.Object[] scenes)
        {
            m_sceneDependencies = scenes;
        }

        public bool ShouldShowDependencies
        {
            get { return m_bShowSceneDependency; }
        }

        public bool FoldOut
        {
            get { return m_bFoldOut; }
            set { m_bFoldOut = value; }
        }

        internal UnityEngine.Object[] GetSceneDependencies()
        {
            return m_sceneDependencies;
        }
    }
}
