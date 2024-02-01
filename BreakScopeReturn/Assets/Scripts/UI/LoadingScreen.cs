using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    Image _loadingProgressImage;
    [SerializeField]
    TextMeshProUGUI _loadingProgressText;

    static string loadSceneName;
    private float _loadingProgress;
    private AsyncOperation _asyncOperation;
    public static void LoadScene(string sceneName)
    {
        loadSceneName = sceneName;
        SceneManager.LoadScene("LoadingAdditive", LoadSceneMode.Additive);
    }
    private void Start()
    {
        StartCoroutine(LoadScene_Internal(loadSceneName));
    }
    private IEnumerator LoadScene_Internal(string sceneName)
    {
        _loadingProgress = 0;
        _loadingProgressImage.fillAmount = 0;
        _loadingProgressText.text = "0 %";
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
        float newloadingProgress = _asyncOperation.progress;
        if (newloadingProgress == 0.9f)
        {
            newloadingProgress = 1;
            _asyncOperation.allowSceneActivation = true;
        }
        if (_loadingProgress < newloadingProgress)
        {
            if ((_loadingProgress += 10F + Time.deltaTime) > newloadingProgress)
                _loadingProgress = newloadingProgress;
            _loadingProgressImage.fillAmount = _loadingProgress;
            _loadingProgressText.text = string.Format("{0:P}", _loadingProgress);
        }
    }
}
