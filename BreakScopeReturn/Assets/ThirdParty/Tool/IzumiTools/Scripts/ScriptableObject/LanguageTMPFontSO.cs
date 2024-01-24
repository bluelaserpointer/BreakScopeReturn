using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "IzumiTools/" + nameof(LanguageTMPFontSO))]
public class LanguageTMPFontSO : ScriptableObject
{
    public TMP_FontAsset fallbackFont;
    public List<LanguageTMPFont> _languageFonts;

    public TMP_FontAsset GetFont(Language language)
    {
        foreach (var pair in _languageFonts)
        {
            if (pair.language.Equals(language))
                return pair.font;
        }
        return fallbackFont;
    }
    public TMP_FontAsset GetCurrentLanguageFont()
    {
        return GetFont(LanguageExtension.currentLanguage);
    }
}
