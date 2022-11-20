using System;
using UnityEditor;
using UnityEngine;

namespace Fyrvall.PreviewObjectPicker
{
    [CustomPropertyDrawer(typeof(PreviewFieldAttribute))]
    public class PreviewFieldPropertyDrawer : PropertyDrawer
    {
        private const int AudioPlayBackButtonWidth = 24;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var expandableAttribute = (PreviewFieldAttribute)attribute;
            var type = expandableAttribute.PreviewType;
            position = EditorGUI.PrefixLabel(position, label);

            if (type == typeof(AudioClip)) {
                AudioClipGUI(position, property);
            } else {
                DefaultGUI(position, property, type);
            }
        }

        private void DefaultGUI(Rect position, SerializedProperty property, System.Type type)
        {
            if (GUI.Button(position, GetPropertyValueName(property, type), GUI.skin.textField)) {
                PreviewSelectorEditor.ShowAuxWindow(type, property);
            }

            var currentEvent = Event.current;
            if (currentEvent.type.In(EventType.DragUpdated, EventType.DragPerform)) {
                if (!position.Contains(currentEvent.mousePosition)) {
                    return;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (currentEvent.type == EventType.DragPerform) {
                    DragAndDrop.AcceptDrag();
                    var draggedObject = DragAndDrop.objectReferences[0];
                    var draggedComponent = (draggedObject as GameObject).GetComponent(type);

                    property.objectReferenceValue = draggedComponent;
                }
            }
        }

        private GUIContent GetPropertyValueName(SerializedProperty serializedProperty, Type type)
        {
            if (serializedProperty.objectReferenceValue == null) {
                return new GUIContent(string.Format("None ({0})", type.Name));
            } else {
                return new GUIContent(string.Format("{0} ({1})", serializedProperty.objectReferenceValue.name, type.Name));
            }
        }

        private void AudioClipGUI(Rect position, SerializedProperty property)
        {
            var objectFieldRect = new Rect(position.position, new Vector2(position.width - AudioPlayBackButtonWidth, position.height));
            DefaultGUI(objectFieldRect, property, typeof(AudioClip));

            var previewButtonRect = new Rect(new Vector2(position.x + (position.width - AudioPlayBackButtonWidth), position.y), new Vector2(AudioPlayBackButtonWidth, position.height));
            if (GUI.Button(previewButtonRect, "P")) {
                var audioClip = property.objectReferenceValue as AudioClip;
                if (audioClip == null) {
                    return;
                }

                TryPlayClip(audioClip);
            }
        }

        private static void TryPlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            var unityEditorAssembly = typeof(AudioImporter).Assembly;
            var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            if (AudioUtilPlayClip(audioUtilClass, clip, startSample, loop)) {
                return;
            } else if (AudioUtilPlayPreviewClip(audioUtilClass, clip, startSample, loop)) {
                return;
            }

        }

        private static bool AudioUtilPlayClip(System.Type audioUtilClass, AudioClip clip, int startSample, bool loop)
        {
            var method = audioUtilClass.GetMethod(
                "PlayClip",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                null,
                new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null
            );

            if (method == null) {
                return false;
            } else {
                method.Invoke(
                    null,
                    new object[] { clip, startSample, loop }
                );
                return true;
            }
        }

        private static bool AudioUtilPlayPreviewClip(System.Type audioUtilClass, AudioClip clip, int startSample = 0, bool loop = false)
        {
            var method = audioUtilClass.GetMethod(
                "PlayPreviewClip",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                null,
                new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null
            );

            if (method == null) {
                return false;
            } else {
                method.Invoke(
                    null,
                    new object[] { clip, startSample, loop }
                );
                return true;
            }
        }
    }
}