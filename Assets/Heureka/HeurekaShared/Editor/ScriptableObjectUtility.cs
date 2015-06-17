using UnityEngine;
using UnityEditor;
using System.IO;

namespace HeurekaGames
{
    public static class ScriptableObjectUtility
    {
        /// <summary>
        /// Generic method to crate a scriptable object
        /// </summary>
        /// <typeparam name="T">Type of scriptableObject</typeparam>
        /// <param name="path">Where to store the file</param>
        public static void CreateAsset<T>(string path) where T : ScriptableObject
        {
            CreateAsset<T>(path, "New_" + typeof(T).ToString());
        }

        /// <summary>
        /// Generic method to crate a scriptable object
        /// </summary>
        /// <typeparam name="T">Type of scriptableObject</typeparam>
        /// <param name="path">Where to store the file</param>
        public static void CreateAsset<T>(string path, string name) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + name + ".asset");

            if (UnityEditor.EditorUtility.DisplayDialog("Asset created", "Created as " + path + name, "Ok"))
            {
                AssetDatabase.CreateAsset(asset, assetPathAndName);

                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }
        }
    }
}