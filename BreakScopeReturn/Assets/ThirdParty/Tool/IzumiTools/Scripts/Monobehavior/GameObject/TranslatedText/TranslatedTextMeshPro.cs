using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TranslatedTextMeshPro : MonoBehaviour
{
    [SerializeField]
    LanguageTMPFontSO _languageFontSO;
    public TranslatableSentence sentence;
    public Language DisplayedLanguage {  get; private set; }
    private void UpdateText()
    {
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            if (_languageFontSO != null)
                text.font = _languageFontSO.GetCurrentLanguageFont();
            text.text = sentence.ToString();
        }
        DisplayedLanguage = LanguageExtension.currentLanguage;
    }
    private void Update()
    {
        if (!DisplayedLanguage.IsCurrentLanguage())
            UpdateText();
    }
    void OnValidate()
    {
        if (sentence == null)
            return;
        UpdateText();
    }
}
