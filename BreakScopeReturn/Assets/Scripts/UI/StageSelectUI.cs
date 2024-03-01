using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectUI : MonoBehaviour
{
    [SerializeField]
    StageInfo[] _stageInfos;
    [SerializeField]
    Transform _stageButtonContainer;
    [SerializeField]
    Button _stageButtonPrefab;
    [SerializeField]
    StageInfoUI _stageInfoUI;
    private void Awake()
    {
        foreach (StageInfo info in _stageInfos)
        {
            Button stageButton = Instantiate(_stageButtonPrefab, _stageButtonContainer);
            TranslatedTMP buttonTransTMP = stageButton.GetComponentInChildren<TranslatedTMP>();
            buttonTransTMP.sentence = info.NameTS;
            buttonTransTMP.UpdateText();
            stageButton.onClick.AddListener(() => _stageInfoUI.Set(info));
        }
    }
}
