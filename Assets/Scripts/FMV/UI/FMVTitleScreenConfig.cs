using UnityEngine;

namespace FMV.UI
{
    /// <summary>
    /// Per-game content for the title screen. Edit this asset instead of the scene to change the
    /// title, subtitle, footer or the freeze-frame shown behind the menu.
    /// </summary>
    [CreateAssetMenu(fileName = "TitleScreenConfig", menuName = "FMV/Title Screen Config")]
    public class FMVTitleScreenConfig : ScriptableObject
    {
        [Tooltip("Game title. Use a line break to control where it wraps. Shown in upper case.")]
        [TextArea(1, 3)] public string Title = "My FMV\nGame";

        public string Subtitle = "An interactive thriller";

        [Tooltip("Small print at the bottom of the screen, e.g. version and build label.")]
        public string Footer = "v0.1  ·  Prototype build";

        [Tooltip("Freeze-frame shown (aspect-fill) behind the menu. Any Texture2D works; change it here.")]
        public Texture2D FreezeFrame;

        [Tooltip("Multiplies the freeze-frame's colour. Lower is darker. 0.65 approximates the mock in a Linear colour-space project; white leaves the image untouched.")]
        public Color FreezeFrameTint = new Color(0.65f, 0.65f, 0.65f, 1f);
    }
}
