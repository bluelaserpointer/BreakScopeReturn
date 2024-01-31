using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TitleScreen : MonoBehaviour
{
    [SerializeField]
    string _initialStageSceneName;
    [SerializeField]
    GameObject _loadingWindow;
    [SerializeField]
    Image _loadingProgressImage;
    public static TitleScreen Instance { get; private set; }

    private AsyncOperation _asyncOperation;
    private void Awake()
    {
        Instance = this;
    }
    public void NextLanguage()
    {
        LanguageExtension.currentLanguage = (Language)((int)(LanguageExtension.currentLanguage + 1) % Enum.GetNames(typeof(Language)).Length);
    }
    public void StartGame()
    {
        //StartCoroutine(LoadScene(_initialStageSceneName));
        LoadingScreen.LoadScene(_initialStageSceneName);
    }
    IEnumerator LoadScene(string sceneName)
    {
        _loadingProgressImage.fillAmount = 0;
        _asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        _asyncOperation.allowSceneActivation = false;
        yield return _asyncOperation;
    }
    private void Update()
    {
        if (_asyncOperation == null)
        {
            return;
        }
        float loadingProgress = _asyncOperation.progress;
        if (loadingProgress > 0.9f)
        {
            loadingProgress = 1;
            _asyncOperation.allowSceneActivation = true;
        }
        if (_loadingProgressImage.fillAmount < loadingProgress)
        {
            _loadingProgressImage.fillAmount += 10F + Time.deltaTime;
            if (_loadingProgressImage.fillAmount > 1)
                _loadingProgressImage.fillAmount = 1;
        }
    }
}
