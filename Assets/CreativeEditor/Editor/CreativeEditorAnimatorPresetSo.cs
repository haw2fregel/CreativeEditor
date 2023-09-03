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
    [CreateAssetMenu(menuName = "CreativeEditorSo/AnimatorPresetSo" )]
    internal class CreativeEditorAnimatorPresetSo : CreativeEditorSo
    {
        [SerializeField]
        List<CreativeEditorAnimatorPresetSetting> settings;
        internal List<CreativeEditorAnimatorPresetSetting> Settings {get{return settings;}}
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
                            //選択中のキーフレームにイージングを反映させる。
                            if (CreativeEditorCustomGUI.Button(new GUIContent(setting.LabelName, setting.Tips), true))
                            {
                                if(Selection.gameObjects.Length == 0)
                                {
                                    EditorUtility.DisplayDialog("オブジェクトが未選択です。", "Animatorを付与するオブジェクトを選択してください。", "閉じる");
                                    GUI.color = normalColor;
                                    return;
                                }

                                var path = CreativeEditorCustomGUI.SelectionAssetPath("", "controller");
                                if(path == "" | path == null) return;

                                var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);

                                var rootStateMachine = controller.layers[0].stateMachine;

                                for (int i = 0; i < setting.StateNames.Count; i++)
                                {
                                    var state = rootStateMachine.AddState(setting.StateNames[i]);
                                    var anim = new AnimationClip();
                                    if(setting.Clips[i] != null)
                                    {
                                        anim = UnityEngine.Object.Instantiate(setting.Clips[i]);
                                    }
                                    anim.name = setting.StateNames[i];
                                    state.motion = anim;
                                    AssetDatabase.AddObjectToAsset(anim, controller);

                                    if(setting.EntryStateIndex == i)
                                    {
                                        rootStateMachine.defaultState = state;
                                    }

                                    controller.AddParameter(setting.StateNames[i], AnimatorControllerParameterType.Trigger);

                                    var transition = rootStateMachine.AddAnyStateTransition(state);
                                    transition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, setting.StateNames[i]);
                                    transition.duration = 0;
                                }

                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(controller));

                                var obj = Selection.gameObjects[0];
                                var animator = obj.AddComponent<Animator>();
                                animator.runtimeAnimatorController = controller;

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
    /// Animator生成プリセットの情報を保存するクラス
    /// </summary>
    [Serializable]
    internal class CreativeEditorAnimatorPresetSetting : CreativeEditorSoSetting
    {
        [SerializeField]
        List<string> stateNames;
        internal List<string> StateNames { get { return stateNames; }}
        internal static readonly string p_stateNames = "stateNames";

        [SerializeField]
        List<AnimationClip> clips;
        internal List<AnimationClip> Clips { get { return clips; }}
        internal static readonly string p_clips = "clips";

        [SerializeField]
        int entryStateIndex;
        internal int EntryStateIndex { get { return entryStateIndex;}}
        internal static readonly string p_entryStateIndex = "entryStateIndex";

        internal CreativeEditorAnimatorPresetSetting()
        {
            base.labelName = "Animator付与";
            base.labelColor = new Color(1, 1, 1, 1);
            base.tips = "";
            clips = new List<AnimationClip>();
            stateNames = new List<string>();
            entryStateIndex = 0;
        }

    }


    /// <summary>
    /// SoファイルのInspector表示を拡張する
    /// </summary>
    [CustomEditor(typeof(CreativeEditorAnimatorPresetSo))]
    internal class CreativeEditorAnimatorPresetSoEditor : CreativeEditorSoEditor
    {
        Vector2 _currentScrollPosition;
        List<bool> showElements = new List<bool>();
        List<bool> showAnims = new List<bool>();

        CreativeEditorAnimatorPresetSo so = null;
        
        void OnEnable() 
        {
            so = target as CreativeEditorAnimatorPresetSo;

            base.showListAll.Clear();
            base.showListAll.Add(showElements);
            base.showListAll.Add(showAnims);
            
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using( new EditorGUILayout.VerticalScope())
            {
                serializedObject.Update();

                var soProp = serializedObject.FindProperty(CreativeEditorAnimatorPresetSo.p_settings);
                
                base.PropertyElementArraySizeVaridate(soProp);

                GUI.color = new Color(2.7f,2.7f,2.7f,1f);

                using( var vertical = new EditorGUILayout.VerticalScope("Box"))
                {
                    EditorGUI.indentLevel++;
                    GUI.color = new Color(1f,1f,1f,1f);
                    EditorGUILayout.LabelField("アニメータープリセット");
                    GUI.color = new Color(0.3f,0.3f,0.3f,1f);
                    using( var vertical2 = new EditorGUILayout.VerticalScope("Box"))
                    {
                        GUI.color = new Color(2f,2f,2f,1f);
                        EditorGUI.indentLevel++;

                        using (var scrollView = new EditorGUILayout.ScrollViewScope(_currentScrollPosition))
                        {

                            _currentScrollPosition = scrollView.scrollPosition;

                            
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
                                    base.ButtomLooksSettingFields<CreativeEditorAnimatorPresetSetting>(element);
                                    showAnims[i] = EditorGUILayout.Foldout(showAnims[i], "アニメーター設定");
                                EditorGUI.indentLevel--;

                                if(!showAnims[i]) 
                                {
                                    GUI.color = new Color(4f,4f,4f,1f);
                                    GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
                                    GUI.color = new Color(2f,2f,2f,1f);
                                    continue;
                                }
                                
                                EditorGUI.indentLevel++;
                                EditorGUI.indentLevel++;                 
                                GUI.color = new Color(1f,1f,1f,1f);

                                var stateNames = so.Settings[i].StateNames;
                                var clips = so.Settings[i].Clips;
                                for(int j = 0; j < stateNames.Count; j++)
                                {
                                    using (new EditorGUILayout.HorizontalScope()) 
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        stateNames[j] = EditorGUILayout.TextField( stateNames[j]);
                                        clips[j] = (AnimationClip)EditorGUILayout.ObjectField( clips[j], typeof(AnimationClip), false);

                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            EditorUtility.SetDirty(so);
                                            return;
                                        }
                                    }
                                }

                                using (new EditorGUILayout.HorizontalScope()) 
                                {
                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button("+", GUILayout.Width(25)))
                                    {
                                        Undo.RecordObject(so, "CreativeEditorObjectPreset AddObject");
                                        stateNames.Add("state");
                                        clips.Add(null);
                                        EditorUtility.SetDirty(so);
                                        return;
                                    }
                                    
                                    if (GUILayout.Button("-",GUILayout.Width(25))) 
                                    {
                                        if (soProp.arraySize >= 1) 
                                        {
                                            Undo.RecordObject(so, "CreativeEditorObjectPreset AddObject");
                                            stateNames.RemoveAt(stateNames.Count - 1);
                                            clips.RemoveAt(clips.Count - 1);
                                            return;
                                        }
                                    }
                                }


                                EditorGUI.indentLevel--;

                                var entryIndex = so.Settings[i].EntryStateIndex;
                                
                                entryIndex = EditorGUILayout.Popup("エントリーステート", entryIndex, so.Settings[i].StateNames.ToArray());
                                element.FindPropertyRelative(CreativeEditorAnimatorPresetSetting.p_entryStateIndex).intValue = entryIndex;


                                GUI.color = new Color(2f,2f,2f,1f);
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

                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("+", GUILayout.Width(25)))
                            {
                                Undo.RecordObject(so, "CreativeEditorSo AddSetting");
                                var setting = new CreativeEditorAnimatorPresetSetting();
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
}





