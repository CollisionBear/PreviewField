using System;
using System.Linq;

namespace CollisionBear.PreviewObjectPicker
{
    public static class ObjectExtension
    {
        public static bool In<T>(this T item, params T[] items)
        {
            if (items == null) {
                throw new ArgumentNullException();
            }

            return items.Contains(item);
        }
    }
}