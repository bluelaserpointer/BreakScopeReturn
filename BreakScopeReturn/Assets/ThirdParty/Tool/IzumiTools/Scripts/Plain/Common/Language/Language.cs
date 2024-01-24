using UnityEngine;
using TMPro;

public enum Language { Chinese, English, Japanese }

public static class LanguageExtension
{
    public static Language currentLanguage = Language.Japanese;
    public static void SetAsCurrentLanguage(this Language language)
    {
        currentLanguage = language;
    }
    public static bool IsCurrentLanguage(this Language language)
    {
        return currentLanguage == language;
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