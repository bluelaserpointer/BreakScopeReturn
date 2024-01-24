using IzumiTools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GunSidePreviewSlot : MonoBehaviour
{
    [SerializeField]
    List<GameObject> _onSelectEnables;
    [SerializeField]
    TextMeshProUGUI _nameText;
    [SerializeField]
    Image _icon;
    [SerializeField]
    TextMeshProUGUI _magazineAmmoText, _spareAmmoText;
    [SerializeField]
    ReuseNest<Image> _spareAmmoSquareGrid;

    public Gun Gun { get; private set; }
    
    public void SetGun(Gun gun)
    {
        Gun = gun;
        if (Gun != null)
        {
            _nameText.text = gun.DisplayName;
            _icon.enabled = true;
            _icon.sprite = gun.Icon;
        }
        else
        {
            _nameText.text = "";
            _icon.enabled = false;
        }
        UpdateGunDataUI();
    }
    private void Update()
    {
        if (Gun == null)
            return;
        UpdateGunDataUI();
    }
    public void UpdateGunDataUI()
    {
        //Selection Background
        //TODO: Animate
        bool selected = Gun.HoldedByPlayer;
        _onSelectEnables.ForEach(each => each.SetActive(selected));
        //Ammo Text
        if (Gun == null)
        {
            _magazineAmmoText.text = _spareAmmoText.text = "";
            return;
        }
        _magazineAmmoText.text = Gun.magazine.Value.ToString();
        _spareAmmoText.text = Gun.spareAmmo.ToString();
        //Spare Ammo Squares
        int spareMagazineCount = Mathf.CeilToInt(Gun.spareAmmo / Gun.magazine.Capacity);
        int countDiff = spareMagazineCount - _spareAmmoSquareGrid.ActiveCount;
        while (countDiff > 0)
        {
            _spareAmmoSquareGrid.EnableOne().transform.SetSiblingIndex(0);
            --countDiff;
        }
        while (countDiff < 0)
        {
            _spareAmmoSquareGrid.DisableFirst();
            ++countDiff;
        }
        float remain = Gun.spareAmmo % Gun.magazine.Capacity;
        _spareAmmoSquareGrid.LastChild.fillAmount = (remain == 0) ? 1 : remain / Gun.magazine.Capacity;
    }
}
