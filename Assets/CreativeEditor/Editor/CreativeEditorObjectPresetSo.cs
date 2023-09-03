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
    /// オブジェクトの情報をプリセットとしてScriptableObjectに保存し、
    /// ワンボタンで同じものを生成する拡張Editor
    /// CreativeEditorWindowに生成したSoファイルを読み込ませて利用する
    /// </summary>
    [CreateAssetMenu(menuName = "CreativeEditorSo/ObjectPresetSo" )]
    internal class CreativeEditorObjectPresetSo : CreativeEditorSo
    {

        [SerializeField]
        List<CreativeEditorObjectSetting> settings;
        internal List<CreativeEditorObjectSetting> Settings {get{return settings;}}
        internal static readonly string p_settings = "settings";
        int createPos = 0;
        Vector2 _currentScrollPosition;

        internal override void ShowWindow()
        {
            base.ShowWindow();
            if (Settings.Count == 0) return;
            if (!base.showWindow) return;

            using (new EditorGUILayout.VerticalScope("box")) 
            {
                //Sceneのオブジェクトを選択している場合にどこに生成するか選択できるようにする
                //オブジェクト未選択の場合はroot直下
                if (Selection.gameObjects.Length != 0)
                {
                    var normalColor = GUI.color;
                    using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
                    {

                        using (new EditorGUI.DisabledScope(createPos == 0))
                        {
                            if (createPos == 0)
                            {
                                GUI.color = new Color(1.5f, 1.5f, 1.5f, 1);
                            }
                            else
                            {
                                GUI.color = new Color(0.75f, 0.75f, 0.75f, 1);
                            }
                            if (GUILayout.Button("子階層に生成", EditorStyles.toolbarButton))
                            {
                                createPos = 0;
                            }
                        }
                        using (new EditorGUI.DisabledScope(createPos == 1))
                        {
                            if (createPos == 1)
                            {
                                GUI.color = new Color(1.5f, 1.5f, 1.5f, 1);
                            }
                            else
                            {
                                GUI.color = new Color(0.75f, 0.75f, 0.75f, 1);
                            }
                            if (GUILayout.Button("同階層に生成", EditorStyles.toolbarButton))
                            {
                                createPos = 1;
                            }
                        }
                    }
                    GUI.color = normalColor;
                    EditorGUILayout.Space();
                }
            
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
                                var obj = new GameObject(setting.ObjName);

                                //生成したオブジェクトの階層を移動
                                if (Selection.gameObjects.Length != 0)
                                {
                                    if (createPos == 0)
                                    {
                                        obj.transform.SetParent(Selection.gameObjects[0].transform);
                                    }
                                    else
                                    {
                                        obj.transform.SetParent(Selection.gameObjects[0].transform.parent);
                                    }
                                    obj.layer = Selection.gameObjects[0].layer;
                                }

                                //コンポーネントリストに保存されている内容を全て追加
                                //Transform,ParticleSystemRendereは自動付与のためAddComponentをしない
                                for (int i = 0; i < setting.CompNames.Count; i++)
                                {
                                    var name = setting.GetCompNamesArrayIndex(i);

                                    UnityEngine.Object component = null;
                                    if (name == "Transform")
                                    {
                                        component = obj.transform;
                                    }
                                    else if (name == "ParticleSystemRenderer")
                                    {
                                        component = obj.GetComponent<ParticleSystemRenderer>();
                                    }
                                    else
                                    {
                                        var type = CreativeEditorObjectPresetHelper.GetType(setting.GetCompNamesArrayIndex(i));
                                        component = obj.AddComponent(type);
                                    }
                                    setting.GetPresetsArrayIndex(i).ApplyTo(component);
                                }
                                Undo.RegisterCreatedObjectUndo(obj, "Create object");
                            }
                            GUI.color = normalColor;
                        }
                    }
                }
                CreativeEditorCustomGUI.PartitionLine();
            }
        }

    }

    /// <summary>
    /// オブジェクトのプリセット情報を保存するクラス
    /// 複数のコンポーネント名と対応するPresetで再現する
    /// </summary>
    [Serializable]
    internal class CreativeEditorObjectSetting : CreativeEditorSoSetting
    {
        [SerializeField]
        string objName;
        internal string ObjName{get{return objName;}}
        internal static readonly string p_objName = "objName";

        [SerializeField]
        List<Preset> presets;
        internal List<Preset> Presets{get{return presets;}}
        internal Preset GetPresetsArrayIndex(int index){return presets[index];}
        internal void SetPresetsArrayIndex(int index, Preset value){presets[index] = value;}
        internal static readonly string p_presets = "presets";

        [SerializeField]
        List<string> compNames;
        internal List<string> CompNames{get{return compNames;}}
        internal string GetCompNamesArrayIndex(int index){return compNames[index];}
        internal void SetCompNamesArrayIndex(int index, string value){compNames[index] = value;}
        internal static readonly string p_compNames = "compNames";

        internal CreativeEditorObjectSetting(CreativeEditorObjectPresetSo so)
        {
            base.labelName = "オブジェクト";
            base.labelColor = new Color(1,1,1,1);
            base.tips = "";
            objName = "Object";
            compNames = new List<string>();
            presets = new List<Preset>();

            var obj = new GameObject();
            var preset = new Preset(obj.transform);
            compNames.Add("Transform");
            presets.Add(preset);
            preset.name = "Transform";
            AssetDatabase.AddObjectToAsset(preset, so);
            AssetDatabase.SaveAssets();
            UnityEngine.Object.DestroyImmediate(obj);
        }

        internal CreativeEditorObjectSetting(GameObject gameObject, CreativeEditorObjectPresetSo so)
        {  
            List<Type> getTypes = new List<Type>();
            List<Preset> getPresets = new List<Preset>();

            foreach (var component in gameObject.GetComponents<Component> ())
            {
                getTypes.Add(component.GetType ());
                var preset = new Preset(component);
                getPresets.Add(preset);
                preset.name = component.name;
            }

            base.labelName = gameObject.name;
            base.labelColor = new Color(1, 1, 1, 1);
            base.tips = "";
            objName = gameObject.name;
            compNames = new List<string>();
            presets = new List<Preset>();
            foreach (Type type in getTypes)
            {
                compNames.Add(type.Name);
            }
            foreach (var preset in getPresets)
            {
                preset.name = objName + "_" + preset.GetTargetTypeName();
                presets.Add(preset);
                AssetDatabase.AddObjectToAsset(preset, so);
            }

            AssetDatabase.SaveAssets();
        }

    }

    /// <summary>
    /// SoファイルのInspector表示を拡張する
    /// </summary>
    [CustomEditor(typeof(CreativeEditorObjectPresetSo))]
    internal class CreativeEditorObjectPresetSoEditor : CreativeEditorSoEditor
    {
        Vector2 _currentScrollPosition;
        List<bool> showElements = new List<bool>();
        List<bool> showComps = new List<bool>();

        CreativeEditorObjectPresetSo so = null;

        int selectIndex = 0;
        
        void OnEnable() 
        {
            so = target as CreativeEditorObjectPresetSo;

            base.showListAll.Clear();
            base.showListAll.Add(showElements);
            base.showListAll.Add(showComps);
            
        }

        void OnDisable()
        {
            var path = AssetDatabase.GetAssetPath(so);
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var subAsset in subAssets)
            {
                if(AssetDatabase.IsMainAsset(subAsset)){continue;}
                var isUse = false;
                foreach (var setting in so.Settings)
                {
                    foreach (var preset in setting.Presets)
                    {
                        if(preset == subAsset)
                        {
                            isUse = true;
                            break;
                        }
                    }
                }
                if (isUse){continue;}
                DestroyImmediate(subAsset, true);
            }
            AssetDatabase.SaveAssets();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using( var verticalAll = new EditorGUILayout.VerticalScope())
            {
                serializedObject.Update();

                var soProp = serializedObject.FindProperty(CreativeEditorObjectPresetSo.p_settings);
                
                base.PropertyElementArraySizeVaridate(soProp);

                GUI.color = new Color(2.7f,2.7f,2.7f,1f);

                using(new EditorGUILayout.VerticalScope("Box"))
                {
                    EditorGUI.indentLevel++;
                    GUI.color = new Color(1f,1f,1f,1f);
                    EditorGUILayout.LabelField("オブジェクト生成プリセット");
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

                                    GUI.color = new Color(1f,1f,1f,0.75f);
                                    if (GUILayout.Button("プレビュー", GUILayout.Width(60)))
                                    {
                                        var previewPresets = so.Settings[i].Presets;
                                        CreativeEditorPresetPreview.Show(previewPresets);
                                        return;
                                    }
                                    GUI.color = new Color(2f,2f,2f,1f);

                                    if(base.PropertyElementMoveUpButton(serializedObject, soProp, i)) return;
                                    if(base.PropertyElementMoveDownButton(serializedObject, soProp, i)) return;
                                    if(base.PropertyElementDeleteButton(serializedObject, soProp, i)) return;

                                }
                                    
                                if(!showElements[i]) continue;
                                        
                                EditorGUI.indentLevel++;
                                    base.ButtomLooksSettingFields<CreativeEditorObjectSetting>(element);
                                    EditorGUILayout.PropertyField(element.FindPropertyRelative(CreativeEditorObjectSetting.p_objName), new GUIContent("オブジェクト名"));
                                    showComps[i] = EditorGUILayout.Foldout(showComps[i], "コンポーネント");
                                EditorGUI.indentLevel--;

                                if(!showComps[i]) continue;
                                
                                EditorGUI.indentLevel++;
                                EditorGUI.indentLevel++;                   
                                GUI.color = new Color(1f,1f,1f,1f);

                                var compNames = so.Settings [i].CompNames;
                                var presets = so.Settings [i].Presets;     
                                for(int j = 0; j < compNames.Count; j++)
                                {
                                    using (var horizontal = new EditorGUILayout.HorizontalScope()) 
                                    {
                                        EditorGUILayout.LabelField(compNames[j]);                                                 
                                        if (GUILayout.Button("編集", GUILayout.Width(40)))
                                        {
                                            CreativeEditorPresetPreview.Show(presets[j]);
                                            return;
                                        }
                                        if (GUILayout.Button("削除", GUILayout.Width(40)))
                                        {
                                            compNames.RemoveAt(j);
                                            presets.RemoveAt(j);
                                            return;
                                        }
                                    }
                                }
                                if (GUILayout.Button("AddComponent"))
                                {
                                    var buttonLabel = new GUIContent("Show");
                                    var buttonStyle = EditorStyles.toolbarButton;
                                    var buttonRect = GUILayoutUtility.GetRect(buttonLabel, buttonStyle);

                                    ComponentAdvancedDropdown dropdown = new ComponentAdvancedDropdown(new AdvancedDropdownState());

                                    dropdown.onItemSelected += OnItemSelected;
                                    selectIndex = i;

                                    dropdown.Show(buttonRect);
                                }
                                EditorGUI.indentLevel--;

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
                            using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
                            {
                                if (GUILayout.Button("選択オブジェクトを登録", GUILayout.Width(145)))
                                {
                                    Undo.RecordObject(so, "CreativeEditorObjectPreset CopyObject");
                                    var selectObj = Selection.gameObjects[0];
                                    var setting = new CreativeEditorObjectSetting(selectObj, so);
                                    so.Settings .Add(setting);
                                    EditorUtility.SetDirty(so);

                                    return;
                                }
                            }

                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("+", GUILayout.Width(25)))
                            {
                                Undo.RecordObject(so, "CreativeEditorObjectPreset AddObject");
                                var setting = new CreativeEditorObjectSetting(so);
                                so.Settings .Add(setting);
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

        /// <summary>
        /// AddComponentウィンドウで選択したコンポーネントを取得・反映する
        /// </summary>
        /// <param name="compName"></param>
        private void OnItemSelected(string compName)
        {
            Undo.RecordObject(so, "CreativeEditorObjectPreset AddComponent");
            var setting = so.Settings [selectIndex];
            var type = CreativeEditorObjectPresetHelper.GetType(compName);
            var preset = CreativeEditorObjectPresetHelper.GetNewPreset(compName);
            if (setting.CompNames.Contains(compName))
            {
                EditorUtility.DisplayDialog("コンポーネントの重複", compName + "は既にセットされています。", "閉じる");
                return;
            }
            if (compName == "Transform" && setting.CompNames.Contains("RectTransform"))
            {
                var t_index = setting.CompNames.IndexOf("RectTransform");
                setting.CompNames[t_index] = compName;
                setting.Presets[t_index] = preset;
                EditorUtility.SetDirty(so);
                return;
            }
            if (compName == "RectTransform" && setting.CompNames.Contains("Transform"))
            {
                var t_index = setting.CompNames.IndexOf("Transform");
                setting.CompNames[t_index] = compName;
                setting.Presets[t_index] = preset;
                EditorUtility.SetDirty(so);
                return;
            }
            setting.CompNames.Add(compName);
            setting.Presets.Add(preset);
            preset.name = compName;
            AssetDatabase.AddObjectToAsset(preset, so);
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// オブジェクトをプリセットとして保存するための補助クラス
    /// ComponentやPresetを名前から扱うためのメソッドを提供する
    /// </summary>
    internal class CreativeEditorObjectPresetHelper
    {
        /// <summary>
        /// コンポーネント名からデフォルトのPresetを生成する
        /// 一時的にGameObjectを生成し、AddComponetしてからPreset変換している。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static Preset GetNewPreset(string name)
        {
            var obj = new GameObject();
            var type = GetType(name);
            UnityEngine.Object component = null;
            if (type == typeof(Transform))
            {
                component = obj.transform;
            }
            else
            {
                component = obj.AddComponent(type);
            }
            var preset = new Preset(component);
            UnityEngine.Object.DestroyImmediate(obj);
            return preset;
        }


        static MonoScript[] _monoScripts;
        /// <summary>
        /// プロジェクト内に存在する全スクリプトファイル
        /// </summary>
        static MonoScript[] MonoScripts
        {
            get { return _monoScripts ?? (_monoScripts = Resources.FindObjectsOfTypeAll<MonoScript>().ToArray()); }
        }

        /// <summary>
        /// クラス名からタイプを取得する
        /// </summary>
        internal static Type GetType(string className)
        {
            var _typeDict = GetAllTypeDictionary();
            //クラスが存在する場合、リストに表示
            if (_typeDict.ContainsKey(className))
            {
                return _typeDict[className][0];
            }
            return null;
        }

        /// <summary>
        /// 全ComponentをDictionaryで取得する
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, List<Type>> GetAllTypeDictionary()
        {
            var _typeDict = new Dictionary<string, List<Type>>();
            var _cashedComponents = GetAllTypes();
            foreach (var type in _cashedComponents)
            {
                if (!_typeDict.ContainsKey(type.Name))
                {
                    _typeDict.Add(type.Name, new List<Type>());
                }

                _typeDict[type.Name].Add(type);
            }
            return _typeDict;
        }

        /// <summary>
        /// 全てのクラスタイプを取得
        /// </summary>
        static IEnumerable<Type> GetAllTypes()
        {
            //Unity標準のクラスタイプを取得する
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type != null && !string.IsNullOrEmpty(type.Namespace))
                .Where(type => type.Namespace.Contains("UnityEngine"))
                .Where(type => type.IsSubclassOf(typeof(Component))); 

             //自作クラスも取得できるように
             var localTypes = MonoScripts
                .Where(script => script != null)
                .Select(script => script.GetClass())
                .Where(classType => classType != null)
                .Where(classType => classType.Module.Name == "Assembly-CSharp.dll");

            return types.Concat(localTypes).Distinct();
        }
    }

    /// <summary>
    /// Presetを既存のInspector表示でプレビューする
    /// </summary>
    internal class CreativeEditorPresetPreview
    {
        static List<Preset> target = new List<Preset>();
        static List<string> targetTypeName = new List<string>();
        Vector2 _currentScrollPosition;
        internal static void Show(List<Preset> objs)
        {
            var inspectorWindowType = Assembly.Load("UnityEditor").GetType("UnityEditor.InspectorWindow");

            var originalSelectedObjects = new UnityEngine.Object[Selection.objects.Length];
            Array.Copy(Selection.objects, originalSelectedObjects, Selection.objects.Length);

            foreach (var obj in objs)
            {
                var inspectorWindow = EditorWindow.CreateInstance(inspectorWindowType) as EditorWindow;

                var isLockedPropertyInfo = inspectorWindowType.GetProperty("isLocked", BindingFlags.Public | BindingFlags.Instance);
                Selection.objects = new UnityEngine.Object[] { obj };
                isLockedPropertyInfo.SetValue(inspectorWindow, true);

                inspectorWindow.Show(true);
            }

            Selection.objects = originalSelectedObjects;
        }

        internal static void Show(Preset obj)
        {
            var inspectorWindowType = Assembly.Load("UnityEditor").GetType("UnityEditor.InspectorWindow");

            var originalSelectedObjects = new UnityEngine.Object[Selection.objects.Length];
            Array.Copy(Selection.objects, originalSelectedObjects, Selection.objects.Length);

            var inspectorWindow = EditorWindow.CreateInstance(inspectorWindowType) as EditorWindow;

            var isLockedPropertyInfo = inspectorWindowType.GetProperty("isLocked", BindingFlags.Public | BindingFlags.Instance);
            Selection.objects = new UnityEngine.Object[] { obj };
            isLockedPropertyInfo.SetValue(inspectorWindow, true);

            inspectorWindow.Show(true);

            Selection.objects = originalSelectedObjects;
        }
    }

    /// <summary>
    /// AddComponentウィンドウを開く
    /// 既存のAddComponentウィンドウの再現が難しかったので
    /// カテゴリは独断と偏見で
    /// </summary>
    internal class ComponentAdvancedDropdown : AdvancedDropdown
    {
        internal event Action<string> onItemSelected = null;
        private Dictionary<int, string> compDictionary = null;
        readonly string[] categorys = { "３Ⅾオブジェクト", "２Ⅾオブジェクト" , "アニメーション", "イベント", "エフェクト" , "オーディオ", "トランスフォーム", "ライト" ,"レンダリング" ,"レイアウト" , "物理演算３Ⅾ" , "物理演算２Ⅾ", "AI",  "UI" ,"UIレイアウト","デバッグ","その他"};

        internal ComponentAdvancedDropdown(AdvancedDropdownState state) : base(state)
        {
            compDictionary = new Dictionary<int, string>();

            var minSize = minimumSize;
            minSize.y = 200;
            minSize.x = 300;
            minimumSize = minSize;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Root");
            //var allComponentPath = Unsupported.GetSubmenus("Component");
            var allTypeDictionary = CreativeEditorObjectPresetHelper.GetAllTypeDictionary();
            var items = new List<AdvancedDropdownItem>();
            foreach (var category in categorys)
            {
                items.Add(new AdvancedDropdownItem(category));
            }

            foreach (KeyValuePair<string, List<Type>> dict  in allTypeDictionary)
            {
                var item = new AdvancedDropdownItem(dict.Key);
                var fullPath = dict.Value[0].Namespace + "." +  dict.Key;
                var index = Array.IndexOf(categorys, "その他");

                if (fullPath.Contains("Debug") || fullPath.Contains("Test"))
                {
                    index = Array.IndexOf(categorys, "デバッグ");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }
                if (fullPath.Contains("Event") || fullPath.Contains("Raycast"))
                {
                    index = Array.IndexOf(categorys, "イベント");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }
                if (fullPath.Contains("Particle") || fullPath.Contains("LineRenderer") || fullPath.Contains("TrailRenderer") || fullPath.Contains("Halo") || fullPath.Contains("Flare") || fullPath.Contains("VFX"))
                {
                    index = Array.IndexOf(categorys, "エフェクト");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }
                if (fullPath.Contains("Collider") || fullPath.Contains("Effector") || fullPath.Contains("Joint") || fullPath.Contains("Rigid") 
                    || fullPath.Contains("Physics") || fullPath.Contains("Wind") || fullPath.Contains("Body") || fullPath.Contains("Cloth") || fullPath.Contains("Force"))
                {
                    if(fullPath.Contains("2D"))
                    {
                        index = Array.IndexOf(categorys, "物理演算２Ⅾ");
                    }
                    else
                    {
                        index = Array.IndexOf(categorys, "物理演算３Ⅾ");
                    }
                    
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }
                if (fullPath.Contains("Light") || fullPath.Contains("Shadow") || fullPath.Contains("Sky") || fullPath.Contains("Probe") || fullPath.Contains("Occlusion"))
                {
                    index = Array.IndexOf(categorys, "ライト");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }
                if (fullPath.Contains("UI") && fullPath.Contains("Layout") ||  fullPath.Contains("Canvas"))
                {
                    index = Array.IndexOf(categorys, "UIレイアウト");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }
                if (fullPath.Contains("Group") || fullPath.Contains("Sorting") || fullPath.Contains("Grid") || fullPath.Contains("Layout"))
                {
                    index = Array.IndexOf(categorys, "レイアウト");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }

                
                if (fullPath.Contains("UnityEngine.AI"))
                {
                    index = Array.IndexOf(categorys, "AI");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }
                
                if (fullPath.Contains("UI")  || fullPath.Contains("Text") || fullPath.Contains("TMP"))
                {
                    index = Array.IndexOf(categorys, "UI");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }

                if (fullPath.Contains("MeshRenderer") || fullPath.Contains("MeshFilter") || fullPath.Contains("Terrain") || fullPath.Contains("Tree"))
                {
                    index = Array.IndexOf(categorys, "３Ⅾオブジェクト");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }

                if(fullPath.Contains("Sprite") || fullPath.Contains("Tilemap"))
                {
                    index = Array.IndexOf(categorys, "２Ⅾオブジェクト");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }
                if (fullPath.Contains("Anim") || fullPath.Contains("Playable") || fullPath.Contains("Constraint") || fullPath.Contains("Controller") || fullPath.Contains("Timeline"))
                {
                    index = Array.IndexOf(categorys, "アニメーション");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }
                
                if (fullPath.Contains("Audio"))
                {
                    index = Array.IndexOf(categorys, "オーディオ");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }
                
                if(fullPath.Contains("Camera") || fullPath.Contains("Renderer") || fullPath.Contains("Cinemachine") || fullPath.Contains("Projector") || fullPath.Contains("Video") || fullPath.Contains("Volume")) 
                {
                    index = Array.IndexOf(categorys, "レンダリング");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }

                if (fullPath.Contains("Transform"))
                {
                    index = Array.IndexOf(categorys, "トランスフォーム");
                    items[index].AddChild(item);
                    compDictionary[item.id] = dict.Key;
                    continue;
                }
                items[index].AddChild(item);
                compDictionary[item.id] = dict.Key;
            }

            foreach (var item in items)
            {
                root.AddChild(item);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            onItemSelected?.Invoke(compDictionary[item.id]);
        }
    }
}





