using System.Collections.Generic;
using System.Linq;

namespace HeurekaGames
{
    [System.Serializable]
    public class ProjectFolderInfo
    {
        [UnityEngine.SerializeField]
        private List<AssetObjectInfo> m_assetList = new List<AssetObjectInfo>();

        [UnityEngine.SerializeField]
        private List<int> m_childFolderIndexers = new List<int>();

        [UnityEngine.SerializeField]
        string m_directoryName;

        [UnityEngine.SerializeField]
        private int m_ParentListIndex;

        [UnityEngine.SerializeField]
        int m_assetsInChildren = 0;

        public bool FoldOut = false;

        internal void AddAsset(AssetObjectInfo assetInfo)
        {
            if (AssetList == null)
                AssetList = new List<AssetObjectInfo>();

            assetInfo.SetParent(this);
            AssetList.Add(assetInfo);
        }

        public List<AssetObjectInfo> AssetList
        {
            get { return m_assetList; }
            set { m_assetList = value; }
        }
        public List<int> ChildFolderIndexers
        {
            get { return m_childFolderIndexers; }
            set { m_childFolderIndexers = value; }
        }
        public string DirectoryName
        {
            get { return m_directoryName; }
            set { m_directoryName = value; }
        }
        public int ParentIndex
        {
            get { return m_ParentListIndex; }
            set { m_ParentListIndex = value; }
        }

        internal bool ShouldBeListed(SortedDictionary<SerializableSystemType, bool> validTypeList)
        {
            return m_assetsInChildren >= 1 && hasValidAssetInChildren(this, validTypeList);
        }

        public void RecalcChildAssets(SortedDictionary<SerializableSystemType, bool> validTypeList)
        {
            CountChildren(validTypeList);

            if (m_ParentListIndex != -1)
                AssetHunterMainWindow.Instance.ReCalcUnusedAssetsFromIndex(m_ParentListIndex);
        }

        private bool hasValidAssetInChildren(ProjectFolderInfo assetFolderInfo, SortedDictionary<SerializableSystemType, bool> validTypeList)
        {
            foreach (AssetObjectInfo aInfo in assetFolderInfo.AssetList)
            {
                if ((validTypeList.ContainsKey(aInfo.m_Type) && validTypeList[aInfo.m_Type] == true))
                    return true;
            }

            bool foundValidAsset = false;
            foreach (int indexer in assetFolderInfo.m_childFolderIndexers)
            {
                foundValidAsset = hasValidAssetInChildren(AssetHunterMainWindow.Instance.GetFolderList()[indexer], validTypeList);

                if (foundValidAsset)
                    return true;
            }

            return false;
        }

        private int calcAssetsInChildren(ProjectFolderInfo assetFolderInfo, SortedDictionary<SerializableSystemType, bool> validTypeList)
        {
            int value = 0;
            foreach (int indexer in assetFolderInfo.m_childFolderIndexers)
            {
                value += AssetHunterMainWindow.Instance.GetFolderList()[indexer].m_assetsInChildren = calcAssetsInChildren(AssetHunterMainWindow.Instance.GetFolderList()[indexer], validTypeList);
            }
            return (value + (assetFolderInfo.AssetList.Where(val => (validTypeList.ContainsKey(val.m_Type) && validTypeList[val.m_Type]) == true)).Count());
        }

        internal void AddChildFolder(ProjectFolderInfo afInfo)
        {
            if (m_childFolderIndexers == null)
                m_childFolderIndexers = new List<int>();

            AssetHunterMainWindow.Instance.AddProjectFolderInfo(afInfo);

            m_childFolderIndexers.Add(AssetHunterMainWindow.Instance.GetFolderList().IndexOf(afInfo));
        }

        internal void CountChildren(SortedDictionary<SerializableSystemType, bool> validTypeList)
        {
            m_assetsInChildren = calcAssetsInChildren(this, validTypeList);
        }

        internal int GetAssetCountInChildren()
        {
            return m_assetsInChildren;
        }

        internal void RemoveAsset(AssetObjectInfo assetObjectInfo)
        {
            m_assetList.Remove(assetObjectInfo);
        }
    }

    [System.Serializable]
    public class AssetObjectInfo
    {
        public string m_Path;

        [UnityEngine.SerializeField]
        public SerializableSystemType m_Type;

        public string m_Name;
        public string m_ParentPath;

        public AssetObjectInfo(string path, SerializableSystemType type)
        {
            this.m_Path = path;
            string[] parts = path.Split('/');
            this.m_Name = parts[parts.Length - 1];
            this.m_Type = type;
        }

        internal void Delete(SortedDictionary<SerializableSystemType, bool> validTypeList)
        {
            ProjectFolderInfo parentFolder = AssetHunterMainWindow.Instance.GetFolderInfo(m_ParentPath);
            parentFolder.RemoveAsset(this);

            UnityEditor.AssetDatabase.DeleteAsset(m_Path);
        }

        internal void SetParent(ProjectFolderInfo projectFolderInfo)
        {
            m_ParentPath = projectFolderInfo.DirectoryName;
        }
    }
}
