using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace CreativeEditor
{
    /// <summary>
    /// Hierarchyの検索条件を保存して
    /// EditorWindowから再現する
    /// </summary> 
    [CreateAssetMenu(menuName = "CreativeEditorSo/HierarchySerchSo" )]
    internal class CreativeEditorHierarchySerchSo : CreativeEditorSo
    {

        [SerializeField]
        List<CreativeEditorHierarchySerchSetting> settings;
        internal List<CreativeEditorHierarchySerchSetting> Settings {get{return settings;}}
        internal static readonly string p_settings = "settings";

        Vector2 _currentScrollPosition;

        internal override void ShowWindow()
        {
            base.ShowWindow();
            if (Settings.Count == 0) return;
            if (!base.showWindow) return;

            using (new EditorGUILayout.VerticalScope("box")) 
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(_currentScrollPosition, GUILayout.Height(40)))
                {
                    _currentScrollPosition = scrollView.scrollPosition;
                    using (new EditorGUILayout.HorizontalScope()) 
                    {
                        foreach (var setting in Settings )
                        {
                            var normalColor = GUI.color;
                            GUI.color = setting.LabelColor ;
                            EditorGUILayout.LabelField(" ", GUILayout.Width(5));
                            if (CreativeEditorCustomGUI.Button(new GUIContent(setting.LabelName, setting.Tips), true))
                            {
                                CreativeEditorHierarchySerchHelper.SerchHierarchy(setting.SerchText);
                            }
                            GUI.color = normalColor;
                        }
                    }
                }
            }

            CreativeEditorCustomGUI.PartitionLine();


        }
    }
    

    /// <summary>
    /// 検索条件を保存するクラス
    /// </summary>
    [Serializable]
    internal class CreativeEditorHierarchySerchSetting : CreativeEditorSoSetting
    {
        [SerializeField]
        string[] serchKeywords;
        internal string[] SerchKeywords { get { return serchKeywords; }}
        internal static readonly string p_serchKeywords = "serchKeywords";

        [SerializeField]
        string[] serchType;
        internal string[] SerchType { get { return serchType; }}
        internal static readonly string p_serchType = "serchType";

        internal string SerchText
        {
            get
            {
                var text = "";
                foreach (var typeName in serchType)
                {
                    text +=  "t:" + typeName + " ";
                }

                foreach (var keyword in serchKeywords)
                {
                    text += keyword + " ";
                }

                return text;
            }
        }

        internal CreativeEditorHierarchySerchSetting()
        {
            base.labelName = "ヒエラルキー検索";
            base.labelColor = new Color(1,1,1,1);
            base.tips = "";
            serchKeywords = new string[]{};
            serchType = new string[]{};
        }
        

    }

    /// <summary>
    /// SoファイルのInspector表示を拡張する
    /// </summary>
    [CustomEditor(typeof(CreativeEditorHierarchySerchSo))]
    internal class CreativeEditorHierarchySerchSoEditor : CreativeEditorSoEditor
    {
        Vector2 _currentScrollPosition;
        List<bool> showElements = new List<bool>();
        List<bool> showComps = new List<bool>();

        CreativeEditorHierarchySerchSo so = null;
        
        void OnEnable() 
        {
            so = target as CreativeEditorHierarchySerchSo;

            base.showListAll.Clear();
            base.showListAll.Add(showElements);
            base.showListAll.Add(showComps);
            
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using( var verticalAll = new EditorGUILayout.VerticalScope())
            {
                serializedObject.Update();

                var soProp = serializedObject.FindProperty(CreativeEditorHierarchySerchSo.p_settings);
                
                base.PropertyElementArraySizeVaridate(soProp);

                GUI.color = new Color(2.7f,2.7f,2.7f,1f);

                using(new EditorGUILayout.VerticalScope("Box"))
                {
                    EditorGUI.indentLevel++;
                    GUI.color = new Color(1f,1f,1f,1f);
                    EditorGUILayout.LabelField("ヒエラルキー検索");
                    GUI.color = new Color(0.3f,0.3f,0.3f,1f);
                    using(new EditorGUILayout.VerticalScope("Box"))
                    {
                        GUI.color = new Color(2f,2f,2f,1f);

                        using (var scrollView = new EditorGUILayout.ScrollViewScope(_currentScrollPosition))
                        {

                            _currentScrollPosition = scrollView.scrollPosition;

                            EditorGUI.indentLevel++;
                            for(int i = 0 ; i < soProp.arraySize ; i++)
                            {
                                var element = soProp.GetArrayElementAtIndex(i);
                                using(new EditorGUILayout.HorizontalScope()) 
                                {
                                    showElements[i] = base.showElementFoldout(element, showElements[i]);
                                    GUILayout.FlexibleSpace();

                                    if(base.PropertyElementMoveUpButton(serializedObject, soProp, i)) return;
                                    if(base.PropertyElementMoveDownButton(serializedObject, soProp, i)) return;
                                    if(base.PropertyElementDeleteButton(serializedObject, soProp, i)) return;
                                }
                                    
                                if(!showElements[i]) continue;

                                        
                                EditorGUI.indentLevel++;
                                    base.ButtomLooksSettingFields<CreativeEditorHierarchySerchSetting>(element);
                                    EditorGUILayout.Space();

                                    EditorGUI.indentLevel++;

                                        GUI.color = new Color(1f,1f,1f,1f);
                                        EditorGUILayout.PropertyField(element.FindPropertyRelative(CreativeEditorHierarchySerchSetting.p_serchKeywords), new GUIContent("キーワード"));
                                        EditorGUILayout.PropertyField(element.FindPropertyRelative(CreativeEditorHierarchySerchSetting.p_serchType), new GUIContent("タイプ"));

                                    EditorGUI.indentLevel--;
                                    
                                    GUI.color = new Color(4f,4f,4f,1f);
                                    GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
                                    GUI.color = new Color(2f,2f,2f,1f);

                                EditorGUI.indentLevel--;
                                
                            }
                        }
                        EditorGUI.indentLevel--;
                
                        GUI.color = new Color(1f,1f,1f,1f);

                        using (new EditorGUILayout.HorizontalScope())
                        {

                            GUILayout.FlexibleSpace();
                            GUILayout.Label("" + soProp.arraySize,EditorStyles.textField,GUILayout.Width(40));
                        }
                        using (new EditorGUILayout.HorizontalScope()) 
                        {

                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("+", GUILayout.Width(25)))
                            {
                                Undo.RecordObject(so, "CreativeEditorSo AddSetting");
                                var setting = new CreativeEditorHierarchySerchSetting();
                                so.Settings.Add(setting);
                                EditorUtility.SetDirty(so);
                                AssetDatabase.SaveAssets();
                                return;
                            }
                            
                            if(base.PropertyElementDeleteButton(serializedObject, soProp, soProp.arraySize - 1)) return;
                            if(base.PropertyElementClearButton(soProp)) return;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>
    /// Hierarchyウィンドウに検索条件を投げ込むための補助クラス
    /// </summary>
    internal class CreativeEditorHierarchySerchHelper
    {

        /// <summary>
        /// ヒエラルキーウィンドウの検索欄に指定文字列を投げ込みます。
        /// </summary>
        /// <param name="filter"></param>
        internal static void SerchHierarchy(string filter)
        {
            var hierarchyWindowType = Type.GetType("UnityEditor.SceneHierarchyWindow, UnityEditor");
            var hierarchyWindow = EditorWindow.GetWindow( hierarchyWindowType, false, null, true);
            var setSearchFilterMethod = hierarchyWindowType.GetMethod( "SetSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
            setSearchFilterMethod.Invoke( hierarchyWindow, new object[] { filter, 0, false, false} );
        }

    }

}





