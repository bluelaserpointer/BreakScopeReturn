using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandsType { Rifle, Place, Toss }
[System.Serializable]
public abstract class HandEquipment : SaveTarget
{
    [Header("Info")]
    [SerializeField]
    HandsType _handsType;
    [SerializeField]
    string _displayName;
    [SerializeField]
    Sprite _icon;

    [Header("Dynamic data")]
    public IzumiTools.CappedValue magazine;
    public int spareAmmo;

    public HandsType HandsType => _handsType;
    public string DisplayName => _displayName;
    public Sprite Icon => _icon;

    public bool HoldedByPlayer => this == GameManager.Instance.Player.GunInventory.HoldingEquipment;
    protected virtual void Awake()
    {
        magazine.Fill();
    }
    struct HandEquipmentSave
    {
        public int magazineSize;
        public int magazineAmmo;
        public int spareAmmo;
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(new HandEquipmentSave()
        {
            magazineSize = (int)magazine.Capacity,
            magazineAmmo = (int)magazine.Value,
            spareAmmo = spareAmmo
        });
    }
    public override void Deserialize(string json)
    {
        HandEquipmentSave save = JsonUtility.FromJson<HandEquipmentSave>(json);
        magazine.Capacity = save.magazineSize;
        magazine.Value = save.magazineAmmo;
        spareAmmo = save.spareAmmo;
    }
}
