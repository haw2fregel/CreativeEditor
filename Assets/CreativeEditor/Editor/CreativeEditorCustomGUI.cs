using UnityEditor;
using UnityEngine;
using System;

namespace CreativeEditor
{
    /// <summary>
    /// CreativeEditorWindowの見た目をカスタムする
    /// EditorGUILayoutの代わりにこっちを使う
    /// </summary>
    internal static class CreativeEditorCustomGUI
    {

        /// <summary>
        /// ParticleSystemで使われてるFoldoutっぽいやつ
        /// </summary>
        /// <param name="display"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        internal static bool Foldout(bool display, GUIContent title)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.border = new RectOffset(15, 2, 4, 2);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);
            style.fontSize = 13;

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y +3f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }

        /// <summary>
        /// 文字の長さで幅を自動調整するボタン
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        internal static bool Button(GUIContent content)
        {
            var style = new GUIStyle("button");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.fixedHeight = 18;
            style.fontSize = 11;
            var width = (content.text.Length + 2) * (style.fontSize + 1);
            return GUILayout.Button(content, style, GUILayout.Width(width));
        }

        /// <summary>
        /// 文字の長さで幅を自動調整するボタン
        /// expand true でWindow幅に合わせて引き伸ばしあり
        /// </summary>
        /// <param name="content"></param>
        /// <param name="expand"></param>
        /// <returns></returns>
        internal static bool Button(GUIContent content, bool expand)
        {
            var style = new GUIStyle("button");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.fixedHeight = 18;
            style.fontSize = 11;
            var width = (content.text.Length + 2) * style.fontSize;
            return GUILayout.Button(content, style, GUILayout.Width(width), GUILayout.ExpandWidth(expand));
        }


        /// <summary>
        /// アセット保存場所を決める時に出てくるWindow
        /// Pathを返すだけなのでアセット保存はその後やる
        /// </summary>
        /// <param name="defaultPath"></param>
        /// <param name="defaultType"></param>
        /// <returns></returns>
        internal static string SelectionAssetPath(string defaultPath, string defaultType)
        {
            var path = "";
            if (string.IsNullOrEmpty(defaultPath) || System.IO.Path.GetExtension(defaultPath) != "." + defaultType)
            {
                if (string.IsNullOrEmpty(defaultPath)) {
                    path = "Assets";
                }
            }// ディレクトリがなければ作る
            else if (System.IO.Directory.Exists(path) == false) {
                System.IO.Directory.CreateDirectory(path);
            }

            // ファイル保存パネルを表示
            var fileName = "name." + defaultType;
            fileName = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(path, fileName)));
            path = EditorUtility.SaveFilePanelInProject("Save Some Asset", fileName, defaultType, "", path);

            return path;
        }

        /// <summary>
        /// 仕切り線表示する
        /// </summary>
        internal static void PartitionLine()
        {
            EditorGUILayout.Space();
            GUI.color = new Color(4f,4f,4f,1f);
            GUILayout.Box("", GUILayout.Height(2), GUILayout.ExpandWidth(true));
            GUI.color = new Color(1f,1f,1f,1f);
            EditorGUILayout.Space();
        }
    }

}