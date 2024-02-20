using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    TextMeshProUGUI _dialogTextUI;
    [SerializeField]
    Image _dialogBackground;

    [Header("Fonts")]
    [SerializeField]
    LanguageTMPFontSO _languageFontSO;

    [Header("Display Time")]
    public float displayTimePerWord = 0.5F;
    public float extraDisplayTimePerSentense = 1F;

    public DialogNodeSet CurrentNodeSet { get; private set; }
    float currentNodeDisplayedTime;

    private void OnEnable()
    {
        _dialogTextUI.font = _languageFontSO.GetCurrentLanguageFont();
    }
    public void SetDialog(DialogNodeSet nodeSet)
    {
        if (CurrentNodeSet != null)
        {
            CurrentNodeSet.gameObject.SetActive(false);
        }
        CurrentNodeSet = nodeSet;
        if (nodeSet == null)
        {
            return; 
        }
        CurrentNodeSet.Init();
        CurrentNodeSet.gameObject.SetActive(true);
        CurrentNodeSet.SetFirstNode();
        currentNodeDisplayedTime = 0;
    }
    public void SetText(string text)
    {
        if (text.Length > 0)
        {
            _dialogBackground.gameObject.SetActive(true);
            _dialogTextUI.text = text;
        }
        else
        {
            RemoveText();
        }
    }
    public void RemoveText()
    {
        if (_dialogBackground)
            _dialogBackground.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (CurrentNodeSet != null)
        {
            currentNodeDisplayedTime += Time.deltaTime;
            float waitTime = CurrentNodeSet.CurrentNodeDisplayTime;
            if (waitTime > 0)
                waitTime += extraDisplayTimePerSentense;
            if (currentNodeDisplayedTime > waitTime)
            {
                currentNodeDisplayedTime = 0;
                CurrentNodeSet.Next();
                if (CurrentNodeSet.ReachEnd)
                {
                    SetDialog(null);
                }
            }
        }
    }
}
