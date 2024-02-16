using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TitleScreen : MonoBehaviour
{
    [SerializeField]
    string _initialStageSceneName;
    public static TitleScreen Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1F;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(Resources.Load<Texture2D>("Cursor/Cursor"), Vector2.zero, CursorMode.Auto);
    }
    public void StartGame()
    {
        LoadingScreen.LoadScene(_initialStageSceneName);
    }
    public void NextLanguage()
    {
        Setting.Set(Setting.LANGUAGE, (int)(LanguageExtension.CurrentLanguage + 1) % Enum.GetNames(typeof(Language)).Length);
    }
}
