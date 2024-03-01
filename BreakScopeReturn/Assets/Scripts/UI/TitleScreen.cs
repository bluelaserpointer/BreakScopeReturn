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
    TextMeshProUGUI _versionText;

    public static TitleScreen Instance { get; private set; }
    private static bool _hasSetSystemLanguage;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1F;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(Resources.Load<Texture2D>("Cursor/Cursor"), Vector2.zero, CursorMode.Auto);
        _versionText.text = "Version: " + Application.version.ToString();
        if (!_hasSetSystemLanguage)
        {
            _hasSetSystemLanguage = true;
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English:
                    Language.English.SetAsCurrentLanguage();
                    break;
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    Language.Chinese.SetAsCurrentLanguage();
                    break;
                case SystemLanguage.Japanese:
                    Language.Japanese.SetAsCurrentLanguage();
                    break;
                default:
                    Language.English.SetAsCurrentLanguage();
                    break;
            }
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
