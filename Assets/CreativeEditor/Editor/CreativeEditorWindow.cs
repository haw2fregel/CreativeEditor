using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor.Callbacks;

namespace CreativeEditor
{
    /// <summary>
    /// CreativeEditor内のSoファイルを扱うためのウィンドウ
    /// 全てのSoはこのウィンドウを通して実行する。
    /// </summary>
    internal class CreativeEditorWindow : EditorWindow
    {
        static List<CreativeEditorSo> soList = null;
        [SerializeField] static List<string> idList = null;
        readonly static string idListKey = "CreativeEditorWindowSoListIds";
        bool showSoList = false;

        string filter;

        [MenuItem("Tools/CreativeEditorWindow")]
        static void Window()
        {
            var window = CreateInstance<CreativeEditorWindow>();

            window.SoListInit(); 
            window.Show();
        }

        /// <summary>
        /// UseSettingから前回のSo設定を取得する
        /// </summary>
        void SoListInit()
        {
            var loadIdList = EditorUserSettings.GetConfigValue(idListKey);

            if (loadIdList != null)
            {
                idList = new List<string>(loadIdList.Split(','));
                soList = new List<CreativeEditorSo>();
                foreach (var id in idList)
                {
                    var path = AssetDatabase.GUIDToAssetPath(id);
                    soList.Add(AssetDatabase.LoadAssetAtPath<CreativeEditorSo>(path));
                }
            }else
            {
                idList = new List<string>();
                soList = new List<CreativeEditorSo>();
            }
        }

        Vector2 _currentScrollPosition;

        void OnGUI()
        {
            
            if(soList == null)
            {
                SoListInit();
            }
            var defaultColor = GUI.color;
            GUI.color = new Color(1,1, 1, 0.6f);
            showSoList = CreativeEditorCustomGUI.Foldout(showSoList, new GUIContent("Soファイル"));
            if (showSoList)
            {
                
                for (int i = 0; i < soList.Count; i++)
                {
                    var so = soList[i];
                    soList[i] = EditorGUILayout.ObjectField(soList[i], typeof(CreativeEditorSo), false) as CreativeEditorSo;
                    if(so != soList[i])
                    { 
                        var path = AssetDatabase.GetAssetPath(soList[i]);
                        idList[i] = AssetDatabase.AssetPathToGUID(path);
                    }
                }
                using (new EditorGUILayout.HorizontalScope()) 
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        soList.Add(null);
                        idList.Add("");
                    }

                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        soList.RemoveAt(soList.Count - 1);
                        idList.RemoveAt(idList.Count - 1);
                    }
                    if (GUILayout.Button("クリア", GUILayout.Width(45)))
                    {
                        soList.Clear(); 
                        idList.Clear();
                    }
                        
                }
                if (GUI.changed)
                {
                    var saveIdList = "";
                    foreach (var id in idList)
                    {
                        if(saveIdList != "") saveIdList += ",";
                        saveIdList += id;
                    }
                    EditorUserSettings.SetConfigValue(idListKey, saveIdList);
                    
                }

            }
            GUI.color = defaultColor;

            if (soList.Count == 0)
            {
                EditorGUILayout.HelpBox("Soファイルをセットしてください。", MessageType.Info);
                return;
            }

            CreativeEditorCustomGUI.PartitionLine();


            using (var scrollView = new EditorGUILayout.ScrollViewScope(_currentScrollPosition))
            {
                _currentScrollPosition = scrollView.scrollPosition;
                foreach (var so in soList)
                {
                    if (so == null) continue;
                    
                    var defalutColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUI.backgroundColor = defalutColor;
                        so.ShowWindow();
                    }
                    var rect = GUILayoutUtility.GetLastRect();
                    RightClickMenu(rect, so);
                    
                }
            }
            
        }

        /// <summary>
        /// 右クリックで表示中のSoファイルをInspectorに表示
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="so"></param>
        static void RightClickMenu (Rect rect, CreativeEditorSo so)
        {
            GUI.enabled = true;
            if (rect.Contains (Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent ("Soファイルを編集"), false, () => { Selection.activeObject = so; });
                menu.ShowAsContext();
            }
        }

    }

}