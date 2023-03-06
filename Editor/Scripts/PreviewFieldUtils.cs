using UnityEditor;
using UnityEngine;

namespace CollisionBear.PreviewObjectPicker
{
    public static class PreviewFieldUtils
    {
        public static float TotalHeight(this GUIStyle guiStyle)
        {
            return guiStyle.lineHeight + guiStyle.margin.bottom + guiStyle.margin.top;
        }

    }
}
