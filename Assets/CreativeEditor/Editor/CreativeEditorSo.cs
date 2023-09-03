using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CreativeEditor
{
    /// <summary>
    /// CreativeEditorで扱うSoの基底クラス
    /// 継承したクラスに機能を持たせてCreativeEditorWindowで扱う
    /// </summary>
    internal class CreativeEditorSo : ScriptableObject
    {
        [SerializeField]
        string title;
        internal string Title{get{return title;}}
        internal static readonly string p_title = "title";

        [SerializeField]
        string tips;
        internal string Tips { get{return tips; }}
        internal static readonly string p_tips = "tips";

        [SerializeField]
        Color titleColor = new Color(1,1,1,1);
        internal Color TitleColor { get { return titleColor; }}
        internal static readonly string p_titleColor = "titleColor";

        internal bool showWindow = false;


        /// <summary>
        /// CreativeEditor Windowにセットした際に呼び出されるメソッド
        /// 継承クラスでこの中にボタン表示など機能を持たせる
        /// </summary>
        internal virtual void ShowWindow()
        {
            var defalutColor = GUI.color;
            GUI.color = TitleColor;
            showWindow = CreativeEditorCustomGUI.Foldout(showWindow, new GUIContent(Title, Tips));
            GUI.color = defalutColor;
            
        }
    }

    /// <summary>
    /// 設定保存用クラスのベース
    /// </summary>
    [Serializable]
    internal class CreativeEditorSoSetting
    {
        [SerializeField]
        protected string labelName;
        internal string LabelName{get{return labelName;}}

        [SerializeField]
        protected Color labelColor;
        internal Color LabelColor { get { return labelColor; }}

        [SerializeField]
        protected string tips;
        internal string Tips { get { return tips; }}
    }

    /// <summary>
    /// Soファイルのインスペクター表示を拡張
    /// </summary>
    [CustomEditor(typeof(CreativeEditorSo))]
    internal class CreativeEditorSoEditor : Editor
    {

        protected List<List<bool>> showListAll = new List<List<bool>>();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var title_prop = serializedObject.FindProperty(CreativeEditorSo.p_title);
            var message_prop = serializedObject.FindProperty(CreativeEditorSo.p_tips);
            var color_prpo = serializedObject.FindProperty(CreativeEditorSo.p_titleColor);

            using ( var verticalAll = new EditorGUILayout.VerticalScope())
            {
                title_prop.stringValue = EditorGUILayout.TextField("ラベル表示名", title_prop.stringValue);
                color_prpo.colorValue = EditorGUILayout.ColorField("ラベルカラー",color_prpo.colorValue);
                message_prop.stringValue = EditorGUILayout.TextArea(message_prop.stringValue);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// ArrayPropertyの順番を入れ替えるボタンを表示する
        /// </summary>
        /// <param name="so"></param>
        /// <param name="soProp"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected bool PropertyElementMoveUpButton(SerializedObject so, SerializedProperty soProp, int index)
        {
            var baseColor = GUI.color;
            GUI.color = new Color(1f,1f,1f,0.75f);
            using (new EditorGUI.DisabledScope(index == 0))
            {
                if (GUILayout.Button("↑",GUILayout.Width(20))) 
                {
                    soProp.MoveArrayElement(index,index-1);
                    so.ApplyModifiedProperties();
                    GUI.color = baseColor;
                    return true;
                }
            }
            GUI.color = baseColor;
            return false;
        }
        
        /// <summary>
        /// ArrayPropertyの順番を入れ替えるボタンを表示する
        /// </summary>
        /// <param name="so"></param>
        /// <param name="soProp"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected bool PropertyElementMoveDownButton(SerializedObject so, SerializedProperty soProp, int index)
        {
            var baseColor = GUI.color;
            GUI.color = new Color(1f,1f,1f,0.75f);
            using (new EditorGUI.DisabledScope(index == soProp.arraySize-1))
            {
                if (GUILayout.Button("↓",GUILayout.Width(20))) 
                {
                    soProp.MoveArrayElement(index,index+1);
                    so.ApplyModifiedProperties();
                    GUI.color = baseColor;
                    return true;
                }
            }
            GUI.color = baseColor;
            return false;
        }

        /// <summary>
        /// ArrayPropertyの要素を削除するボタンを表示する
        /// </summary>
        /// <param name="so"></param>
        /// <param name="soProp"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected bool PropertyElementDeleteButton(SerializedObject so, SerializedProperty soProp, int index)
        {
            var baseColor = GUI.color;
            GUI.color = new Color(1f,1f,1f,0.75f);
            if (GUILayout.Button("削除",GUILayout.Width(40))) 
            {
                if (soProp.arraySize == 0) return false; 

                soProp.DeleteArrayElementAtIndex(index);
                foreach(var showList in showListAll)
                {
                    showList.RemoveAt(index);
                }
                so.ApplyModifiedProperties();
                GUI.color = baseColor;
                return true;
                
            }
            GUI.color = baseColor;
            return false;
        }
        
        /// <summary>
        /// ArrayPropertyの要素を空にするボタンを表示する
        /// </summary>
        /// <param name="soProp"></param>
        /// <returns></returns>
        protected bool PropertyElementClearButton(SerializedProperty soProp)
        {
            if (GUILayout.Button("クリア",GUILayout.Width(45))) 
            {
                soProp.ClearArray();
                foreach(var showList in showListAll)
                {
                    showList.Clear();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// リストの長さに差異がないかチェック
        /// 違う場合はSoファイルの状況に合わせる
        /// </summary>
        /// <param name="soProp"></param>
        protected void PropertyElementArraySizeVaridate(SerializedProperty soProp)
        {
            foreach (var showList in showListAll)
            {
                if(showList.Count >= soProp.arraySize) continue;

                for(int i = showList.Count - 1 ; i <= soProp.arraySize ; i++)
                {
                    showList.Add(false);
                }
            }
        }

        protected void ButtomLooksSettingFields<T>(SerializedProperty element)
        {
            EditorGUILayout.PropertyField(element.FindPropertyRelative("labelName"), new GUIContent("ボタン表示名"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("labelColor"), new GUIContent("ボタンカラー"));
            element.FindPropertyRelative("tips").stringValue = EditorGUILayout.TextArea(element.FindPropertyRelative("tips").stringValue);
        }

        protected bool showElementFoldout(SerializedProperty element, bool beforeValue)
        {
            return EditorGUILayout.Foldout(beforeValue, element.FindPropertyRelative("labelName").stringValue);
        }
    }
}
