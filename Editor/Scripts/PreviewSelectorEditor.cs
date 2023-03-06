using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CollisionBear.PreviewObjectPicker
{
    public class PreviewSelectorEditor : EditorWindow
    {
        const int DefaultListViewWidth = 250;
        const int DefaultBottomRow = 38;

        private static readonly Vector2 MinWindowSize = new Vector2(400, 400);
        static readonly Vector2 SearchBoxSize = new Vector2(100, 32);
        static Dictionary<System.Type, List<Asset<Object>>> ObjectCache = new Dictionary<System.Type, List<Asset<Object>>>();

        public static GUIStyle SelectedStyle;
        public static GUIStyle UnselectedStyle;

        public static void CreateStyles()
        {
            SelectedStyle = new GUIStyle(GUI.skin.label);
            SelectedStyle.normal.textColor = Color.white;
            SelectedStyle.normal.background = CreateTexture(300, 20, new Color(0.24f, 0.48f, 0.9f));

            UnselectedStyle = new GUIStyle(GUI.skin.label);

        }
        private static Texture2D CreateTexture(int width, int height, Color color)
        {
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(Enumerable.Repeat(color, width * height).ToArray());
            result.Apply();

            return result;
        }
        public class Asset<T> where T : Object
        {
            public T Object;
            public long Id;
            public GUIContent Content { get; set; }

            public Asset(T o, long id)
            {
                Object = o;
                Id = id;
                Content = new GUIContent(GetObjectName(o), AssetPreview.GetMiniThumbnail(Object));
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

        private Vector2 ListScrollViewOffset;

        private SearchField ObjectSearchField;

        private float PreviewWidth;
        private float PreviewHeight;

        private Vector2 LastWindowSize;

        private void OnEnable()
        {
            ObjectSearchField = new SearchField();
            ObjectSearchField.SetFocus();
        }

        private void OnGUI()
        {
            if (HasResizedWindow()) {
                UpdateWindowHeight();
            }

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

        private bool HasResizedWindow() => LastWindowSize != position.size;

        private void UpdateWindowHeight()
        {
            PreviewWidth = position.width - (DefaultListViewWidth + (GUI.skin.box.margin.left + GUI.skin.box.margin.right) * 2);
            PreviewHeight = position.height - (DefaultBottomRow);

            LastWindowSize = position.size;

            EnsureItemIsInView(SelectedObject);
        }

        private void DrawLayout()
        {
            using (new EditorGUILayout.VerticalScope()) {
                using (new EditorGUILayout.VerticalScope(GUILayout.Height(PreviewHeight))) {
                    using (new EditorGUILayout.HorizontalScope()) {
                        DisplayLeftColumn();
                        DisplayRightColumn(PreviewWidth, PreviewHeight);
                    }
                }
                DisplayBottomRow();
            }
        }

        private void DisplayLeftColumn()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box, GUILayout.Width(DefaultListViewWidth))) {
                DisplayObjectList();
                EditorGUILayout.Space();
                if (GUILayout.Button("Refresh", EditorStyles.miniButton)) {

                    if (ObjectCache.ContainsKey(SelectedType)) {
                        ObjectCache.Remove(SelectedType);
                    }

                    FoundObjects = FindAssetsOfType(SelectedType).OrderBy(a => a.Object.name).ToList();
                    ResetFilter();
                    SetSelectedObject(SerializedProperty.objectReferenceInstanceIDValue);
                }
            }
        }

        private void DisplayRightColumn(float previewWidth, float previewHeight)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box, GUILayout.Width(previewWidth), GUILayout.Height(previewHeight))) {
                DisplaySelection(previewWidth, previewHeight);
            }
        }

        private void DisplayBottomRow()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Height(DefaultBottomRow))) {
                OnDisplayBottomRow();
            }
        }

        protected virtual void OnDisplayBottomRow()
        {
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

        private void HandleKeyboardInput()
        {
            if (Event.current.clickCount == 2) {
                ApplyValue();
                Close();
                return;
            }

            if (Event.current.type == EventType.KeyDown) {
                if (Event.current.keyCode == KeyCode.DownArrow) {
                    UpdateSelectedObjectIndex(delta: 1);
                    Event.current.Use();
                    return;
                } else if (Event.current.keyCode == KeyCode.UpArrow) {
                    UpdateSelectedObjectIndex(delta: -1);
                    Event.current.Use();
                    return;
                } else if (Event.current.keyCode == KeyCode.Return) {
                    Event.current.Use();
                    ApplyValue();
                    Close();
                    return;
                }
            }
        }

        private void UpdateSelectedObjectIndex(int delta)
        {
            if (FilteredObjects.Count == 0) {
                return;
            }

            var currentIndex = Mathf.Max(FilteredObjects.IndexOf(SelectedObject), 0);
            currentIndex = Mathf.Clamp(currentIndex + delta, 0, FilteredObjects.Count - 1);
            ChangeSelectedObject(FilteredObjects[currentIndex]);
        }

        private void ApplyValue()
        {
            SerializedProperty.objectReferenceValue = SelectedObject.Object;
            SerializedProperty.serializedObject.ApplyModifiedProperties();

        }

        public void DisplayObjectList()
        {       
            EditorGUILayout.LabelField("Found " + FilteredObjects.Count());
            DisplaySearchField();

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(ListScrollViewOffset)) {
                ListScrollViewOffset = scrollScope.scrollPosition;

                var itemWidth = DefaultListViewWidth - 24;
                foreach (var foundObject in FilteredObjects) {
                    using (new EditorGUILayout.HorizontalScope()) {
                        //if (GUILayout.Button(foundObject.Name, GetGUIStyle(foundObject), GUILayout.Height(EditorGUIUtility.singleLineHeight))) {
                        if (GUILayout.Button(foundObject.Content, GetGUIStyle(foundObject), GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(itemWidth))) {
                                ChangeSelectedObject(foundObject);
                        }
                    }
                }
            }
        }

        public void DisplaySearchField()
        {
            var searchRect = GUILayoutUtility.GetRect(SearchBoxSize.x, SearchBoxSize.y);
            var tmpFilterString = ObjectSearchField.OnGUI(searchRect, FilterString);

            if (tmpFilterString != FilterString) {
                UpdateFilter(tmpFilterString);
                FilterString = tmpFilterString;
            }
        }

        public void UpdateFilter(string filterString)
        {
            FilteredObjects = FilterObjects(FoundObjects, filterString);
        }

        public GUIStyle GetGUIStyle(Asset<Object> o)
        {
            if (SelectedObject == o || SelectedObject.Object == o.Object) {
                return SelectedStyle;
            } else {
                return UnselectedStyle;
            }
        }

        public void DisplaySelection(float previewWidth, float previewHeight)
        {
            if (SelectedObjectEditor == null) {
                return;
            }

            SelectedObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(previewWidth, previewHeight - 8), GUIStyle.none);
            Repaint();
        }

        public float GetLabelWidth(float totalWidth, float minWidth)
        {
            if (totalWidth <= minWidth) {
                return totalWidth;
            }

            return Mathf.Min(minWidth, totalWidth);
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
                result = FindAudioClips();
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

        public List<Asset<Object>> FindAudioClips()
        {
            return AssetDatabase.FindAssets(string.Format("t:AudioClip"))
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
            EnsureItemIsInView(selectedObject);

            if (SelectedObjectEditor != null) {
                GameObject.DestroyImmediate(SelectedObjectEditor);
            }

            if (SelectedObject != null) {
                SelectedObjectEditor = Editor.CreateEditor(SelectedObject.Object);
            }

            GUI.FocusControl(null);
        }

        public void EnsureItemIsInView(Asset<Object> selectedObject)
        {
            var currentIndex = FilteredObjects.IndexOf(selectedObject);

            var objectHeight = SelectedStyle.TotalHeight();
            var selectedObjectPosition = currentIndex * objectHeight;

            var controllerHeight = EditorStyles.label.TotalHeight() + SearchBoxSize.y + 40 + EditorStyles.miniButton.TotalHeight();
            var totalHeight = PreviewHeight - controllerHeight;

            var minValue = ListScrollViewOffset.y;
            var listHeight = RoundToObjectHeight(totalHeight, objectHeight) - objectHeight;
            var maxValue = minValue + listHeight;

            if (!IsInView(selectedObjectPosition, minValue, maxValue)) {
                if (selectedObjectPosition < minValue) {
                    ListScrollViewOffset = new Vector2(0, selectedObjectPosition);
                } else if (selectedObjectPosition > maxValue) {
                    ListScrollViewOffset = new Vector2(0, selectedObjectPosition - (listHeight + EditorStyles.label.TotalHeight()));
                }
            }
        }


        private float RoundToObjectHeight(float viewportHeight, float objectHeight)
        {
            var viewportIndex = Mathf.Floor(viewportHeight / objectHeight);
            return viewportIndex * objectHeight;
        }

        public bool IsInView(float selectedObjectPosition, float minValue, float maxValue) => selectedObjectPosition >= minValue && selectedObjectPosition <= maxValue;

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