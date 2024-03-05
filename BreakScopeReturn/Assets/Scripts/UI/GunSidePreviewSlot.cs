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

    public HandEquipment Equipment { get; private set; }
    
    public void SetEquipment(HandEquipment equipment)
    {
        Equipment = equipment;
        if (Equipment != null)
        {
            _nameText.text = equipment.DisplayName;
            _icon.enabled = true;
            _icon.sprite = equipment.Icon;
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
        if (Equipment == null)
            return;
        UpdateGunDataUI();
    }
    public void UpdateGunDataUI()
    {
        //Selection Background
        //TODO: Animate
        bool selected = Equipment.HoldedByPlayer;
        _onSelectEnables.ForEach(each => each.SetActive(selected));
        //Ammo Text
        if (Equipment == null)
        {
            _magazineAmmoText.text = _spareAmmoText.text = "";
            return;
        }
        _magazineAmmoText.text = Equipment.magazine.Value.ToString();
        _spareAmmoText.text = Equipment.spareAmmo.ToString();
        //Spare Ammo Squares
        int spareMagazineCount = Mathf.CeilToInt(Equipment.spareAmmo / Equipment.magazine.Capacity);
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
        float remain = Equipment.spareAmmo % Equipment.magazine.Capacity;
        _spareAmmoSquareGrid.LastChild.fillAmount = (remain == 0) ? 1 : remain / Equipment.magazine.Capacity;
    }
}
