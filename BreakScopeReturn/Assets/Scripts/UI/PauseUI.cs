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
    SettingUI _settingScreen;

    public bool Paused { get; private set; }
    
    Transform _graphicRoot;
    private Player Player => GameManager.Instance.Player;
    private void Awake()
    {
        _graphicRoot = transform.GetChild(0);
        _graphicRoot.gameObject.SetActive(false);
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
        else if (_settingScreen.gameObject.activeSelf)
        {
            _settingScreen.gameObject.SetActive(false);
            _pauseMenu.SetActive(true);
        }
    }
    public void SetPause(bool cond)
    {
        Paused = cond;
        Time.timeScale = Paused ? 0F : 1F;
        _graphicRoot.gameObject.SetActive(Paused);
        Player.AIEnableUpdate();
        GameManager.Instance.Stage.NpcUnits.ForEach(unit => unit.AIEnableUpdate());
    }
    public void UIEventBackToTitle()
    {
        LoadingScreen.LoadScene("Title", longLoadStyle: false);
    }
}
