using UnityEngine;

namespace CollisionBear.PreviewObjectPicker
{
    public class PreviewFieldAttribute : PropertyAttribute
    {
        private const float DefaultSize = 128;

        public bool ShowInspectorPreview;
        public Vector2 PreviewSize;

        public PreviewFieldAttribute()
        {
            ShowInspectorPreview = true;
            PreviewSize = new Vector2(DefaultSize, DefaultSize);
        }

        public PreviewFieldAttribute(float size)
        {
            ShowInspectorPreview = true;
            PreviewSize = new Vector2(size, size);
        }
    }
}