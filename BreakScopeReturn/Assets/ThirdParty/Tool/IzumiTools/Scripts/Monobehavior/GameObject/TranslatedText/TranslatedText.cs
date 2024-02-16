using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Text))]
public class TranslatedText : MonoBehaviour, ILanguageTranslator
{
    [SerializeField]
    TranslatableSentenceSO so_sentence;
    [SerializeField]
    TranslatableSentence sentence;

    private void Awake()
    {
        LanguageExtension.AddLanguageTranslator(this);
        UpdateText();
    }
    public void SetText(TranslatableSentence sentence)
    {
        so_sentence = null;
        this.sentence = sentence;
        UpdateText();
    }
    void OnValidate()
    {
        if (so_sentence != null)
        {
            sentence = so_sentence.sentence.Clone();
        }
        else if (sentence == null)
            return;
        UpdateText();
    }
    public void UpdateText()
    {
        Text text = GetComponent<Text>();
        if (text != null)
            text.text = sentence;
    }
}
