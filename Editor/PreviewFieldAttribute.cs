using System;
using UnityEngine;

namespace Fyrvall.PreviewObjectPicker
{
    public class PreviewFieldAttribute : PropertyAttribute
    {
        public Type PreviewType;
        public PreviewFieldAttribute(Type previewType)
        {
            PreviewType = previewType;
        }
    }
}