using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace CreativeEditor
{
    /// <summary>
    /// Animationウィンドウのキーフレーム間のイージングをプリセットとしてScriptableObjectに保存し、
    /// 別のキーフレームに適用するための拡張Editor
    /// CreativeEditorWindowに生成したSoファイルを読み込ませて利用する
    /// </summary>
    [CreateAssetMenu(menuName = "CreativeEditorSo/AnimationValueOffsetSo" )]
    internal class CreativeEditorAnimationValueOffsetSo : CreativeEditorSo
    {

        Vector2 _currentScrollPosition;
        float offsetValue;

        internal override void ShowWindow()
        {
            base.ShowWindow();
            if (!base.showWindow) return;

            using (new EditorGUILayout.VerticalScope("box")) 
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(_currentScrollPosition, GUILayout.Height(70)))
                {
                    _currentScrollPosition = scrollView.scrollPosition;
                    
                    offsetValue = EditorGUILayout.FloatField("補正値", offsetValue);
                    if (CreativeEditorCustomGUI.Button(new GUIContent("値補正", ""), true))
                    {
                        CreativeEditorAnimationValueOffsetHelper.CurveValueOffset(offsetValue);
                    }
                }
            }
            CreativeEditorCustomGUI.PartitionLine();
        }

    }


    /// <summary>
    /// SoファイルのInspector表示を拡張する
    /// </summary>
    [CustomEditor(typeof(CreativeEditorAnimationValueOffsetSo))]
    internal class CreativeEditorAnimationValueOffsetSoEditor : CreativeEditorSoEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }


    /// <summary>
    /// イージングのプリセット化するための補助的クラス
    /// Animationウィンドウへのアクセスを提供する。
    /// </summary>
    internal class CreativeEditorAnimationValueOffsetHelper
    {

        //System.Refrectionで扱うためAnimationWindow関連のTypeを取得しておく
        private static Type animationWindowType = Type.GetType("UnityEditor.AnimationWindow, UnityEditor");
        private static Type animationWindowStateType = Type.GetType("UnityEditorInternal.AnimationWindowState, UnityEditor");
        private static Type animationWindowKeyframeType = Type.GetType("UnityEditorInternal.AnimationWindowKeyframe, UnityEditor");
        private static Type animationWindowCurveType = Type.GetType("UnityEditorInternal.AnimationWindowCurve, UnityEditor");
        private static Type animEditorType = Type.GetType("UnityEditor.AnimEditor, UnityEditor");


        /// <summary>
        /// AnimationCurveを渡すと、Animationウィンドウで選択中のキーフレームにイージングを反映させる。
        /// </summary>
        /// <param name="offsetValue"></param>
        internal static void CurveValueOffset(float offsetValue)
        {
            //Animationウィンドウを取得する。開いてなけれ開き、そのインスタンスを取得する。
            var animationWindow = EditorWindow.GetWindow(animationWindowType,false,null,true);

            //CurveEditorとDopeSheetの管理用にAnimEditorを取得
            var animEditorProperty = animationWindowType.GetProperty("animEditor", BindingFlags.Instance | BindingFlags.NonPublic);
            var animEditor = animEditorProperty.GetValue(animationWindow);

            //Animationウィンドウ内の情報取得のため、AnimationWindowstateを取得。
            var stateProperty = animEditorType.GetProperty("state", BindingFlags.Instance | BindingFlags.Public);
            var state = stateProperty.GetValue(animEditor);

            //今開いてるのがCurve Windowの場合は一度DopeSheetに戻してSelectionKeyの更新をする
            var showCurveEditor = animationWindowStateType.GetField("showCurveEditor", BindingFlags.Instance | BindingFlags.Public);
            if((bool)showCurveEditor.GetValue(state))
            {
                var onEnableWindow = animEditorType.GetMethod("SwitchBetweenCurvesAndDopesheet", BindingFlags . Instance | BindingFlags . NonPublic | BindingFlags . Public);
                onEnableWindow.Invoke(animEditor,null);
                onEnableWindow.Invoke(animEditor,null);
            }

            //選択中のキーフレーム取得のため、AnimationWindowState.selectedKeysを取得
            //AnimationWindowKeyframe型のListで返ってくるが、キャスト不可のため、dynamic型で受け取る
            var getSelectionProperty = animationWindowStateType.GetProperty("selectedKeys", BindingFlags.Instance | BindingFlags.Public);
            var keys = getSelectionProperty.GetValue(state) as dynamic;

            //キーが選択されているか判定用
            var isKeySelection = false;

            //選択されている全てのキーにイージングを反映する。
            foreach (var key in keys)
            {
                //foreach内に入ったのでキー選択されている
                isKeySelection = true;

                //選択しているキーフレームを含む、AnimationWindowCurveを取得
                var curveProperty = animationWindowKeyframeType.GetProperty("curve", BindingFlags.Instance | BindingFlags.Public);
                var selectWindowCurve = curveProperty.GetValue(key);

                //取得したAnimationWindowCurveをUnityEngene.AnimationCurveに変換
                var toAnimationCurve = animationWindowCurveType.GetMethod("ToAnimationCurve" ,  BindingFlags.Instance | BindingFlags.Public);
                var animationCurve = toAnimationCurve.Invoke(obj:selectWindowCurve , parameters: null) as AnimationCurve;

                var editKeys = animationCurve.keys;
                for (int i = 0; i < editKeys.Length; i++)
                {
                    editKeys[i].value += offsetValue;
                    animationCurve.MoveKey(i,editKeys[i]);
                }

                //保存するAnimationClipを取得
                var clipProperty = animationWindowCurveType.GetProperty("clip", BindingFlags.Instance | BindingFlags.Public);
                var clip = clipProperty.GetValue(selectWindowCurve);
                //AnimationCurve判別のためにCurveBindingを取得
                var bindingProperty = animationWindowCurveType.GetProperty("binding", BindingFlags.Instance | BindingFlags.Public);
                var binding = bindingProperty.GetValue(selectWindowCurve);

                //AnimationClipにAnimationCurveの編集を反映
                Undo.RecordObject(clip, "CreativeEditorSo AnimationEdit");
                AnimationUtility.SetEditorCurve(clip, binding, animationCurve);


            }

            //キーフレームを選択してない場合foreachの中に入らず警告表示
            if(!isKeySelection)
            {
                EditorUtility.DisplayDialog("キーフレームが選択されていません。", "Animationウィンドウから値調整したいカーブ内の\nキーフレームを1つ選択して実行してください。", "閉じる");
            }

            return;

        }

    }
}





