using UnityEngine;
using UnityEditor;
using System.Collections;
using HeurekaGames;

public class AssetHunterSettingsWindow : EditorWindow
{
    private static AssetHunterSettingsWindow m_window;
    private static Color m_IntialGUIColor;
    private Vector2 scrollPos;

    [SerializeField]
    public AssetHunterSettings settings;
    private float btnMinWidthLarge = 200;
    private float btnMinWidthSmall = 80;

    public static AssetHunterSettingsWindow Instance
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

    void OnInspectorUpdate()
    {
        if (!m_window)
            doInit();
    }

    private static AssetHunterSettingsWindow doInit()
    {
        m_IntialGUIColor = GUI.color;

        m_window = EditorWindow.GetWindow<AssetHunterSettingsWindow>();
        m_window.Show();

        return m_window;
    }

    private void OnGUI()
    {

        if (settings == null)
        {
            string path = AssetHunterSettingsCreator.GetAssetPath();
            settings = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject)) as AssetHunterSettings;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        //Show all used types
        EditorGUILayout.BeginVertical();
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        //Make sure this window has focus to update contents
        AssetHunterSettingsWindow.Instance.Repaint();

        EditorGUILayout.Separator();
        GUILayout.Label("This is the settingswindow for Asset Hunter! " + System.Environment.NewLine + "-Choose folders and types to exclude when scanning the project");
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        //Do we have a folder selected
        bool bFolderSelected = System.IO.Directory.Exists(selectedPath);
        //Is it valid
        bool validSelection = (bFolderSelected && settings.ValidateDirectory(Selection.activeObject));

        //Select folder to exclude
        EditorGUILayout.BeginHorizontal();

        GUI.color = (validSelection ? Color.green : Color.grey);

        if (GUILayout.Button(validSelection ? "Exclude selected folder" : "No valid folder selected", GUILayout.Width(btnMinWidthLarge)))
        {
            if (validSelection)
            {
                settings.ExcludeDirectory(Selection.activeObject);
            }
        }

        GUI.color = m_IntialGUIColor;

        if (validSelection)
            GUILayout.Label(selectedPath, EditorStyles.miniBoldLabel);

        GUI.color = m_IntialGUIColor;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        //Select type to exclude
        EditorGUILayout.BeginHorizontal();
        SerializableSystemType selectedType = null;

        if (Selection.activeObject)
            selectedType = new SerializableSystemType(Selection.activeObject.GetType());

        //Do we have a valid asset selected
        validSelection = (selectedType != null && !bFolderSelected && settings.ValidateType(selectedType));

        GUI.color = (validSelection ? Color.green : Color.grey);

        if (GUILayout.Button(validSelection ? "Exclude selected type" : "No valid type selected", GUILayout.Width(btnMinWidthLarge)))
        {
            if (validSelection)
            {
                settings.ExcludeType(selectedType);
            }
        }

        if (validSelection)
            GUILayout.Label(selectedType.SystemType.ToString(), EditorStyles.miniBoldLabel);

        GUI.color = m_IntialGUIColor;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        GUILayout.Label("---------------------------Excluded Folders------------------------------", EditorStyles.boldLabel);

        if (settings.m_DirectoryExcludes.Count >= 1)
            for (int i = settings.m_DirectoryExcludes.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = Color.red;

                if (GUILayout.Button("Delete", GUILayout.Width(btnMinWidthSmall)))
                {
                    settings.RemoveDirectoryAtIndex(i);
                    continue;
                }
                GUI.color = m_IntialGUIColor;
                EditorGUILayout.ObjectField(settings.m_DirectoryExcludes[i], typeof(UnityEngine.Object), false);
                EditorGUILayout.EndHorizontal();
            }
        else
        {
            EditorGUILayout.LabelField("No folders are currently excluded");
        }

        EditorGUILayout.Separator();
        GUILayout.Label("---------------------------Excluded Types--------------------------------", EditorStyles.boldLabel);

        if (settings.m_AssetTypeExcludes.Count >= 1)
            for (int i = settings.m_AssetTypeExcludes.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = Color.red;
                if (GUILayout.Button("Delete", GUILayout.Width(btnMinWidthSmall)))
                {
                    settings.RemoveTypeAtIndex(i);
                    continue;
                }
                GUI.color = m_IntialGUIColor;
                GUILayout.Label(settings.m_AssetTypeExcludes[i].Name);
                EditorGUILayout.EndHorizontal();
            }
        else
        {
            EditorGUILayout.LabelField("No types are currently excluded");
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
}
