using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GunSidePreviewSlot : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _nameText;
    [SerializeField]
    Image _icon;
    [SerializeField]
    TextMeshProUGUI _subText;

    public Gun Gun { get; private set; }

    public void SetGun(Gun gun)
    {
        Gun = gun;
        if (Gun != null)
        {
            _nameText.text = gun.DisplayName;
            _icon.enabled = true;
            _icon.sprite = gun.Icon;
            _subText.text = Gun.magazine.Value + " (" + Gun.spareAmmo + ")";
        }
        else
        {
            _nameText.text = "";
            _icon.enabled = false;
            _subText.text = "";
        }
    }
    private void Update()
    {
        if (Gun == null)
            return;
        _subText.text = Gun.magazine.Value + " (" + Gun.spareAmmo + ")";
    }
}
