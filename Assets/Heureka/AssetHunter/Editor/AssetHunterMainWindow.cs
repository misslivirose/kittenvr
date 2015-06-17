using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HeurekaGames;

public class AssetHunterMainWindow : EditorWindow, ISerializationCallbackReceiver
{
    public enum AssetHunterWindowState
    {
        BuildReport,
        UnusedAssets,
        //Settings
    }

    [SerializeField]
    public AssetHunterSettings settings;

    private AssetHunterWindowState m_WindowState = AssetHunterWindowState.UnusedAssets;
    private static AssetHunterMainWindow m_window;
    private Vector2 scrollPos;

    //If we should should the excluded folders/types foldout
    private bool bShowExcludeFoldout;

    [UnityEngine.SerializeField]
    public List<ProjectFolderInfo> m_ProjectFolderList = new List<ProjectFolderInfo>();

    //Dictionary and list to help serialization of unusedTypeDict
    public List<SerializableSystemType> m_unusedTypeListKeysSerializer = new List<SerializableSystemType>();
    public List<bool> m_unusedTypeListValuesSerializer = new List<bool>();
    public SortedDictionary<SerializableSystemType, bool> m_unusedTypeDict = new SortedDictionary<SerializableSystemType, bool>(new SerializableSystemTypeComparer());

    //Dictionary and list to help serialization of unusedTypeDict
    public List<SerializableSystemType> m_usedTypeListKeysSerializer = new List<SerializableSystemType>();
    public List<bool> m_usedTypeListValuesSerializer = new List<bool>();
    public SortedDictionary<SerializableSystemType, bool> m_usedTypeDict = new SortedDictionary<SerializableSystemType, bool>(new SerializableSystemTypeComparer());

    //Dictionary that holds the dependencies of assets and the scenes that contain them
    public List<string> m_assetSceneDependencyKeysSerializer = new List<string>();
    public List<StringListWrapper> m_assetSceneDependencyValueSerializer = new List<StringListWrapper>();
    public Dictionary<string, List<string>> m_assetSceneDependencies = new Dictionary<string, List<string>>();


    //Workaround to serialize dictionaries in Unity
    #region DictionarySerialization

    public void OnBeforeSerialize()
    {
        m_unusedTypeListKeysSerializer.Clear();
        m_unusedTypeListValuesSerializer.Clear();
        foreach (var kvp in m_unusedTypeDict)
        {
            m_unusedTypeListKeysSerializer.Add(kvp.Key);
            m_unusedTypeListValuesSerializer.Add(kvp.Value);
        }

        m_usedTypeListKeysSerializer.Clear();
        m_usedTypeListValuesSerializer.Clear();
        foreach (var kvp in m_usedTypeDict)
        {
            m_usedTypeListKeysSerializer.Add(kvp.Key);
            m_usedTypeListValuesSerializer.Add(kvp.Value);
        }

        m_assetSceneDependencyKeysSerializer.Clear();
        m_assetSceneDependencyValueSerializer.Clear();
        foreach (var kvp in m_assetSceneDependencies)
        {
            m_assetSceneDependencyKeysSerializer.Add(kvp.Key);
            m_assetSceneDependencyValueSerializer.Add(new StringListWrapper(kvp.Value));
        }
    }

    public void OnAfterDeserialize()
    {
        m_folderMarkedForDeletion = null;
        m_assetMarkedForDeletion = null;

        m_unusedTypeDict.Clear();
        for (int i = 0; i != Mathf.Min(m_unusedTypeListKeysSerializer.Count, m_unusedTypeListValuesSerializer.Count); i++)
        {
            m_unusedTypeDict.Add(m_unusedTypeListKeysSerializer[i], m_unusedTypeListValuesSerializer[i]);
        }

        m_usedTypeDict.Clear();
        for (int i = 0; i != Mathf.Min(m_usedTypeListKeysSerializer.Count, m_usedTypeListValuesSerializer.Count); i++)
        {
            m_usedTypeDict.Add(m_usedTypeListKeysSerializer[i], m_usedTypeListValuesSerializer[i]);
        }

        m_assetSceneDependencies.Clear();
        for (int i = 0; i != Mathf.Min(m_assetSceneDependencyKeysSerializer.Count, m_assetSceneDependencyValueSerializer.Count); i++)
        {
            m_assetSceneDependencies.Add(m_assetSceneDependencyKeysSerializer[i], m_assetSceneDependencyValueSerializer[i].list);
        }
    }
    #endregion

    private bool m_showTypesFoldout = true;

    [SerializeField]
    private AssetObjectInfo m_assetMarkedForDeletion = null;
    [SerializeField]
    public ProjectFolderInfo m_folderMarkedForDeletion = null;

    public bool m_BuildLogExists { get; set; }
    public bool m_BuildLogLoaded { get; set; }

    private Texture2D m_UIWarning;
    private Texture2D m_UISmallLogo;
    private Texture2D m_UIWideLogo;
    private Texture2D m_UIAchievementIcon;
    private Texture2D m_UISceneSelect;
    private Texture2D m_UIFolderSelect;
    private Texture2D m_UISettings;

    [SerializeField]
    private AssetBuildReport m_BuildLog;

    [SerializeField]
    private bool m_newBuildReady;
    private bool m_TypeChangeDetected;

    //BtnWidth
    float btnMinWidth = 180;
    int btnImageSize = 16;

    //Save initial Color
    private static Color m_IntialGUIColor;

    //Add menu named "Asset Hunter" to the window menu  
    [UnityEditor.MenuItem("Window/Asset Hunter _%h", priority = 1)]
    public static void InitDebugWindow()
    {
        if (!m_window)
            doInit();
    }

    private static AssetHunterMainWindow doInit()
    {
        m_IntialGUIColor = GUI.color;

        m_window = EditorWindow.GetWindow<AssetHunterMainWindow>();
        m_window.Show();

        loadEditorResources();

        return m_window;
    }

    private static void loadEditorResources()
    {
        //Small logo
        string[] GUIDsmallLogo = AssetDatabase.FindAssets("AssetHunterLogoSmall t:texture2D", null);
        if (GUIDsmallLogo.Length >= 1)
        {
            m_window.m_UISmallLogo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDsmallLogo[0]), typeof(Texture2D)) as Texture2D;
            EditorUtils.SetWindowValues(m_window, m_window.m_UISmallLogo, "Asset Hunter");
        }

        //Warning
        string[] GUIDwarning = AssetDatabase.FindAssets("AssetHunterWarning t:texture2D", null);
        if (GUIDwarning.Length >= 1)
        {
            m_window.m_UIWarning = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDwarning[0]), typeof(Texture2D)) as Texture2D;
        }

        //wide logo
        string[] GUIDwideLogo = AssetDatabase.FindAssets("AssetHunterLogoWide t:texture2D", null);
        if (GUIDwideLogo.Length >= 1)
        {
            m_window.m_UIWideLogo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDwideLogo[0]), typeof(Texture2D)) as Texture2D;
        }

        //Achievement
        string[] GUIDachievement = AssetDatabase.FindAssets("AssetHunterAchievementUnlocked t:texture2D", null);
        if (GUIDachievement.Length >= 1)
        {
            m_window.m_UIAchievementIcon = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDachievement[0]), typeof(Texture2D)) as Texture2D;
        }

        //SceneSelect
        string[] GUIDsceneSelect = AssetDatabase.FindAssets("AssetHunterSceneSelect t:texture2D", null);
        if (GUIDsceneSelect.Length >= 1)
        {
            m_window.m_UISceneSelect = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDsceneSelect[0]), typeof(Texture2D)) as Texture2D;
        }

        //FolderSelect
        string[] GUIDfolderSelect = AssetDatabase.FindAssets("AssetHunterFolderSelect t:texture2D", null);
        if (GUIDfolderSelect.Length >= 1)
        {
            m_window.m_UIFolderSelect = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDfolderSelect[0]), typeof(Texture2D)) as Texture2D;
        }

        //Settings
        string[] GUIDsettings = AssetDatabase.FindAssets("AssetHunterSettings t:texture2D", null);
        if (GUIDsettings.Length >= 1)
        {
            m_window.m_UISettings = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDsettings[0]), typeof(Texture2D)) as Texture2D;
        }

        string path = AssetHunterSettingsCreator.GetAssetPath();
        AssetHunterMainWindow.Instance.settings = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject)) as AssetHunterSettings;
    }

    void OnInspectorUpdate()
    {
        if (!m_window)
            doInit();
    }

    void OnGUI()
    {

        OnShowDefaultGUI();

        if (m_BuildLogLoaded)
        {
            if (m_WindowState == AssetHunterWindowState.UnusedAssets)
                OnUnusedAssetsUIUpdate();
            else if (m_WindowState == AssetHunterWindowState.BuildReport)
                OnBuildReportUIUpdate();
        }
    }

    private void OnShowDefaultGUI()
    {
        EditorGUILayout.LabelField("Asset Hunter v1.3.3", EditorStyles.boldLabel);

        //Show logo
        if (m_window && m_window.m_UIWideLogo)
            GUILayout.Label(m_window.m_UIWideLogo);

        //If there is no valid build log
        if (!m_BuildLogExists)
        {
            m_BuildLogExists = AssetHunterHelper.HasBuildLogAvaliable();

            if (!m_BuildLogExists)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(m_UIWarning);
                GUILayout.Label("Go build your project in order for this tool to function...(Ctrl+Shift+B)", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        string buildLogButtonText;

        //Settings
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button(new GUIContent("Edit Settings", m_UISettings), GUILayout.Width(btnMinWidth - 70), GUILayout.Height(20)))
        {
            EditorWindow.GetWindow<AssetHunterSettingsWindow>(true, "Asset Hunter Settings");
        }


        EditorGUI.indentLevel = 0;

        //Only show the foldout if we actually have any manually excluded folders or types
        if (settings.HasExcludes())
            bShowExcludeFoldout = EditorGUILayout.Foldout(bShowExcludeFoldout, "Show manual excludes");

        if (bShowExcludeFoldout)
        {
            if (settings.HasDirectoryExcludes())
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.LabelField("Excluded Directories", EditorStyles.boldLabel);

                EditorGUI.indentLevel = 2;
                foreach (Object obj in settings.m_DirectoryExcludes)
                    EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(obj), EditorStyles.miniLabel);
            }
            if (settings.HasTypeExcludes())
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.LabelField("Excluded Types", EditorStyles.boldLabel);

                EditorGUI.indentLevel = 2;
                foreach (SerializableSystemType sType in settings.m_AssetTypeExcludes)
                {
                    EditorGUILayout.LabelField(sType.Name);
                }
            }
        }

        EditorGUI.indentLevel = 0;
        EditorGUILayout.EndVertical();

        GUILayout.Label("-------------------------------Build Log--------------------------------");

        //If build log up to date
        if (!m_newBuildReady)
        {
            buildLogButtonText = m_BuildLogLoaded ? "Log updated (refresh)" : "Load Build Log (Required)";
            GUI.color = m_BuildLogLoaded ? Color.green : Color.red;
        }

        //If build log outdated
        else
        {
            buildLogButtonText = "Log outdated(Refresh)";
            GUI.color = Color.yellow;
        }

        EditorGUILayout.BeginHorizontal();

        //Load the Editor build log
        if (GUILayout.Button(buildLogButtonText, GUILayout.MinWidth(btnMinWidth)))
        {
            loadEditorLog();
            return;
        }
        EditorGUILayout.Space();

        GUI.color = Color.cyan;

        if (GUILayout.Button("Open Log", GUILayout.MinWidth(btnMinWidth)))
        {
            System.Diagnostics.Process.Start(AssetHunterHelper.GetLogFolderPath());
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        GUILayout.Label("------------------------------Select Mode------------------------------");
        EditorGUILayout.BeginHorizontal();

        //Choose window state
        GUI.color = (m_WindowState == AssetHunterWindowState.UnusedAssets) ? Color.gray : m_IntialGUIColor;
        if (GUILayout.Button(AssetHunterWindowState.UnusedAssets.ToString(), GUILayout.MinWidth(btnMinWidth)))
        {
            changeState(AssetHunterWindowState.UnusedAssets);
        }
        EditorGUILayout.Space();
        GUI.color = (m_WindowState == AssetHunterWindowState.BuildReport) ? Color.gray : m_IntialGUIColor;
        if (GUILayout.Button(AssetHunterWindowState.BuildReport.ToString(), GUILayout.MinWidth(btnMinWidth)))
        {
            changeState(AssetHunterWindowState.BuildReport);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //Reset GUI Color
        GUI.color = m_IntialGUIColor;
    }

    private void changeState(AssetHunterWindowState newState)
    {
        m_WindowState = newState;
    }

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (m_window)
        {
            m_window.m_newBuildReady = true;
        }
    }

    private void OnBuildReportUIUpdate()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginHorizontal();

        //Show all used types
        EditorGUILayout.BeginVertical();
        showTypesUI(m_usedTypeDict);
        EditorGUILayout.EndVertical();

        //Show included assemblies
        showDependenciesUI();

        EditorGUILayout.EndHorizontal();

        //Show the used assets
        showBuildAssetInfoUI();

        if (m_TypeChangeDetected)
        {
            //Type change detected
        };

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndScrollView();
    }

    private void OnUnusedAssetsUIUpdate()
    {
        if (m_ProjectFolderList == null || m_ProjectFolderList.Count <= 0)
            return;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginVertical();

        if (m_unusedTypeDict.Count >= 1)
        {
            GUILayout.Label("Select UnusedAssets to view the project assets not used in last build");
            showTypesUI(m_unusedTypeDict);

            if (m_TypeChangeDetected)
            {
                m_ProjectFolderList[0].RecalcChildAssets(m_unusedTypeDict);
            };

            showUnusedAssets();
        }
        else
        {
            GUILayout.Label("Good job!!! Your project is completely clean. Give yourself a pat on the back", EditorStyles.boldLabel);
            GUILayout.Label(m_UIAchievementIcon);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void showUnusedAssets()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Collapse All", GUILayout.Width(btnMinWidth)))
        {
            foreach (ProjectFolderInfo folder in m_ProjectFolderList)
            {
                folder.FoldOut = false;
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Expand All", GUILayout.Width(btnMinWidth)))
        {
            foreach (ProjectFolderInfo folder in m_ProjectFolderList)
            {
                folder.FoldOut = true;
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        int indentLevel = 0;

        drawAssetFolderInfoRecursively(m_ProjectFolderList[0], indentLevel);

        if (m_assetMarkedForDeletion != null)
        {
            if (EditorUtility.DisplayDialog("Delete asset", "Are you sure you want to delete " + m_assetMarkedForDeletion.m_Name, "Yes", "No"))
            {
                m_assetMarkedForDeletion.Delete(m_unusedTypeDict);
                //Delete potential empty folders
                processDirectory(getSystemFolderPath(m_assetMarkedForDeletion.m_ParentPath));
                m_assetMarkedForDeletion = null;
            }
            else
                m_assetMarkedForDeletion = null;
        }
        else if (m_folderMarkedForDeletion != null)
        {
            if (EditorUtility.DisplayDialog("Delete all child assets", "Are you sure you want to delete all " + m_folderMarkedForDeletion.GetAssetCountInChildren() + " child assets", "Yes", "No"))
            {
                List<AssetObjectInfo> objectsToDelete = new List<AssetObjectInfo>();
                getObjectsMarkedForDeletion(m_folderMarkedForDeletion, ref objectsToDelete);

                deleteSelected(objectsToDelete);
                //Delete potential empty folders
                processDirectory(getSystemFolderPath(m_folderMarkedForDeletion.DirectoryName));
                m_folderMarkedForDeletion = null;
                refreshUnusedAssets();
            }
            else
            {
                m_folderMarkedForDeletion = null;
            }
        }
    }

    private string getSystemFolderPath(string assetPath)
    {
        return Application.dataPath.Substring(0, Application.dataPath.IndexOf("/Assets") + 1) + assetPath;
    }

    private static void processDirectory(string startLocation)
    {
        foreach (var directory in System.IO.Directory.GetDirectories(startLocation, "*.*", System.IO.SearchOption.TopDirectoryOnly))
        {
            processDirectory(directory);
        }

        if (System.IO.Directory.GetFiles(startLocation).Where(path => !path.EndsWith(".meta")).Count() == 0 &&
        System.IO.Directory.GetDirectories(startLocation).Length == 0)
        {
            FileUtil.DeleteFileOrDirectory(startLocation);
            AssetDatabase.Refresh();
        }
    }

    private void deleteSelected(List<AssetObjectInfo> objectsToDelete)
    {
        for (int i = 0; i < objectsToDelete.Count; i++)
        {
            UnityEditor.EditorUtility.DisplayProgressBar("Deleting unused assets", "Currently deleting " + i + "/" + objectsToDelete.Count, (float)i / (float)objectsToDelete.Count);
            objectsToDelete[i].Delete(m_unusedTypeDict);
        }
        UnityEditor.EditorUtility.ClearProgressBar();
    }

    private void getObjectsMarkedForDeletion(ProjectFolderInfo aFInfo, ref List<AssetObjectInfo> objectsToDelete)
    {
        int childAssetCount = aFInfo.AssetList.Count;
        int childFolderCount = aFInfo.ChildFolderIndexers.Count;

        for (int i = childAssetCount - 1; i > -1; i--)
        {
            if ((m_unusedTypeDict.ContainsKey(aFInfo.AssetList[i].m_Type) && m_unusedTypeDict[aFInfo.AssetList[i].m_Type] == true))
                objectsToDelete.Add(aFInfo.AssetList[i]);
        }

        for (int i = childFolderCount - 1; i > -1; i--)
        {
            getObjectsMarkedForDeletion(m_ProjectFolderList[aFInfo.ChildFolderIndexers[i]], ref objectsToDelete);
        }
    }

    private void showTypesUI(SortedDictionary<SerializableSystemType, bool> typeList)
    {
        EditorGUI.indentLevel = 0;
        m_showTypesFoldout = EditorGUILayout.Foldout(m_showTypesFoldout, "Show Types");

        m_TypeChangeDetected = false;
        if (m_showTypesFoldout)
        {
            EditorGUI.indentLevel = 1;
            foreach (var key in typeList.Keys.ToList())
            {
                bool lastVal = typeList[key];
                typeList[key] = EditorGUILayout.ToggleLeft(key.Name, typeList[key]);
                if (lastVal != typeList[key])
                    m_TypeChangeDetected = true;
            }
        }
        EditorGUILayout.Space();
    }

    private void showBuildAssetInfoUI()
    {

        EditorGUILayout.BeginVertical();
        EditorGUI.indentLevel = 0;

        GUILayout.Label("--------------------------------LEGEND--------------------------------");

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label(m_UISceneSelect, GUILayout.Width(25), GUILayout.Height(25));
        GUILayout.Label("Select all scenes that reference this asset: ", GUILayout.Width(300));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label(m_UIFolderSelect, GUILayout.Width(25), GUILayout.Height(25));
        GUILayout.Label("Locate asset in project view: ", GUILayout.Width(300));
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("-------------------------------------------------------------------------");

        GUILayout.Space(15);
        EditorGUILayout.LabelField("Assets included in build", EditorStyles.boldLabel);

        EditorGUI.indentLevel = 1;
        for (int i = 0; i < m_BuildLog.AssetCount; i++)
        {
            if (m_usedTypeDict[m_BuildLog.GetAssetAtIndex(i).Type] == true)
            {
                BuildReportAsset asset = m_BuildLog.GetAssetAtIndex(i);
                EditorGUILayout.BeginHorizontal();

                GUILayout.Space(10);
                if (GUILayout.Button(m_UISceneSelect, GUIStyle.none, GUILayout.Width(btnImageSize), GUILayout.Height(btnImageSize)))
                {
                    //Toggle if we should show scene dependency
                    asset.ToggleShowSceneDependency();

                    if (m_assetSceneDependencies.ContainsKey(asset.Path))
                    {
                        List<string> scenes = m_assetSceneDependencies[asset.Path];

                        UnityEngine.Object[] selectedObjects = new Object[scenes.Count];

                        for (int j = 0; j < scenes.Count; j++)
                        {
                            selectedObjects[j] = AssetDatabase.LoadAssetAtPath(scenes[j], typeof(UnityEngine.Object));
                        }

                        Selection.activeObject = null;
                        Selection.objects = selectedObjects;
                        asset.SetSceneDependencies(selectedObjects);
                    }
                }
                GUILayout.Space(10);

                if (GUILayout.Button(m_UIFolderSelect, GUIStyle.none, GUILayout.Width(btnImageSize), GUILayout.Height(btnImageSize)))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath(asset.Path, asset.Type.SystemType);
                    EditorGUIUtility.PingObject(Selection.activeObject);
                }

                EditorGUILayout.LabelField(asset.Name, GUILayout.Width(350));

                EditorGUILayout.LabelField(asset.Size + " " + asset.SizePostFix, GUILayout.Width(75));
                GUILayout.Label(EditorGUIUtility.ObjectContent(null, asset.Type.SystemType).image, GUILayout.Width(btnImageSize), GUILayout.Height(btnImageSize));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (asset.ShouldShowDependencies)
                {
                    EditorGUI.indentLevel = 2;
                    //If the asset is actually reference in scenes
                    if (asset.GetSceneDependencies() != null && asset.GetSceneDependencies().Length >= 1)
                    {
                        asset.FoldOut = EditorGUILayout.Foldout(asset.FoldOut, "View scenes with dependency");
                        if (asset.FoldOut)
                        {
                            foreach (Object obj in asset.GetSceneDependencies())
                            {
                                if (obj != null)
                                    EditorGUILayout.ObjectField(obj, typeof(Object), false);
                            }
                        }
                    }
                    //If no scenes reference the asset
                    else
                    {
                        Color initialColor = GUI.color;
                        GUI.color = Color.red;
                        EditorGUILayout.LabelField("Asset not referenced in any scene, most likely a part of the \"resources\" foldes", EditorStyles.whiteLabel);
                        GUI.color = initialColor;
                    }
                }
                EditorGUI.indentLevel = 1;
            }
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }

    private void showDependenciesUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUI.indentLevel = 0;

        EditorGUILayout.LabelField("Assemblies included in build", EditorStyles.boldLabel);
        EditorGUI.indentLevel = 1;
        for (int i = 0; i < m_BuildLog.IncludedDependencies.Count; i++)
        {
            EditorGUILayout.LabelField(m_BuildLog.IncludedDependencies[i]);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }

    private void drawAssetFolderInfoRecursively(ProjectFolderInfo assetFolder, int indentLevel)
    {
        EditorGUI.indentLevel = indentLevel;

        if (!assetFolder.ShouldBeListed(m_unusedTypeDict))
            return;
        else
        {
            int assetCount = assetFolder.GetAssetCountInChildren();
            EditorGUILayout.BeginHorizontal();

            Color initialColor = GUI.color;
            GUI.color = Color.yellow;
            float buttonSizeSelect = 60;
            float buttonSizeDelete = 100;

            if (GUILayout.Button("Delete all " + assetCount, GUILayout.Width(buttonSizeDelete)))
            {
                m_folderMarkedForDeletion = assetFolder;
            }

            //Add space to align UI elements
            GUILayout.Space(buttonSizeSelect);

            //Create new style to have a bold foldout
            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;

            //Show foldout
            assetFolder.FoldOut = EditorGUILayout.Foldout(assetFolder.FoldOut, assetFolder.DirectoryName + " (" + assetCount + ")", style);

            //Reset style
            style.fontStyle = previousStyle;

            //Reset color
            GUI.color = initialColor;

            EditorGUILayout.EndHorizontal();
            if (assetFolder.FoldOut)
            {
                foreach (AssetObjectInfo aInfo in assetFolder.AssetList)
                {
                    if ((m_unusedTypeDict.ContainsKey(aInfo.m_Type) && m_unusedTypeDict[aInfo.m_Type] == false))
                        continue;

                    EditorGUI.indentLevel = (indentLevel + 1);
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = Color.grey;
                    if (GUILayout.Button("Delete", GUILayout.Width(buttonSizeDelete)))
                    {
                        m_assetMarkedForDeletion = aInfo;
                    }
                    GUI.color = initialColor;
                    if (GUILayout.Button("Select", GUILayout.Width(buttonSizeSelect)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath(aInfo.m_Path, aInfo.m_Type.SystemType);
                    }

                    EditorGUILayout.LabelField(aInfo.m_Name, GUILayout.MaxWidth(600));
                    EditorGUILayout.EndHorizontal();
                }

                foreach (int childFolder in assetFolder.ChildFolderIndexers)
                {
                    drawAssetFolderInfoRecursively(m_ProjectFolderList[childFolder], (indentLevel + 1));
                }
            }
        }
    }

    private void loadEditorLog()
    {
        m_newBuildReady = false;

        m_ProjectFolderList.Clear();
        m_BuildLog = AssetHunterHelper.AnalyzeBuildLog();

        if (m_BuildLog.IsEmpty())
        {
            m_BuildLogLoaded = false;
            return;
        }
        else
        {
            m_BuildLogLoaded = true;
        }

        List<string> usedPrefabsInScenes = AssetReader.GetPrefabsFromSceneFiles(AssetReader.GetEnabledScenesInBuild(), out m_assetSceneDependencies);

        m_BuildLog.AddPrefabs(usedPrefabsInScenes);
        m_BuildLog.AddPlatformSpecificAssets();

        AssetHunterHelper.PopulateUnusedList(m_BuildLog, m_unusedTypeDict);

        refreshUnusedAssets();
    }

    private void refreshUnusedAssets()
    {
        updateUnusedTypeList();
        updateLogTypeList();

        m_ProjectFolderList[0].RecalcChildAssets(m_unusedTypeDict);
    }

    private void updateLogTypeList()
    {
        m_usedTypeDict.Clear();
        foreach (BuildReportAsset asset in m_BuildLog.m_BuildSizeList)
        {
            if (!m_usedTypeDict.ContainsKey(asset.Type))
            {
                bool shouldBeListed = (asset.Type.SystemType == typeof(UnityEditor.MonoScript) ||
                    asset.Type.SystemType == typeof(UnityEngine.GameObject) ||
                    asset.Type.SystemType == typeof(UnityEngine.Object))
                    ? false : true;
                m_usedTypeDict.Add(asset.Type, shouldBeListed);
            }
        }
        m_usedTypeDict.OrderBy(val => val.Key);
    }

    private void updateUnusedTypeList()
    {
        m_unusedTypeDict.Clear();

        getTypesRecursively(m_ProjectFolderList[0]);

        m_unusedTypeDict.OrderBy(val => val.Key);
    }

    private void getTypesRecursively(ProjectFolderInfo afi)
    {
        foreach (AssetObjectInfo ai in afi.AssetList)
        {
            if (!m_unusedTypeDict.ContainsKey(ai.m_Type))
            {
                m_unusedTypeDict.Add(ai.m_Type, true);
            }
        }
        foreach (int indexer in afi.ChildFolderIndexers)
        {
            getTypesRecursively(m_ProjectFolderList[indexer]);
        }
    }

    public static AssetHunterMainWindow Instance
    {
        get
        {
            if (m_window != null)
                return m_window;
            else
            {
                return doInit();
            }
            ;
        }
    }

    internal void ReCalcUnusedAssetsFromIndex(int m_ParentListIndex)
    {
        m_ProjectFolderList[m_ParentListIndex].RecalcChildAssets(m_unusedTypeDict);
    }

    internal List<ProjectFolderInfo> GetFolderList()
    {
        return m_ProjectFolderList;
    }

    internal void AddProjectFolderInfo(ProjectFolderInfo afInfo)
    {
        m_ProjectFolderList.Add(afInfo);
    }

    internal int GetIndexOf(ProjectFolderInfo projectFolderInfo)
    {
        return m_ProjectFolderList.IndexOf(projectFolderInfo);
    }

    internal ProjectFolderInfo GetFolderInfo(string path)
    {
        return m_ProjectFolderList.Find(val => val.DirectoryName == path);
    }
}

[System.Serializable]
public class StringListWrapper
{
    public List<string> list;

    public StringListWrapper(List<string> list)
    {
        this.list = list;
    }
}
