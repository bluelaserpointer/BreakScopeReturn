using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StageInfoUI : MonoBehaviour
{
    [SerializeField]
    TranslatedTMP _nameTransTMP;
    [SerializeField]
    TranslatedTMP _descriptionTransTMP;
    [SerializeField]
    Button _enterStageButton;

    public StageInfo StageInfo {  get; private set; }
    private void OnEnable()
    {
        UpdateDisplay();
    }
    public void UpdateDisplay()
    {
        if (StageInfo == null)
        {
            _enterStageButton.gameObject.SetActive(false);
            _nameTransTMP.sentence = _descriptionTransTMP.sentence = new TranslatableSentence() { defaultString = "" };
        }
        else
        {
            _enterStageButton.gameObject.SetActive(true);
            _nameTransTMP.sentence = StageInfo.NameTS;
            _descriptionTransTMP.sentence = StageInfo.DescriptionTS;
        }
        _nameTransTMP.UpdateText();
        _descriptionTransTMP.UpdateText();
    }
    public void Set(StageInfo stageInfo)
    {
        StageInfo = stageInfo;
        UpdateDisplay();
    }
    public void UIEventOnPlayStage()
    {
        if (StageInfo == null)
            return;
        LoadingScreen.LoadScene(StageInfo.SceneName, longLoadStyle: true);
    }
}
