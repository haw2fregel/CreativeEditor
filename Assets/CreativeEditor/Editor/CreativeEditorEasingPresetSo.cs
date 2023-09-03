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
    [CreateAssetMenu(menuName = "CreativeEditorSo/EasingPresetSo" )]
    internal class CreativeEditorEasingPresetSo : CreativeEditorSo
    {

        [SerializeField]
        List<CreativeEditorEasingPreset> settings;
        internal List<CreativeEditorEasingPreset> Settings {get{return settings;}}
        internal static readonly string p_settings = "settings";
        Vector2 _currentScrollPosition;

        internal override void ShowWindow()
        {
            base.ShowWindow();
            if (Settings.Count == 0) return;
            if (!base.showWindow) return;

            using (new EditorGUILayout.VerticalScope("box")) 
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(_currentScrollPosition, GUILayout.Height(70)))
                {
                    _currentScrollPosition = scrollView.scrollPosition;
                    
                    using (new EditorGUILayout.HorizontalScope()) 
                    {
                        foreach (var setting in Settings )
                        {
                            var normalColor = GUI.color;
                            GUI.color = setting.LabelColor ;
                            EditorGUILayout.LabelField(" ", GUILayout.Width(5));
                            //選択中のキーフレームにイージングを反映させる。
                            if (GUILayout.Button(new GUIContent(setting.LabelName, setting.Tips), GUILayout.Width(150), GUILayout.ExpandWidth(true)))
                            {
                                CreativeEditorEasingPresetHelper.SetEasingPreset(setting.Curve);
                            }
                            GUI.color = normalColor;
                        }
                    }
                    using (new EditorGUILayout.HorizontalScope()) 
                    {
                        foreach (var setting in Settings )
                        {
                            var normalColor = GUI.color;
                            GUI.color = setting.LabelColor ;
                            EditorGUILayout.LabelField(" ", GUILayout.Width(5));
                            //カーブの見た目表示用。触れないように
                            using (new EditorGUI.DisabledScope(true))
                            {
                                EditorGUILayout.CurveField(setting.Curve, GUILayout.Width(150), GUILayout.ExpandWidth(true));
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
    /// イージングのプリセット情報を保存するクラス
    /// ボタンの見た目を変えるためのプロパティと
    /// AnimationCurveを持つ
    /// </summary>
    [Serializable]
    internal class CreativeEditorEasingPreset : CreativeEditorSoSetting
    {
        [SerializeField]
        AnimationCurve curve;
        internal AnimationCurve Curve { get { return curve; }}
        internal static readonly string p_curve = "curve";

        internal CreativeEditorEasingPreset()
        {
            base.labelName = "イージング";
            base.labelColor = new Color(1, 1, 1, 1);
            base.tips = "";
            curve = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0));
        }

        internal CreativeEditorEasingPreset(AnimationCurve newCurve)
        {
            base.labelName = "イージング";
            base.labelColor = new Color(1, 1, 1, 1);
            base.tips = "";
            curve = newCurve;
        }

    }


    /// <summary>
    /// SoファイルのInspector表示を拡張する
    /// </summary>
    [CustomEditor(typeof(CreativeEditorEasingPresetSo))]
    internal class CreativeEditorEasingPresetSoEditor : CreativeEditorSoEditor
    {
        Vector2 _currentScrollPosition;
        List<bool> showElements = new List<bool>();

        CreativeEditorEasingPresetSo so = null;
        
        void OnEnable() 
        {
            so = target as CreativeEditorEasingPresetSo;

            base.showListAll.Clear();
            base.showListAll.Add(showElements);
            
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using( var verticalAll = new EditorGUILayout.VerticalScope())
            {
                serializedObject.Update();

                var soProp = serializedObject.FindProperty(CreativeEditorEasingPresetSo.p_settings);
                
                base.PropertyElementArraySizeVaridate(soProp);

                GUI.color = new Color(2.7f,2.7f,2.7f,1f);

                using( var vertical = new EditorGUILayout.VerticalScope("Box"))
                {
                    EditorGUI.indentLevel++;
                    GUI.color = new Color(1f,1f,1f,1f);
                    EditorGUILayout.LabelField("イージングプリセット");
                    GUI.color = new Color(0.3f,0.3f,0.3f,1f);
                    using( var vertical2 = new EditorGUILayout.VerticalScope("Box"))
                    {
                        GUI.color = new Color(2f,2f,2f,1f);

                        using (var scrollView = new EditorGUILayout.ScrollViewScope(_currentScrollPosition))
                        {

                            _currentScrollPosition = scrollView.scrollPosition;

                            EditorGUI.indentLevel++;
                            for(int i = 0 ; i < soProp.arraySize ; i++)
                            {
                                    
                                var element = soProp.GetArrayElementAtIndex(i);
                                using (new EditorGUILayout.HorizontalScope()) 
                                {
                                    showElements[i] = base.showElementFoldout(element, showElements[i]);
                                    GUILayout.FlexibleSpace();

                                    if(base.PropertyElementMoveUpButton(serializedObject, soProp, i)) return;
                                    if(base.PropertyElementMoveDownButton(serializedObject, soProp, i)) return;
                                    if(base.PropertyElementDeleteButton(serializedObject, soProp, i)) return;
                                }
                                    
                                if(!showElements[i]) continue;
                                        
                                EditorGUI.indentLevel++;
                                    base.ButtomLooksSettingFields<CreativeEditorEasingPreset>(element);
                                    
                                    EditorGUILayout.PropertyField(element.FindPropertyRelative(CreativeEditorEasingPreset.p_curve), new GUIContent("カーブ"));
                                
                                EditorGUI.indentLevel--;
                                    
                                GUI.color = new Color(4f,4f,4f,1f);
                                GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
                                GUI.color = new Color(2f,2f,2f,1f);
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
                            using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
                            {
                                if (GUILayout.Button("選択キーフレームから登録", GUILayout.Width(160)))
                                {
                                    var getCurve = CreativeEditorEasingPresetHelper.GetSelectionAnimationCurve();
                                    if(getCurve == null){return;}
                                    var setting = new CreativeEditorEasingPreset(getCurve);
                                    Undo.RecordObject(so, "CreativeEditorObjectPreset CopyObject");
                                    so.Settings.Add(setting);
                                    EditorUtility.SetDirty(so);
                                    return;
                                }
                            }

                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("+", GUILayout.Width(25)))
                            {
                                Undo.RecordObject(so, "CreativeEditorSo AddSetting");
                                var setting = new CreativeEditorEasingPreset();
                                so.Settings.Add(setting);
                                EditorUtility.SetDirty(so);
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
    /// イージングのプリセット化するための補助的クラス
    /// Animationウィンドウへのアクセスを提供する。
    /// </summary>
    internal class CreativeEditorEasingPresetHelper
    {
        //System.Refrectionで扱うためAnimationWindow関連のTypeを取得しておく
        private static Type animationWindowType = Type.GetType("UnityEditor.AnimationWindow, UnityEditor");
        private static Type animationWindowStateType = Type.GetType("UnityEditorInternal.AnimationWindowState, UnityEditor");
        private static Type animationWindowKeyframeType = Type.GetType("UnityEditorInternal.AnimationWindowKeyframe, UnityEditor");
        private static Type animationWindowCurveType = Type.GetType("UnityEditorInternal.AnimationWindowCurve, UnityEditor");
        private static Type animEditorType = Type.GetType("UnityEditor.AnimEditor, UnityEditor");

        /// <summary>
        /// Animationウィンドウで選択中のキーフレームが持つイージングをAnimationCurveに変換して取得する。
        /// </summary>
        /// <returns></returns>
        internal static AnimationCurve GetSelectionAnimationCurve()
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
            };

            //選択中のキーフレーム取得のため、AnimationWindowState.selectedKeysを取得
            //AnimationWindowKeyframe型のListで返ってくるが、キャスト不可のため、dynamic型で受け取る
            var getSelectionProperty = animationWindowStateType.GetProperty("selectedKeys", BindingFlags.Instance | BindingFlags.Public);
            var keys = getSelectionProperty.GetValue(state) as dynamic;

            //keys[0]だけ扱えれば良いが、dynamic型だとindex参照できないため、foreachで無理やり参照する
            //1週だけでいいので最後にreturn
            foreach (var key in keys)
            {
                //選択しているキーフレームを含む、AnimationWindowCurveを取得
                var curveProperty = animationWindowKeyframeType.GetProperty("curve", BindingFlags.Instance | BindingFlags.Public);
                var selectWindowCurve = curveProperty.GetValue(key);

                //取得したAnimationWindowCurveをUnityEngene.AnimationCurveに変換
                var toAnimationCurve = animationWindowCurveType.GetMethod("ToAnimationCurve" ,  BindingFlags.Instance | BindingFlags.Public);
                var animationCurve = toAnimationCurve.Invoke(obj:selectWindowCurve , parameters: null) as AnimationCurve;
                

                //選択したキーフレームがAnimationCurve内の何番目か取得
                var getIndex = animationWindowKeyframeType.GetMethod("GetIndex" ,  BindingFlags.Instance | BindingFlags.Public);
                var index = (int)getIndex.Invoke(key , null);

                if(animationCurve.length <= index + 1)
                {
                    EditorUtility.DisplayDialog("イージングの取得に失敗しました。", "最終キーフレームからは取得できません。\n別のキーフレームを選択してください。", "閉じる");
                    return null;
                }

                //扱いやすいようにtime,valueの幅を0~1に正規化したカーブに変換してから保存する。
                var fromKey = animationCurve.keys[index];
                var toKey = animationCurve.keys[index+1];

                fromKey.time = 0;
                fromKey.value = 0;
                
                toKey.time = 1;
                toKey.value = 1;

                var deltaTime = animationCurve.keys[index+1].time - animationCurve.keys[index].time;
                var deltaTime1 = toKey.time - fromKey.time;

                var deltaValue = animationCurve.keys[index+1].value - animationCurve.keys[index].value;
                var deltaValue1 = toKey.value - fromKey.value;

                var weight = deltaTime / deltaValue;
                var weight1 = deltaTime1 / deltaValue1;

                var tangent = weight/ weight1;

                fromKey.outTangent = fromKey.outTangent * tangent;
                toKey.inTangent = toKey.inTangent * tangent;

                return new AnimationCurve(fromKey, toKey);

            }
            //fキーが選択されていない場合、foreachに入らないので警告出して終了
            EditorUtility.DisplayDialog("キーフレームが選択されていません。", "Animationウィンドウからプリセット登録したい\nキーフレームを選択してください。", "閉じる");
            return null;
        }

        /// <summary>
        /// AnimationCurveを渡すと、Animationウィンドウで選択中のキーフレームにイージングを反映させる。
        /// </summary>
        /// <param name="curve"></param>
        internal static void SetEasingPreset(AnimationCurve curve)
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

                //選択したキーフレームがAnimationCurve内の何番目か取得
                var getIndex = animationWindowKeyframeType.GetMethod("GetIndex" ,  BindingFlags.Instance | BindingFlags.Public);
                var index = (int)getIndex.Invoke(key , null);

                //一番最後のキーフレームを選択していた場合、警告出して戻す。
                if(animationCurve.length <= index + 1)
                {
                    EditorUtility.DisplayDialog("イージングの設定に失敗しました。", "最終キーフレームには設定できません。\n別のキーフレームを選択してください。", "閉じる");
                    return;
                }

                var fromSelectKey = animationCurve.keys[index];
                var toSelectKey = animationCurve.keys[index+1];

                //書き換えるキーフレームのValueに合わせてTangentを変換
                var fromKey = curve.keys[0];
                var toKey = curve.keys[1];

                fromKey.time = fromSelectKey.time;
                fromKey.value = fromSelectKey.value;
                
                toKey.time = toSelectKey.time;
                toKey.value = toSelectKey.value;

                var deltaTime = curve.keys[1].time - curve.keys[0].time;
                var deltaTime1 = toKey.time - fromKey.time;

                var deltaValue = curve.keys[1].value - curve.keys[0].value;
                var deltaValue1 = toKey.value - fromKey.value;

                var weight = deltaTime / deltaValue;
                var weight1 = deltaTime1 / deltaValue1;

                var tangent = weight/ weight1;

                fromSelectKey.outTangent = fromKey.outTangent * tangent;
                toSelectKey.inTangent = toKey.inTangent * tangent;
                fromSelectKey.outWeight = fromKey.outWeight;
                toSelectKey.inWeight = toKey.inWeight;

                fromSelectKey.weightedMode =  SetWeightMode(fromSelectKey.weightedMode, fromKey.weightedMode, true);
                toSelectKey.weightedMode = SetWeightMode(toSelectKey.weightedMode, toKey.weightedMode, false);

                //キーフレームの編集をAnimationCurveに通知
                animationCurve.MoveKey(index,fromSelectKey);
                animationCurve.MoveKey(index+1,toSelectKey);

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
                EditorUtility.DisplayDialog("キーフレームが選択されていません。", "Animationウィンドウからイージング適用したい\nキーフレームを選択してください。", "閉じる");
            }

            return;

        }

        /// <summary>
        /// WeightModeを更新する。
        /// 手前のキーを更新する時はisOutWeightをtrue
        /// 奥のキーを更新する時はfalse
        /// </summary>
        /// <param name="beforeMode"></param>
        /// <param name="overrideMode"></param>
        /// <param name="isOutWeight"></param>
        /// <returns></returns>
        static WeightedMode SetWeightMode(WeightedMode beforeMode, WeightedMode overrideMode, bool isOutWeight)
        {
            if(beforeMode == overrideMode) return overrideMode;
            if(isOutWeight)
            {
                if(overrideMode == WeightedMode.Out || overrideMode == WeightedMode.Both)
                {
                    if(beforeMode == WeightedMode.None)
                    {
                        return WeightedMode.Out;
                    }
                    if(beforeMode == WeightedMode.In)
                    {
                        return WeightedMode.Both;
                    }
                }else
                {
                    if(beforeMode == WeightedMode.Out)
                    {
                        return WeightedMode.None;
                    }
                    if(beforeMode == WeightedMode.Both)
                    {
                        return  WeightedMode.In;
                    }
                }

            }else{
                if(overrideMode == WeightedMode.In || overrideMode == WeightedMode.Both)
                {
                    if(beforeMode == WeightedMode.None)
                    {
                        return WeightedMode.In;
                    }
                    if(beforeMode == WeightedMode.Out)
                    {
                        return WeightedMode.Both;
                    }
                }else
                {
                    if(beforeMode == WeightedMode.In)
                    {
                        return WeightedMode.None;
                    }
                    if(beforeMode == WeightedMode.Both)
                    {
                        return  WeightedMode.Out;
                    }
                }
            }
            return WeightedMode.None;
        }

    }
}





