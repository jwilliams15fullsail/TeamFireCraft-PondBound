using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Colours a text field's outline: dark blue normally, lily green while you type in it, red after
// an error (until you change the text). Optionally shows a "6 / 12" letter counter.
// Put it on the same object as the TMP_InputField.
[RequireComponent(typeof(TMP_InputField))]
public class FieldHighlight : MonoBehaviour
{
    [Tooltip("The outline image around the field.")]
    [SerializeField] Graphic outline;
    [Tooltip("Optional: shows letters typed / Character Limit.")]
    [SerializeField] TMP_Text counter;
    [Tooltip("Optional: a message under the field, shown only while there's an error.")]
    [SerializeField] GameObject errorMessage;
    [SerializeField] Color normalColor = new Color32(0x0A, 0x3D, 0x7A, 0xFF);
    [SerializeField] Color focusedColor = new Color32(0x5D, 0xB2, 0x2A, 0xFF);
    [SerializeField] Color errorColor = new Color32(0xE8, 0x45, 0x2C, 0xFF);

    TMP_InputField field;
    bool focused, error;

    void Awake()
    {
        field = GetComponent<TMP_InputField>();
        field.onSelect.AddListener(_ => { focused = true; Refresh(); });
        field.onDeselect.AddListener(_ => { focused = false; Refresh(); });
        field.onValueChanged.AddListener(_ => { error = false; Refresh(); });
    }

    void Start() => Refresh();

    // PlayMenu calls this when the code didn't work. Typing clears it again.
    public void SetError(bool value)
    {
        error = value;
        Refresh();
    }

    void Refresh()
    {
        if (outline != null)
            outline.color = error ? errorColor : focused ? focusedColor : normalColor;
        if (errorMessage != null)
            errorMessage.SetActive(error);
        if (counter != null && field.characterLimit > 0)
            counter.text = $"{field.text.Length} / {field.characterLimit}";
    }
}