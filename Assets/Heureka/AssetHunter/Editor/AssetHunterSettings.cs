using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HeurekaGames
{
    [System.Serializable]
    public class AssetHunterSettings : ScriptableObject
    {
        [SerializeField]
        public List<Object> m_DirectoryExcludes = new List<Object>();

        [SerializeField]
        public List<SerializableSystemType> m_AssetTypeExcludes = new List<SerializableSystemType>();

        internal static string GetAssetPath()
        {
            return AssetHunterSettingsCreator.GetAssetPath();
        }

        internal bool ValidateDirectory(Object newDir)
        {
            return !m_DirectoryExcludes.Contains(newDir);
        }

        internal void ExcludeDirectory(Object newDir)
        {
            m_DirectoryExcludes.Add(newDir);
        }

        internal bool ValidateType(SerializableSystemType newtype)
        {
            return !m_AssetTypeExcludes.Contains(newtype);
        }

        internal void ExcludeType(SerializableSystemType newtype)
        {
            m_AssetTypeExcludes.Add(newtype);
        }

        internal void RemoveDirectoryAtIndex(int indexer)
        {
            m_DirectoryExcludes.RemoveAt(indexer);
            EditorUtility.SetDirty(this);
        }

        internal void RemoveTypeAtIndex(int indexer)
        {
            m_AssetTypeExcludes.RemoveAt(indexer);
            EditorUtility.SetDirty(this);
        }

        internal bool HasExcludes()
        {
            return m_DirectoryExcludes.Count >= 1 || m_AssetTypeExcludes.Count >= 1;
        }

        internal bool HasDirectoryExcludes()
        {
            return m_DirectoryExcludes.Count >= 1;
        }

        internal bool HasTypeExcludes()
        {
            return m_AssetTypeExcludes.Count >= 1;
        }
    }
}
