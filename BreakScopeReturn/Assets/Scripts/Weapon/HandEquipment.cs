using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandsType { Rifle, Toss }
[System.Serializable]
public abstract class HandEquipment : SaveTarget
{
    [SerializeField]
    HandsType _handsType;
    public HandsType HandsType => _handsType;

    public bool HoldedByPlayer => this == GameManager.Instance.Player.GunInventory.HoldingEquipment;
}
