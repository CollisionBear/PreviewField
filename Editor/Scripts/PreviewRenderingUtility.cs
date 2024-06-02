using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CollisionBear.PreviewObjectPicker
{
    public class PreviewRenderingUtility
    {
        public static Texture2D GetPreviewTexture(Object asset)
        {
            if (asset == null) {
                return AssetPreview.GetMiniThumbnail(asset);
            }

            var assetInstanceId = asset.GetInstanceID();
            if (AssetPreview.IsLoadingAssetPreview(assetInstanceId)) {
                return null;
            }

            var result = AssetPreview.GetAssetPreview(asset);
            if (result != null) {
                return result;
            } else {
                return AssetPreview.GetMiniThumbnail(asset);
            }
        }

        public static Texture2D CreateTexture(int width, int height, Color color)
        {
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(Enumerable.Repeat(color, width * height).ToArray());
            result.Apply();

            return result;
        }
    }
}