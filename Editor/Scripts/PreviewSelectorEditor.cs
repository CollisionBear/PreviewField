using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CollisionBear.PreviewObjectPicker
{
    [InitializeOnLoad]
    public class PreviewSelectorEditor : EditorWindow
    {
        private const string EditorName = "Preview Field";
        private const string Version = "1.3.3";
        private const string CollisionBearUrl = "https://assetstore.unity.com/publishers/82099";

        private const int ListViewWidth = 280;
        private const int ListViewItemHeight = 18;
        private const int FooterHeight = 30;

        private static readonly Vector2 MinWindowSize = new Vector2(400, 400);
        static Dictionary<System.Type, List<Asset<Object>>> ObjectCache = new Dictionary<System.Type, List<Asset<Object>>>();

        private static Texture2D LogoTexture;

        public class Asset<T> where T : Object
        {
            public T Object;
            public long Id;

            public Texture Icon;
            public string Name;

            public Asset(T o, long id)
            {
                Object = o;
                Id = id;
                Icon = AssetPreview.GetMiniThumbnail(Object);
                Name = GetObjectName(o);
            }

            private string GetObjectName(T o) => o?.name ?? "None";
        }

        public static void ShowAuxWindow(System.Type type, SerializedProperty serializedProperty)
        {
            var window = CreateInstance<PreviewSelectorEditor>();
            window.ChangeSelectedType(type);
            window.SerializedProperty = serializedProperty;
            window.SetSelectedObject(serializedProperty.objectReferenceInstanceIDValue);
            window.titleContent = new GUIContent(type.Name);
            window.minSize = MinWindowSize;
            window.ShowAuxWindow();
        }

        public System.Type SelectedType;

        public string FilterString;
        public Asset<Object> SelectedObject;
        public Editor SelectedObjectEditor;
        public SerializedProperty SerializedProperty;

        public List<Asset<Object>> FoundObjects = new List<Asset<Object>>();
        public List<Asset<Object>> FilteredObjects = new List<Asset<Object>>();

        private int SelectedObjectIndex;

        private float PreviewWidth = 0;
        private float PreviewHeight = 0;
        private float ScrollViewHeight = 0;
        private Vector2 ListScrollViewOffset;

        private SearchField ObjectSearchField;

        private GUIStyle SelectedStyle;
        private GUIStyle UnselectedStyle;

        private void OnEnable()
        {
            CreateStyles();

            ObjectSearchField = new SearchField();
            ObjectSearchField.SetFocus();

            LogoTexture = Resources.Load<Texture2D>("CollsionsBearLogo");
        }

        private void OnGUI()
        {
            PreviewWidth = position.width - (ListViewWidth + 32);
            PreviewHeight = position.height - 28;

            EditorGUILayout.Space();
            HandleKeyboardInput();
            DrawLayout();
        }

        private void OnDisable()
        {
            if (SelectedObjectEditor != null) {
                GameObject.DestroyImmediate(SelectedObjectEditor);
            }
        }

        public void CreateStyles() {
            SelectedStyle = new GUIStyle(GUI.skin.label);
            SelectedStyle.normal.textColor = Color.white;
            SelectedStyle.normal.background = PreviewRenderingUtility.CreateTexture(300, ListViewItemHeight, new Color(0.24f, 0.48f, 0.9f));

            UnselectedStyle = new GUIStyle(GUI.skin.label);

        }

        private void DrawLayout() {
            using (new EditorGUILayout.HorizontalScope()) {
                using (new EditorGUILayout.VerticalScope(GUI.skin.box, GUILayout.Width(ListViewWidth))) {
                    DisplayObjectList();
                }

                if (SelectedObject != null) {
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
                        DisplayRightColumn();
                    }
                }
            }
        }

        private void DisplayObjectList() {
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField("Found " + FilteredObjects.Count());
                if (GUILayout.Button("Refresh", GUILayout.Width(64))) {
                    RefreshObjects();
                }
            }

            DisplaySearchField();
            DisplayScrollListView();
            DisplayBottomRow();
            DisplayFooter();
        }

        public void RefreshObjects() {
            if (SelectedType != null && FilterString != null) {
                UpdateFilter(FilterString);
            }
        }

        private void DisplaySearchField() {
            var searchRect = GUILayoutUtility.GetRect(100, 32);
            var tmpFilterString = ObjectSearchField.OnGUI(searchRect, FilterString);

            if (tmpFilterString != FilterString) {
                UpdateFilter(tmpFilterString);
                FilterString = tmpFilterString;
            }
        }

        private void UpdateFilter(string filterString) {
            FilteredObjects = FilterObjects(FoundObjects, filterString);
        }

        private void DisplayScrollListView() {
            if (FoundObjects == null) {
                return;
            }

            using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
                using (var scrollScope = new EditorGUILayout.ScrollViewScope(ListScrollViewOffset)) {
                    ListScrollViewOffset = scrollScope.scrollPosition;
                    foreach (var foundObject in new List<Asset<Object>>(FilteredObjects)) {
                        if (foundObject == null) {
                            FilteredObjects.Remove(foundObject);
                        } else {
                            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(ListViewItemHeight))) {
                                if (foundObject.Icon != null) {
                                    GUI.DrawTexture(GUILayoutUtility.GetRect(16, 16, GUILayout.Width(16)), foundObject.Icon);
                                }
                                if (GUILayout.Button(foundObject.Name, GetGUIStyle(foundObject))) {
                                    ChangeSelectedObject(foundObject);
                                }
                            }
                        }
                    }
                }
            }

            if (Event.current.type == EventType.Repaint) {
                var lastRect = GUILayoutUtility.GetLastRect();
                ScrollViewHeight = lastRect.height;
            }
        }

        private void DisplayFooter() {
            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(FooterHeight))) {
                if (GUILayout.Button(LogoTexture, EditorStyles.label)) {
                    Application.OpenURL(CollisionBearUrl);
                }

                using (new EditorGUILayout.VerticalScope(GUILayout.Height(FooterHeight))) {
                    EditorGUILayout.LabelField(EditorName);
                    EditorGUILayout.LabelField($"Version {Version}");
                }
            }
        }

        private GUIStyle GetGUIStyle(Asset<Object> o) {          
            if (SelectedObject == o) {
                return SelectedStyle;
            } else {
                return UnselectedStyle;
            }
        }

        private void DisplayRightColumn()
        {
            var previewWidth = Mathf.Max(32, PreviewWidth);
            var previewHeight = Mathf.Max(32, PreviewHeight);

            using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
                DisplaySelection(previewWidth, previewHeight);
            }
        }

        private void DisplayBottomRow() {
            using (new EditorGUILayout.VerticalScope(GUILayout.Height(32))) {
                using (new EditorGUILayout.HorizontalScope()) {
                    if (GUILayout.Button("Ok")) {
                        ApplyValue();
                        Close();
                    }

                    if (GUILayout.Button("Cancel")) {
                        Close();
                    }
                }
            }
        }

        private void HandleKeyboardInput()
        {
            if (Event.current.clickCount == 2) {
                ApplyValue();
                Close();
                return;
            }

            if (Event.current.type == EventType.KeyDown) {
                if (Event.current.keyCode == KeyCode.DownArrow) {
                    UpdateSelectedObjectIndex(SelectedObjectIndex + 1);
                    Event.current.Use();
                } else if (Event.current.keyCode == KeyCode.UpArrow) {
                    UpdateSelectedObjectIndex(SelectedObjectIndex + -1);
                    Event.current.Use();
                } else if(Event.current.keyCode == KeyCode.Return) {
                    ApplyValue();
                    Close();
                } else if(Event.current.keyCode == KeyCode.Escape) {
                    Close();
                }
            }
        }

        private void UpdateSelectedObjectIndex(int newIndex) {
            if (FilteredObjects.Count == 0) {
                return;
            }

            newIndex = Mathf.Clamp(newIndex, 0, FilteredObjects.Count - 1);
            ChangeSelectedObject(FilteredObjects[newIndex]);
        }

        private void ApplyValue()
        {
            SerializedProperty.objectReferenceValue = SelectedObject.Object as Object;
            SerializedProperty.serializedObject.ApplyModifiedProperties();
        }

        public void DisplaySelection(float previewWidth, float previewHeight)
        {
            if (SelectedObjectEditor == null) {
                return;
            }

            if(previewHeight <= 0 || previewHeight <= 0) {
                return;
            }

            SelectedObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(previewWidth, previewHeight), GUIStyle.none);
            Repaint();
        }

        public List<Asset<Object>> FindAssetsOfType(System.Type type)
        {
            if (ObjectCache.ContainsKey(type)) {
                return ObjectCache[type];
            }

            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            var result = new List<Asset<Object>>();
            if (typeof(ScriptableObject).IsAssignableFrom(type)) {
                result = FindScriptableObjectOfType(type);
            } else if(typeof(GameObject).IsAssignableFrom(type)) {
                result = FindGameObjects(type);
            } else if (typeof(Component).IsAssignableFrom(type)) {
                result = FindPrefabsWithComponentType(type);
            } else if (typeof(Object).IsAssignableFrom(type)) {
                result = FindAssetTypes(type);
            }

            ObjectCache[type] = result;
            return result;
        }

        public List<Asset<Object>> FindScriptableObjectOfType(System.Type type)
        {
            return AssetDatabase.FindAssets(string.Format("t:{0}", type))
                .Select(g => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(g)))
                .OrderBy(o => o.name)
                .Select(a => new Asset<Object>(a, a.GetInstanceID()))
                .ToList();
        }

        public List<Asset<Object>> FindAssetTypes(System.Type type)
        {
            return AssetDatabase.FindAssets(string.Format("t:{0}", type.Name))
                .Select(g => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(g)))
                .OrderBy(o => o.name)
                .Select(a => new Asset<Object>(a, a.GetInstanceID()))
                .ToList();
        }

        private List<Asset<Object>> FindGameObjects(System.Type type)
        {
            return AssetDatabase.FindAssets("t:GameObject")
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Where(p => p.EndsWith(".prefab"))
                .Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p))
                .Select(a => new Asset<Object>(a, a.GetInstanceID()))
                .ToList();
        }

        private List<Asset<Object>> FindPrefabsWithComponentType(System.Type type)
        {
            return AssetDatabase.FindAssets("t:GameObject")
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Where(p => p.EndsWith(".prefab"))
                .Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p))
                .Where(a => HasComponent(a, type))
                .Select(a => new Asset<Object>(a, a.GetComponent(type).GetInstanceID()))
                .ToList();
        }

        private bool HasComponent(GameObject gameObject, System.Type type)
        {
            return gameObject.GetComponents<Component>()
                .Where(t => type.IsInstanceOfType(t))
                .Any();
        }

        public List<Asset<Object>> FilterObjects(List<Asset<Object>> startCollection, string filter)
        {
            var result = startCollection.ToList();

            if (filter != string.Empty) {
                result = result.Where(o => o.Object.name.ToLower().Contains(filter.ToLower())).ToList();
            }

            // Make sure the null(None) Option is available at the top
            result.Insert(0, new Asset<Object>(null, 0));
            return result;
        }

        public void SetSelectedObject(long itemId)
        {
            ChangeSelectedObject(GetSelectedObject(itemId));
        }

        public Asset<Object> GetSelectedObject(long itemId)
        {
            foreach (var item in FoundObjects) {
                if (item.Id == itemId) {
                    return item;
                }
            }

            return new Asset<Object>(null, 0);
        }

        public void ChangeSelectedObject(Asset<Object> selectedObject)
        {
            if (selectedObject == SelectedObject) {
                return;
            }

            SelectedObject = selectedObject;
            var index = FilteredObjects.IndexOf(SelectedObject);
            EnsureSelectItemInView(index);

            if (SelectedObjectEditor != null) {
                GameObject.DestroyImmediate(SelectedObjectEditor);
            }

            if (SelectedObject != null) {
                SelectedObjectEditor = Editor.CreateEditor(SelectedObject.Object);
            }

            GUI.FocusControl(null);
        }

        private void EnsureSelectItemInView(int index) {
            SelectedObjectIndex = index;
            var expectedScrollPosition = GetExpectedScrollPosition(index);

            var scrollPositionTop = ListScrollViewOffset.y;
            var scrollPositionBottom = scrollPositionTop + ScrollViewHeight;

            if (expectedScrollPosition < scrollPositionTop) {
                ListScrollViewOffset.y = expectedScrollPosition;
            } else if (expectedScrollPosition > scrollPositionBottom - ListViewItemHeight) {
                var adjustment = expectedScrollPosition - (scrollPositionBottom - ListViewItemHeight);
                var misalignedRows = Mathf.CeilToInt(adjustment / ListViewItemHeight);
                ListScrollViewOffset.y += misalignedRows * ListViewItemHeight;
            }
        }

        private float GetExpectedScrollPosition(int index) => index * ListViewItemHeight;

        public void ChangeSelectedType(System.Type type)
        {
            SelectedType = type;
            FoundObjects = FindAssetsOfType(type).OrderBy(a => a.Object.name).ToList();
            ResetFilter();

            var mappedObjects = FoundObjects.ToDictionary(o => o.Id, o => o);
            SelectedObject = null;
        }

        public void ResetFilter()
        {
            FilteredObjects = FilterObjects(FoundObjects, "");
            FilterString = string.Empty;
        }
    }
}