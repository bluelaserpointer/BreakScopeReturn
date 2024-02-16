using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractUI : MonoBehaviour
{
    [SerializeField]
    TranslatedTMP _actionNameText; 

    public void SetInfo(Interactable interactable)
    {
        _actionNameText.sentence = interactable.ActionName;
        _actionNameText.UpdateText();
    }
}
