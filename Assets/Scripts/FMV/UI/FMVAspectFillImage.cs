using UnityEngine;
using UnityEngine.UI;

namespace FMV.UI
{
    /// <summary>
    /// Makes a RawImage "aspect-fill": the texture covers the whole rect and the overflow is cropped
    /// (like CSS object-fit: cover), instead of being stretched.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class FMVAspectFillImage : MonoBehaviour
    {
        private RawImage _image;

        private RawImage Raw => _image != null ? _image : (_image = GetComponent<RawImage>());

        public void SetTexture(Texture texture)
        {
            Raw.texture = texture;
            Refresh();
        }

        public void Refresh()
        {
            Texture tex = Raw.texture;
            Rect rect = Raw.rectTransform.rect;
            if (tex == null || tex.height == 0 || rect.height <= 0f)
            {
                Raw.uvRect = new Rect(0, 0, 1, 1);
                return;
            }

            float texAspect = (float)tex.width / tex.height;
            float rectAspect = rect.width / rect.height;

            if (texAspect > rectAspect)
            {
                // Texture is wider than the rect: crop the sides.
                float w = rectAspect / texAspect;
                Raw.uvRect = new Rect((1f - w) * 0.5f, 0f, w, 1f);
            }
            else
            {
                // Texture is taller than the rect: crop top and bottom.
                float h = texAspect / rectAspect;
                Raw.uvRect = new Rect(0f, (1f - h) * 0.5f, 1f, h);
            }
        }

        private void OnEnable() => Refresh();

        private void OnRectTransformDimensionsChange()
        {
            if (isActiveAndEnabled) Refresh();
        }
    }
}
