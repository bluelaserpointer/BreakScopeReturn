using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HPBar : MonoBehaviour
{
    [SerializeField]
    Image _fillImage;

    public void UpdateHP(float percent)
    {
        _fillImage.fillAmount = percent;
    }
}
