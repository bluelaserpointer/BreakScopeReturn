using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PauseUI : MonoBehaviour
{
    [SerializeField]
    GameObject _pauseMenu;
    [SerializeField]
    GameObject _settingScreen;
    public bool Paused { get; private set; }
    Transform graphicRoot;
    private Player Player => GameManager.Instance.Player;
    private void Awake()
    {
        graphicRoot = transform.GetChild(0);
        graphicRoot.gameObject.SetActive(false);
        GameManager.DoAfterInit(UpdateSettingUIDisplay);
    }
    private void Update()
    {
        bool pauseInput;
#if UNITY_EDITOR
        pauseInput = Input.GetKeyDown(KeyCode.F1);
#else
        pauseInput = Input.GetKeyDown(KeyCode.Escape);
#endif
        if (pauseInput)
        {
            OnPressEscapeButton();
        }
    }
    public void OnPressEscapeButton()
    {
        if (!Paused)
        {
            SetPause(true);
        }
        else if (_pauseMenu.activeSelf)
        {
            SetPause(false);
        }
        else if (_settingScreen.activeSelf)
        {
            _settingScreen.SetActive(false);
            _pauseMenu.SetActive(true);
        }
    }
    public void SetPause(bool cond)
    {
        Paused = cond;
        Time.timeScale = Paused ? 0F : 1F;
        graphicRoot.gameObject.SetActive(Paused);
        Player.AIEnableUpdate();
        GameManager.Instance.Stage.NpcUnits.ForEach(unit => unit.AIEnableUpdate());
    }
    public void UpdateSettingUIDisplay()
    {
        foreach (SettingUI settingUI in GetComponentsInChildren<SettingUI>(includeInactive: true))
        {
            settingUI.UpdateDisplay();
        }
    }
    public void UIEventBackToTitle()
    {
        LoadingScreen.LoadScene("Title", longLoadStyle: false);
    }
    public void UIEventResetSetting()
    {
        Setting.SetDefault();
        UpdateSettingUIDisplay();
    }
}
