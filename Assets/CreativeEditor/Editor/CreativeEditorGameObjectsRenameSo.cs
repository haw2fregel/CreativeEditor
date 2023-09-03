using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CreativeEditor
{
    /// <summary>
    /// Animatorのプリセットを生成して、
    /// Controllerアセットの生成と選択オブジェクトにコンポーネント付与をする
    /// </summary>
    [CreateAssetMenu(menuName = "CreativeEditorSo/GameObjectRenameSo" )]
    internal class CreativeEditorGameObjectsRenameSo : CreativeEditorSo
    {

        Vector2 _currentScrollPosition;
        string beforeKeyword;
        string afterKeyword;
        string addKeyword;
        string numBaseKeyword;
        int renameType;

        internal override void ShowWindow()
        {
            base.ShowWindow();
            if (!base.showWindow) return;
            
            using (new EditorGUILayout.VerticalScope("box")) 
            {
                var normalColor = GUI.color;
                using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
                {

                    using (new EditorGUI.DisabledScope(renameType == 0))
                    {
                        if (renameType == 0)
                        {
                            GUI.color = new Color(1.5f, 1.5f, 1.5f, 1);
                        }
                        else
                        {
                            GUI.color = new Color(0.75f, 0.75f, 0.75f, 1);
                        }
                        if (GUILayout.Button("入れ替え", EditorStyles.toolbarButton))
                        {
                            renameType = 0;
                        }
                    }
                    using (new EditorGUI.DisabledScope(renameType == 1))
                    {
                        if (renameType == 1)
                        {
                            GUI.color = new Color(1.5f, 1.5f, 1.5f, 1);
                        }
                        else
                        {
                            GUI.color = new Color(0.75f, 0.75f, 0.75f, 1);
                        }
                        if (GUILayout.Button("末尾に追加", EditorStyles.toolbarButton))
                        {
                            renameType = 1;
                        }
                    }
                    using (new EditorGUI.DisabledScope(renameType == 2))
                    {
                        if (renameType == 2)
                        {
                            GUI.color = new Color(1.5f, 1.5f, 1.5f, 1);
                        }
                        else
                        {
                            GUI.color = new Color(0.75f, 0.75f, 0.75f, 1);
                        }
                        if (GUILayout.Button("連番名", EditorStyles.toolbarButton))
                        {
                            renameType = 2;
                        }
                    }
                }
                GUI.color = normalColor;
                EditorGUILayout.Space();

                if(renameType == 0)
                {
                    using (var scrollView = new EditorGUILayout.ScrollViewScope(_currentScrollPosition, GUILayout.Height(40)))
                    {
                        _currentScrollPosition = scrollView.scrollPosition;
                        using (new EditorGUILayout.HorizontalScope()) 
                        {
                            EditorGUILayout.LabelField("変更前", GUILayout.Width(40));
                            beforeKeyword = EditorGUILayout.TextField(beforeKeyword);
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.LabelField("→", GUILayout.Width(20));
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.LabelField("変更後", GUILayout.Width(40));
                            afterKeyword = EditorGUILayout.TextField(afterKeyword);
                        }
                        
                    }

                    if (CreativeEditorCustomGUI.Button(new GUIContent("リネーム", ""), true))
                    {

                        foreach (var obj in Selection.gameObjects)
                        {
                            var path = AssetDatabase.GetAssetPath(obj);
                            var newName = obj.name.Replace(beforeKeyword, afterKeyword);
                            obj.gameObject.name = newName;
                        }
            
                    }
                }

                if(renameType == 1)
                {
                    using (var scrollView = new EditorGUILayout.ScrollViewScope(_currentScrollPosition, GUILayout.Height(40)))
                    {
                        _currentScrollPosition = scrollView.scrollPosition;
                        using (new EditorGUILayout.HorizontalScope()) 
                        {
                            addKeyword = EditorGUILayout.TextField("追加キーワード", addKeyword);
                        }
                        
                    }

                    if (CreativeEditorCustomGUI.Button(new GUIContent("リネーム", ""), true))
                    {

                        foreach (var obj in Selection.gameObjects)
                        {
                            var path = AssetDatabase.GetAssetPath(obj);
                            var newName = obj.name + addKeyword;
                            obj.gameObject.name = newName;
                        }
       
                    }
                }

                if(renameType == 2)
                {
                    using (var scrollView = new EditorGUILayout.ScrollViewScope(_currentScrollPosition, GUILayout.Height(40)))
                    {
                        _currentScrollPosition = scrollView.scrollPosition;
                        using (new EditorGUILayout.HorizontalScope()) 
                        {
                            numBaseKeyword = EditorGUILayout.TextField("名称", numBaseKeyword);
                            EditorGUILayout.LabelField("_00", GUILayout.Width(40));
                        }
                        
                    }

                    if (CreativeEditorCustomGUI.Button(new GUIContent("リネーム", ""), true))
                    {
                        int count = 0;
                        foreach (var obj in Selection.gameObjects)
                        {
                            var path = AssetDatabase.GetAssetPath(obj);
                            string number = "" + count;
                            if(number.Length == 1) number = "0" + number;
                            var newName = numBaseKeyword + "_" + number;
                            obj.gameObject.name = newName;
                            count++;
                        }
                    }
                }
                
            }
            CreativeEditorCustomGUI.PartitionLine();
            
        }

    }


    /// <summary>
    /// SoファイルのInspector表示を拡張する
    /// </summary>
    [CustomEditor(typeof(CreativeEditorGameObjectsRenameSo))]
    internal class CreativeEditorGameObjectsRenameSoEditor : CreativeEditorSoEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}





