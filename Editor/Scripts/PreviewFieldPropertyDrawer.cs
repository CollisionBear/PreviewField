using System;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Fyrvall.PreviewObjectPicker
{
    [CustomPropertyDrawer(typeof(PreviewFieldAttribute))]
    public class PreviewFieldPropertyDrawer : PropertyDrawer
    {
        private Type DisplayType;

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

        private Type GetTypeFromString(string unityTypeName)
        {
            if(unityTypeName.ToLower().StartsWith("pptr")) {
                unityTypeName = GetPptrTypeName(unityTypeName);
            }

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name.ToLower() == unityTypeName);
        }

        private string GetPptrTypeName(string unityTypeName) => unityTypeName.ToLower().Replace("pptr<$", "").Replace(">", "");

        private void DefaultGUI(Rect position, SerializedProperty property, System.Type type)
        {
            var namePlatePosition = new Rect(position.position, position.size - new Vector2(24, 0));
            var selectButtonPosition = new Rect(position.position + new Vector2(namePlatePosition.width, 0), new Vector2(24, position.size.y));

            if (GUI.Button(namePlatePosition, GetPropertyValueName(property, type), EditorStyles.objectField)) {
                EditorGUIUtility.PingObject(property.objectReferenceValue);
            }

            if (GUI.Button(selectButtonPosition, EditorGUIUtility.FindTexture("PrefabNormal Icon"), EditorStyles.miniButton)) {
                PreviewSelectorEditor.ShowAuxWindow(type, property);
            }

            var currentEvent = Event.current;
            if (currentEvent.type.In(EventType.DragUpdated, EventType.DragPerform)) {
                if (!namePlatePosition.Contains(currentEvent.mousePosition)) {
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
                    }
                }
            }
        }

        private GUIContent GetPropertyValueName(SerializedProperty serializedProperty, Type type)
        {
            if (serializedProperty.objectReferenceValue == null) {
                return new GUIContent(string.Format("None ({0})", type.Name), EditorGUIUtility.FindTexture("PrefabNormal Icon"));
            } else {
                return new GUIContent(string.Format("{0} ({1})", serializedProperty.objectReferenceValue.name, type.Name), EditorGUIUtility.FindTexture("PrefabNormal Icon"));
            }
        }
    }
}