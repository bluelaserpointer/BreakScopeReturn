using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Achievement : MonoBehaviour
{
    [SerializeField]
    Text _descriptionText;
    [SerializeField]
    Text _conditionText;
    [SerializeField]
    Image _achievedIcon;

    [SerializeField]
    Color _achievedColor, _notAchievedColor;

    public void SetAchivement(string description, bool condition)
    {
        _descriptionText.text = description;
        if (condition)
        {
            _conditionText.text = "����ץ�`�ȣ�";
            _achievedIcon.color = _achievedColor;
        }
        else
        {
            _conditionText.text = "δ�_��";
            _achievedIcon.color = _notAchievedColor;
        }
    }
}
