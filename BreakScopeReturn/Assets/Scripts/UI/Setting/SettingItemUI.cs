using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class SettingItemUI : MonoBehaviour
{
    [Header("Option Name")]
    [SerializeField]
    string _playerPrefsKey;
    [SerializeField]
    TranslatableSentenceSO _optionNameTS;
    [SerializeField]
    TranslatedTMP _optionNameTranslator;

    public string PlayerPrefsKey => _playerPrefsKey;

    protected void ApplySetting(object value)
    {
        Setting.Set(PlayerPrefsKey, value);
    }
    public abstract void UpdateDisplay();
    private void OnValidate()
    {
        if (_optionNameTS == null || _optionNameTranslator == null)
            return;
        _optionNameTranslator.sentenceSO = _optionNameTS;
        _optionNameTranslator.OnValidate();
    }
    protected string GetOptionValueString() => PlayerPrefs.GetString(PlayerPrefsKey);
    protected int GetOptionValueInt() => PlayerPrefs.GetInt(PlayerPrefsKey);
    protected float GetOptionValueFloat() => PlayerPrefs.GetFloat(PlayerPrefsKey);
}
