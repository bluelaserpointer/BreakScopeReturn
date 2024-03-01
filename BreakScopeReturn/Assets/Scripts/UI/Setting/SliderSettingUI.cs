using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SliderSettingUI : SettingItemUI
{
    [Header("Option Value")]
    [SerializeField]
    float _minValue = 0;
    [SerializeField]
    float _maxValue = 1;
    [SerializeField]
    string _stringFormatCode = "F1";
    [SerializeField]
    Slider _slider;
    [SerializeField]
    TMP_InputField _inputField;

    public override void UpdateDisplay()
    {
        float value = GetOptionValueFloat();
        _slider.value = Mathf.Clamp01((value - _minValue) / (_maxValue - _minValue));
        _inputField.text = string.Format("{0:" + _stringFormatCode + "}", value);
    }
    public void UIEventOnSliderValueChange(float value)
    {
        ApplySetting(_minValue + (_maxValue - _minValue) * value);
        UpdateDisplay();
    }
}
