using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TranslatedTextMeshPro : MonoBehaviour
{
    [SerializeField]
    LanguageTMPFontSO _languageFontSO;
    public TranslatableSentence sentence;
    private void UpdateText()
    {
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            if (_languageFontSO != null)
                text.font = _languageFontSO.GetCurrentLanguageFont();
            text.text = sentence.ToString();
        }
    }
    private void Awake()
    {
        UpdateText();
    }
    void OnValidate()
    {
        if (sentence == null)
            return;
        UpdateText();
    }
}
