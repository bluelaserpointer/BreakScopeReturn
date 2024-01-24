using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public class DialogPlainText : DialogNode
{
    [SerializeField]
    DialogTalker _talker;
    [SerializeField]
    TranslatableSentence _text;
    public DialogTalker Talker => _talker;
    public string Text => _text;

    private void OnEnable()
    {
        GameManager.Instance.DialogUI.SetText(Talker.TalkerNameRichText + Text);
    }
    private void OnDisable()
    {
        GameManager.Instance.DialogUI.SetText("");
    }
    public override float LifeTime()
    {
        return Text.Length * GameManager.Instance.DialogUI.displayTimePerWord;
    }
}
