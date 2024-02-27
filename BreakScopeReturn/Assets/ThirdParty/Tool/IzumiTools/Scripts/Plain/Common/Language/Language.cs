using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public enum Language { Chinese, English, Japanese }

public static class LanguageExtension
{
    public static Language CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                UpdateLanguageTranslatorTexts();
            }
        }
    }
    private static Language _currentLanguage = Language.Japanese;
    private static readonly List<ILanguageTranslator> _translators = new List<ILanguageTranslator>();
    public static void UpdateLanguageTranslatorTexts()
    {
        for (int i = 0; i < _translators.Count;)
        {
            ILanguageTranslator translator = _translators[i];
            if (translator == null)
            {
                _translators.RemoveAt(i);
                continue;
            }
            translator.UpdateText();
            ++i;
        }
    }
    public static void AddLanguageTranslator(ILanguageTranslator translator)
    {
        if (!_translators.Contains(translator))
            _translators.Add(translator);
    }
    public static void SetAsCurrentLanguage(this Language language)
    {
        CurrentLanguage = language;
    }
    public static bool IsCurrentLanguage(this Language language)
    {
        return CurrentLanguage == language;
    }
}
[System.Serializable]
public struct LanguageFont
{
    public Language language;
    public Font font;
}
[System.Serializable]
public struct LanguageTMPFont
{
    public Language language;
    public TMP_FontAsset font;
}