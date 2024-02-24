using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class TitleScreen : MonoBehaviour
{
    [SerializeField]
    string _initialStageSceneName;
    [SerializeField]
    Language _buildLanguage;
    [SerializeField]
    TextMeshProUGUI _versionText;

    public static TitleScreen Instance { get; private set; }
    private static bool _hasSetBuildLanguage;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1F;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(Resources.Load<Texture2D>("Cursor/Cursor"), Vector2.zero, CursorMode.Auto);
        _versionText.text = "Version: " + Application.version.ToString();
        if (!_hasSetBuildLanguage)
        {
            _hasSetBuildLanguage = true;
            _buildLanguage.SetAsCurrentLanguage();
        }
    }
    public void StartGame()
    {
        LoadingScreen.LoadScene(_initialStageSceneName, longLoadStyle: true);
    }
    public void NextLanguage()
    {
        Setting.Set(Setting.LANGUAGE, (int)(LanguageExtension.CurrentLanguage + 1) % Enum.GetNames(typeof(Language)).Length);
    }
}
