using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ReloadIndicator : MonoBehaviour
{
    [SerializeField]
    RectTransform _visibleRoot;
    [SerializeField]
    RectTransform _crosshair;
    [SerializeField]
    Image _image;
    [SerializeField]
    PlayerGunHands _gunHands;

    void Update()
    {
        if (_gunHands.IsReloading)
        {
            _visibleRoot.gameObject.SetActive(true);
            _crosshair.gameObject.SetActive(false);
            _image.fillAmount = _gunHands.ReloadCD.Ratio;
        }
        else
        {
            _visibleRoot.gameObject.SetActive(false);
            _crosshair.gameObject.SetActive(true);
        }
    }
}
