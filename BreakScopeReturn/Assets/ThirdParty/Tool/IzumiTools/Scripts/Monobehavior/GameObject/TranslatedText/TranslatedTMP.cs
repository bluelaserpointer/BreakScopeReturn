using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TranslatedTMP : MonoBehaviour, ILanguageTranslator
{
    [Header("Font")]
    public LanguageTMPFontSO fontSO;
    
    [Header("Text")]
    public TranslatableSentenceSO sentenceSO;
    public string prefix, suffix;
    public TranslatableSentence sentence;
    public Language DisplayedLanguage {  get; private set; }
    public TextMeshProUGUI Text { get; private set; }
    private void Awake()
    {
        LanguageExtension.AddLanguageTranslator(this);
        Text = GetComponent<TextMeshProUGUI>();
        UpdateText();
    }
    public void UpdateText()
    {
        if (fontSO != null)
        {
            Text.font = fontSO.GetCurrentLanguageFont();
        }
        Text.text = prefix + sentence.ToString() + suffix;
        DisplayedLanguage = LanguageExtension.CurrentLanguage;
    }
    public void OnValidate()
    {
        if (sentenceSO != null)
        {
            sentence = sentenceSO.sentence.Clone();
        }
        else
        {
            //for fixed text with current language font
            sentence.languageAndSentences = new TranslatableSentence.LanguageAndSentence[0];
        }
        Text = GetComponent<TextMeshProUGUI>();
        UpdateText();
    }
}
