using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Text))]
public class DialogPlainText : DialogNode
{
    public Text Text { get; private set; }

    public override float DisplayTime()
    {
        return Text.text.Length * GameManager.Instance.DialogUI.displayTimePerWord;
    }

    private void Awake()
    {
        Text = GetComponent<Text>();
    }
}
