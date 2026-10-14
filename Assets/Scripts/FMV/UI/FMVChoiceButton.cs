using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FMV.UI
{
    /// <summary>
    /// One choice button on the choice screen. Shows a chevron while hovered or selected (keyboard or
    /// gamepad focus) and sizes its label: one line at full size, two lines when it wraps, and for very
    /// long text shrinks the font (auto-size) and then truncates with an ellipsis instead of growing.
    /// The button itself grows with the label through the Vertical Layout Group on this object.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class FMVChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Tooltip("Choice text. Auto-size, wrapping and Ellipsis overflow are set up on it.")]
        public TMP_Text Label;

        [Tooltip("Icon shown at the right end while the button is hovered or selected.")]
        public Graphic Chevron;

        [Tooltip("Layout Element on the label. Its preferred height is capped to MaxLines so the button cannot grow without limit.")]
        public LayoutElement LabelLayout;

        [Tooltip("Most lines the button grows to before the label shrinks and then truncates.")]
        public int MaxLines = 2;

        [Tooltip("Width used for measuring until the layout has given the button its real width.")]
        public float FallbackWidth = 740f;

        private Button _button;
        private bool _hovered;
        private bool _selected;

        private void Awake() => _button = GetComponent<Button>();

        private void OnEnable() => UpdateChevron();

        public void SetLabel(string text)
        {
            if (Label == null) return;
            Label.text = text;
            Refit();
        }

        /// <summary>Re-measures the label. Call again after the layout has run if the button width changed.</summary>
        public void Refit()
        {
            if (Label == null || LabelLayout == null) return;

            var rt = (RectTransform)transform;
            float buttonWidth = rt.rect.width > 1f ? rt.rect.width : FallbackWidth;
            var group = GetComponent<LayoutGroup>();
            float labelWidth = buttonWidth - (group != null ? group.padding.horizontal : 0);

            // Measure at the largest font size, then let auto-size shrink the text inside the capped height.
            Label.enableAutoSizing = false;
            Label.fontSize = Label.fontSizeMax;
            float oneLine = Label.GetPreferredValues("Ag", 100000f, 0f).y;
            float twoLines = Label.GetPreferredValues("Ag\nAg", 100000f, 0f).y;
            float wanted = Label.GetPreferredValues(Label.text, labelWidth, 0f).y;
            Label.enableAutoSizing = true;

            // Count lines by the line pitch (which includes the label's line spacing) and give each its full pitch,
            // so the first line is not shorter than the rest.
            float pitch = Mathf.Max(1f, twoLines - oneLine);
            int lines = Mathf.Max(1, Mathf.RoundToInt((wanted - oneLine) / pitch) + 1);
            LabelLayout.preferredHeight = Mathf.Min(lines, MaxLines) * pitch;
        }

        public void OnPointerEnter(PointerEventData eventData) { _hovered = true; UpdateChevron(); }
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; UpdateChevron(); }
        public void OnSelect(BaseEventData eventData) { _selected = true; UpdateChevron(); }
        public void OnDeselect(BaseEventData eventData) { _selected = false; UpdateChevron(); }

        private void UpdateChevron()
        {
            if (Chevron == null) return;
            if (_button == null) _button = GetComponent<Button>();
            Chevron.enabled = (_hovered || _selected) && _button.interactable;
        }
    }
}
