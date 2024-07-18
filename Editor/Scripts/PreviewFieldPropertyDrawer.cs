using UnityEditor;
using UnityEngine;
using System.Linq;

namespace CollisionBear.PreviewObjectPicker
{
    [CustomPropertyDrawer(typeof(PreviewFieldAttribute))]
    public class PreviewFieldPropertyDrawer : PropertyDrawer
    {
        private const float ObjectPickerButtonWidth = 24;

        private static readonly Vector2 ButtonOffset = new Vector2(-8, 0);

        private static GUIContent ObjectPickerContent = EditorGUIUtility.IconContent("d_PreMatCube");
        private static GUIContent PrefabContent = EditorGUIUtility.IconContent("Prefab Icon");

        private static Texture2D BackgroundImage = PreviewRenderingUtility.CreateTexture(256, 256, new Color(0.32f, 0.32f, 0.32f, 1f));

        private static readonly GUIStyle TinyButtonStyle = new GUIStyle(EditorStyles.miniButton) {
            margin = new RectOffset(1, 1, 1, 1),
            border = new RectOffset(),
            padding = new RectOffset()
        };

        public static readonly GUIStyle TinyIconObjectFieldStyle = new GUIStyle(EditorStyles.objectField) {
            padding = new RectOffset(3, 3, 3, 3)
        };

        public static readonly GUIStyle ClipLabelStyle = new GUIStyle(EditorStyles.label) {
            padding = new RectOffset(3, 3, 3, 3),
            clipping = TextClipping.Clip
        };

        private System.Type DisplayType;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if( DisplayType == null) {
                DisplayType = GetTypeFromString(property.type);

                if(DisplayType == null) {
                    Debug.LogWarning("Failed to read type");
                    return;
                }
            }

            position = EditorGUI.PrefixLabel(position, label);
            DefaultGUI(position, property, DisplayType);
        }

        private System.Type GetTypeFromString(string unityTypeName)
        {
            if(unityTypeName.ToLower().StartsWith("pptr")) {
                unityTypeName = GetPptrTypeName(unityTypeName);
            }

            return System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name.ToLower() == unityTypeName);
        }

        private string GetPptrTypeName(string unityTypeName) => unityTypeName.ToLower().Replace("pptr<$", "").Replace(">", "");

        private void DefaultGUI(Rect position, SerializedProperty property, System.Type type)
        {
            var previewAttribute = attribute as PreviewFieldAttribute;

            if(previewAttribute.ShowInspectorPreview) {
                PreviewPropertyDrawer(previewAttribute, position, property, type);
            } else {
                UnityPropertyDrawer(position, property, type);
            }
        }

        private void PreviewPropertyDrawer(PreviewFieldAttribute previewAttribute, Rect position, SerializedProperty property, System.Type type)
        {
            var offset = new Vector2(position.width - (previewAttribute.PreviewSize.x), 0);
            var imagePosition = new Rect(position.position + offset, previewAttribute.PreviewSize);
            var nameplatePosition = new Rect(position.position + offset, new Vector2(previewAttribute.PreviewSize.x, EditorGUIUtility.singleLineHeight));
            var selectButtonPosition = new Rect(position.position + (position.size - (new Vector2(ObjectPickerButtonWidth, ObjectPickerButtonWidth)) + ButtonOffset), new Vector2(ObjectPickerButtonWidth, ObjectPickerButtonWidth));

            GUI.DrawTexture(imagePosition, BackgroundImage);

            var previewTexture = PreviewRenderingUtility.GetPreviewTexture(GetPrefab(property.objectReferenceValue));
            if (previewTexture != null) {
                GUI.DrawTexture(imagePosition, previewTexture);
            }

            if (GUI.Button(nameplatePosition, GetPropertyValueName(property, type), ClipLabelStyle)) {
                EditorGUIUtility.PingObject(property.objectReferenceValue);
            }

            if (GUI.Button(selectButtonPosition, ObjectPickerContent, TinyButtonStyle)) {
                PreviewSelectorEditor.CreateStyles();
                PreviewSelectorEditor.ShowAuxWindow(type, property);
            }

            HandleIspectorEvent(imagePosition, property, type);
        }

        private UnityEngine.Object GetPrefab(UnityEngine.Object obj)
        { 
            if(obj is MonoBehaviour monoBehaviour) {
                return monoBehaviour.gameObject;
            } else {
                return obj;
            }
        }

        private void UnityPropertyDrawer(Rect position, SerializedProperty property, System.Type type)
        {
            var nameplatePosition = new Rect(position.position, position.size - new Vector2(ObjectPickerButtonWidth, 0));
            var selectButtonPosition = new Rect(position.position + new Vector2(nameplatePosition.width, 0), new Vector2(ObjectPickerButtonWidth, position.size.y));

            if (GUI.Button(nameplatePosition, GetPropertyValueNameAndIcon(property, type), TinyIconObjectFieldStyle)) {
                EditorGUIUtility.PingObject(property.objectReferenceValue);
            }

            if (GUI.Button(selectButtonPosition, ObjectPickerContent, TinyButtonStyle)) {
                PreviewSelectorEditor.CreateStyles();
                PreviewSelectorEditor.ShowAuxWindow(type, property);
            }

            HandleIspectorEvent(nameplatePosition, property, type);
        }

        private void HandleIspectorEvent(Rect nameplatePosition, SerializedProperty property, System.Type type)
        {
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform) {
                if (!nameplatePosition.Contains(currentEvent.mousePosition)) {
                    return;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (currentEvent.type == EventType.DragPerform) {
                    DragAndDrop.AcceptDrag();
                    var draggedObject = DragAndDrop.objectReferences[0];

                    if (type == typeof(GameObject)) {
                        property.objectReferenceValue = draggedObject;
                    } else if (typeof(Component).IsAssignableFrom(type)) {
                        var draggedComponent = (draggedObject as GameObject).GetComponent(type);
                        property.objectReferenceValue = draggedComponent;
                    } else if(typeof(Object).IsAssignableFrom(type)) {
                        property.objectReferenceValue = draggedObject;
                    }
                }
            }
        }

        private GUIContent GetPropertyValueNameAndIcon(SerializedProperty serializedProperty, System.Type type)
        {
            if (serializedProperty.objectReferenceValue == null) {
                return new GUIContent(string.Format("None ({0})", type.Name), PrefabContent.image);
            } else {
                return new GUIContent(string.Format("{0} ({1})", serializedProperty.objectReferenceValue.name, type.Name), PrefabContent.image);
            }
        }

        private GUIContent GetPropertyValueName(SerializedProperty serializedProperty, System.Type type)
        {
            if (serializedProperty.objectReferenceValue == null) {
                return new GUIContent(string.Format("None ({0})", type.Name));
            } else {
                return new GUIContent(string.Format("{0} ({1})", serializedProperty.objectReferenceValue.name, type.Name));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var previewAttribute = attribute as PreviewFieldAttribute;

            if(previewAttribute.ShowInspectorPreview) {
                return previewAttribute.PreviewSize.y;
            } else {
                return EditorGUIUtility.singleLineHeight;
            }
        }
    }
}