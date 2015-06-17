using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace HeurekaGames
{
    public class EditorUtils
    {
        static Dictionary<EditorWindow, GUIContent> m_windowContentDict;

        public static void SetWindowValues(EditorWindow editor, Texture icon, string title)
        {

            GUIContent guiContent;
            if (m_windowContentDict == null) 
                m_windowContentDict = new Dictionary<EditorWindow, GUIContent>();
            
            if (m_windowContentDict.ContainsKey(editor))
            {
                guiContent = m_windowContentDict[editor];
                if (guiContent != null)
                {
                    if (guiContent.image != icon) guiContent.image = icon;
                    if (title != null && guiContent.text != title) guiContent.text = title;
                    return;
                }
                m_windowContentDict.Remove(editor);
            }

            guiContent = getContent(editor);
            if (guiContent != null)
            {
                if (guiContent.image != icon) guiContent.image = icon;
                if (title != null && guiContent.text != title) guiContent.text = title;
                m_windowContentDict.Add(editor, guiContent);
            }
        }

        static GUIContent getContent(EditorWindow editor)
        {
            const BindingFlags bFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            PropertyInfo p = typeof(EditorWindow).GetProperty("cachedTitleContent", bFlags);
            if (p == null) return null;
            return p.GetValue(editor, null) as GUIContent;
        }
    }
}
