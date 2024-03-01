using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SettingUI : MonoBehaviour
{
    private void OnEnable()
    {
        UpdateSettingUIDisplay();
    }
    public void UpdateSettingUIDisplay()
    {
        foreach (SettingItemUI settingUI in GetComponentsInChildren<SettingItemUI>(includeInactive: true))
        {
            settingUI.UpdateDisplay();
        }
    }
    public void UIEventResetSetting()
    {
        Setting.SetDefault();
        UpdateSettingUIDisplay();
    }
}
