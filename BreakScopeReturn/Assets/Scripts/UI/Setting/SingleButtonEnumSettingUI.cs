using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleButtonEnumSettingUI : SettingUI
{
    [Header("Option Value")]
    [SerializeField]
    string[] _enumTexts;
    [SerializeField]
    TranslatedTMP _enumValueTranslator;

    public int Index { get; private set; }
    public override void UpdateDisplay()
    {
        _enumValueTranslator.sentence.defaultString = _enumTexts[GetOptionValueInt()];
        _enumValueTranslator.UpdateText();
    }
    public void UIEventOnButtonPressed()
    {
        ApplySetting((GetOptionValueInt() + 1) % _enumTexts.Length);
        UpdateDisplay();
    }
}
