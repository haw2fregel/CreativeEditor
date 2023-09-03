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
    /// Projectの検索条件を保存して
    /// EditorWindowから再現する
    /// </summary> 
    
    [CreateAssetMenu(menuName = "CreativeEditorSo/ProjectSerchSo" )]
    internal class CreativeEditorProjectSerchSo : CreativeEditorSo
    {

        [SerializeField]
        List<CreativeEditorProjectSerchSetting> settings;
        internal List<CreativeEditorProjectSerchSetting> Settings {get{return settings;}}
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
                                if(setting.SerchMode == CreativeEditorProjectSerchSetting.SerchModeEnum.Path)
                                {
                                    CreativeEditorProjectSerchHelper.SelectionProjectPath(setting.SelectFolderPaths);
                                }else if(setting.SerchMode == CreativeEditorProjectSerchSetting.SerchModeEnum.Keyword)
                                {
                                    CreativeEditorProjectSerchHelper.SerchProject(setting.SerchText);
                                }
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
    internal class CreativeEditorProjectSerchSetting : CreativeEditorSoSetting
    {

        [SerializeField]
        SerchModeEnum serchMode;
        internal SerchModeEnum SerchMode { get { return serchMode; } }
        internal static readonly string p_serchMode = "serchMode";
        internal void SetSerchMode(SerchModeEnum value) { serchMode = value;}

        [SerializeField]
        UnityEngine.Object[] selectFolders;
        internal UnityEngine.Object[] SelectFolders { get { return selectFolders; } }
        internal static readonly string p_selectFolders = "selectFolders";
        internal string[] SelectFolderPaths
        {
            get
            {
                List<string> selectFolderPaths = new List<string>();

                foreach (var selectFolder in selectFolders)
                {
                    selectFolderPaths.Add(AssetDatabase.GetAssetPath(selectFolder));
                }
                
                return selectFolderPaths.ToArray();
            }
        }


        [SerializeField]
        string[] serchKeywords;
        internal string[] SerchKeywords { get { return serchKeywords; } }
        internal static readonly string p_serchKeywords = "serchKeywords";

        [SerializeField]
        int serchTypeIndex;
        internal int SerchTypeIndex { get { return serchTypeIndex; } }
        internal static readonly string p_serchTypeIndex = "serchTypeIndex";
        internal string[] SerchTypeNames
        {
            get
            {
                List<string> serchTypeNames = new List<string>();

                for(int i = 0; i < SerchType.Length; i++)
                {
                    if((serchTypeIndex & 1 << i) == (1 << i)) serchTypeNames.Add("t:" + SerchType[i] + " ");
                }
                
                return serchTypeNames.ToArray();
            }
        }

        internal static readonly string[] SerchType = {"AnimationClip", "AnimatorController", "AudioClip", "AudioMixer", "ComputeShader", "Font", "GUISkin", "Material", "Mesh", "Model", "PhysicMaterial", "Prefab", "Scene", "Script", "Shader", "Sprite", "Texture", "VideoClip"};

        [SerializeField]
        int serchLabelIndex;
        internal int SerchLabelIndex { get { return serchLabelIndex; }}
        internal static readonly string p_serchLabelIndex = "serchLabelIndex";

        internal string[] SerchLabelNames
        {
            get
            {
                List<string> serchLabelNames = new List<string>();

                for(int i = 0; i < SerchType.Length; i++)
                {
                    if((serchLabelIndex & 1 << i) == (1 << i)) serchLabelNames.Add("l:" + SerchLabel[i] + " ");
                }
                
                return serchLabelNames.ToArray();
            }
        }

        internal static readonly string[] SerchLabel = {"Advanced", "Architecture", "Audio", "Character", "Effect", "Ground", "Prop", "Road", "Sky", "Terrain", "Vegetation", "Vehicle", "Wall", "Water", "Weapon"};

        internal string SerchText
        {
            get
            {
                var text = "";
                foreach (var typeName in SerchTypeNames)
                {
                    text += typeName;
                }

                foreach (var labelName in SerchLabelNames)
                {
                    text += labelName;
                }

                foreach (var keyword in serchKeywords)
                {
                    text += keyword + " ";
                }

                return text;
            }
        }

        internal enum SerchModeEnum
        {
            Path,
            Keyword
        }

        internal CreativeEditorProjectSerchSetting()
        {
            base.labelName = "プロジェクト検索";
            base.labelColor = new Color(1,1,1,1);
            base.tips = "";
            serchMode = SerchModeEnum.Path;
            selectFolders = new UnityEngine.Object[]{};
            serchKeywords = new string[]{};
            serchTypeIndex = 0;
            serchLabelIndex = 0;
        }
        

    }

    /// <summary>
    /// SoファイルのInspector表示を拡張する
    /// </summary>
    [CustomEditor(typeof(CreativeEditorProjectSerchSo))]
    internal class CreativeEditorProjectSerchSoEditor : CreativeEditorSoEditor
    {
        Vector2 _currentScrollPosition;
        List<bool> showElements = new List<bool>();
        List<bool> showComps = new List<bool>();

        CreativeEditorProjectSerchSo so = null;
        
        void OnEnable() 
        {
            so = target as CreativeEditorProjectSerchSo;

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

                var soProp = serializedObject.FindProperty(CreativeEditorProjectSerchSo.p_settings);
                
                base.PropertyElementArraySizeVaridate(soProp);

                GUI.color = new Color(2.7f,2.7f,2.7f,1f);

                using(new EditorGUILayout.VerticalScope("Box"))
                {
                    EditorGUI.indentLevel++;
                    GUI.color = new Color(1f,1f,1f,1f);
                    EditorGUILayout.LabelField("プロジェクトフォルダ検索");
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
                                base.ButtomLooksSettingFields<CreativeEditorProjectSerchSetting>(element);

                                EditorGUILayout.Space();

                                EditorGUI.BeginChangeCheck();
                                GUI.color = new Color(1f,1f,1f,1f);
                                var serchMode = so.Settings[i].SerchMode;
                                serchMode = (CreativeEditorProjectSerchSetting.SerchModeEnum)EditorGUILayout.EnumPopup("検索方法",serchMode);

                                if (EditorGUI.EndChangeCheck())
                                {
                                    so.Settings[i].SetSerchMode(serchMode);
                                    EditorUtility.SetDirty(so);
                                    AssetDatabase.SaveAssets();
                                }

                                EditorGUI.indentLevel++;

                                if(so.Settings[i].SerchMode == CreativeEditorProjectSerchSetting.SerchModeEnum.Path)
                                {
                                    EditorGUILayout.LabelField("パス指定");
                                    EditorGUILayout.LabelField("入力したパスと完全一致するアセット、フォルダを選択状態にします");
                                    EditorGUILayout.PropertyField(element.FindPropertyRelative(CreativeEditorProjectSerchSetting.p_selectFolders), new GUIContent("検索フォルダパス"));


                                }else if(so.Settings[i].SerchMode == CreativeEditorProjectSerchSetting.SerchModeEnum.Keyword)
                                {
                                    EditorGUILayout.PropertyField(element.FindPropertyRelative(CreativeEditorProjectSerchSetting.p_serchKeywords), new GUIContent("キーワード"));
                                    element.FindPropertyRelative(CreativeEditorProjectSerchSetting.p_serchTypeIndex).intValue = EditorGUILayout.MaskField(new GUIContent("タイプ"), element.FindPropertyRelative(CreativeEditorProjectSerchSetting.p_serchTypeIndex).intValue, CreativeEditorProjectSerchSetting.SerchType);
                                    element.FindPropertyRelative(CreativeEditorProjectSerchSetting.p_serchLabelIndex).intValue = EditorGUILayout.MaskField(new GUIContent("ラベル"), element.FindPropertyRelative(CreativeEditorProjectSerchSetting.p_serchLabelIndex).intValue, CreativeEditorProjectSerchSetting.SerchLabel);
                                }

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
                                var setting = new CreativeEditorProjectSerchSetting();
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
    /// ProjectWindowに検索条件を投げ込むための補助クラス
    /// </summary>
    internal class CreativeEditorProjectSerchHelper
    {

        /// <summary>
        /// 指定パスのアセットを選択状態にします。
        /// </summary>
        /// <param name="path"></param>
        internal static void SelectionProjectPath(string[] paths)
        {
            var flag = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var assembly = typeof( Editor ).Assembly;
            var projectBrowserType = assembly.GetType( "UnityEditor.ProjectBrowser" );
            var projectBrowser = EditorWindow.GetWindow( projectBrowserType );

            //フォルダIDを取得するメソッドを取得
            var GetFolderInstanceIDs = projectBrowserType.GetMethod("GetFolderInstanceIDs", flag);
            
            var SetFolderSelections = projectBrowserType.GetMethods(flag);
            //任意IDのフォルダを選択するメソッドを取得
            var SetFolderSelection = SetFolderSelections.FirstOrDefault(c =>
            {
                return
                c.Name == "SetFolderSelection" &&
                c.GetParameters().Count() == 2;
            });

            //渡されたパスのフォルダIDを取得
            int[] folderids = (int[])GetFolderInstanceIDs.Invoke(null, new[] { paths });

            //取得したIDのフォルダを選択（第二引数はとりあえずfalse）
            SetFolderSelection.Invoke(projectBrowser, new object[] { folderids, false });
        }


        /// <summary>
        /// プロジェクトウィンドウの検索欄に指定文字列を投げ込みます。
        /// </summary>
        /// <param name="filter"></param>
        internal static void SerchProject(string filter)
        {
            var projectWindowType = Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
            var projectWindow = EditorWindow.GetWindow( projectWindowType, false, null, true);

            var setSearchMethod = projectWindowType.GetMethod
            (
                name: "SetSearch",
                bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof( string ) },
                modifiers: null
            );

            setSearchMethod.Invoke(obj: projectWindow,parameters: new object[]{filter});
        }

        
    }

}





