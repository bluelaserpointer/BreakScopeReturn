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
    Transform _randomTipsContainer;
    [SerializeField]
    IzumiTools.Cooldown _randomTipsChangeCD;
    [SerializeField]
    GameObject _confirmButtonContainer;
    [SerializeField]
    Image _loadingProgressImage;
    [SerializeField]
    TextMeshProUGUI _loadingProgressText;

    static string loadSceneName;
    static bool longLoadStyle;
    private float _loadingProgress;
    private AsyncOperation _asyncOperation;
    private GameObject _displayingTips;
    public static void LoadScene(string sceneName, bool longLoadStyle)
    {
        loadSceneName = sceneName;
        LoadingScreen.longLoadStyle = longLoadStyle;
        SceneManager.LoadScene("LoadingAdditive", LoadSceneMode.Additive);
    }
    private void Start()
    {
        _randomTipsContainer.gameObject.SetActive(longLoadStyle);
        _randomTipsChangeCD.Fill();
        _loadingProgress = 0;
        _loadingProgressImage.fillAmount = 0;
        _loadingProgressText.text = "0 %";
        StartCoroutine(LoadScene_Internal(loadSceneName));
    }
    private IEnumerator LoadScene_Internal(string sceneName)
    {
        if (longLoadStyle)
            yield return new WaitForSeconds(0.5f); //Ensure Loading Screen Tips loaded before the freeze
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
        if (longLoadStyle)
        {
            if (_randomTipsChangeCD.AddDeltaTimeAndEat())
            {
                if (_displayingTips != null)
                    _displayingTips.SetActive(false);
                _displayingTips = _randomTipsContainer.GetChild((int)(Random.value * _randomTipsContainer.childCount)).gameObject;
                _displayingTips.SetActive(true);
            }
        }
        float newloadingProgress = _asyncOperation.progress;
        if (newloadingProgress == 0.9f)
        {
            newloadingProgress = 1;
            if (longLoadStyle)
            {
                if (!_confirmButtonContainer.activeSelf)
                    _confirmButtonContainer.SetActive(true);
            }
            else
            {
                _asyncOperation.allowSceneActivation = true;
            }
        }
        if (_loadingProgress < newloadingProgress)
        {
            if ((_loadingProgress += 10F + Time.deltaTime) > newloadingProgress)
                _loadingProgress = newloadingProgress;
            _loadingProgressImage.fillAmount = _loadingProgress;
            _loadingProgressText.text = string.Format("{0:P}", _loadingProgress);
        }
    }
    public void UIEventAllowLoadedSceneActivation()
    {
        _asyncOperation.allowSceneActivation = true;
    }
}
