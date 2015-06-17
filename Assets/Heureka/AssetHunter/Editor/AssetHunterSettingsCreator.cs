using UnityEngine;
using UnityEditor;
using System.Collections;
using HeurekaGames;

namespace HeurekaGames
{
    public class AssetHunterSettingsCreator : MonoBehaviour
    {
        public const string NAME = "AssetHunterSettingsData";

        internal static string GetAssetPath()
        {
            //SettingsData
            string[] data = AssetDatabase.FindAssets("AssetHunterSettingsData", null);
            if (data.Length >= 1)
            {

                return AssetDatabase.GUIDToAssetPath(data[0]);
            }

            return null;
        }
    }
}