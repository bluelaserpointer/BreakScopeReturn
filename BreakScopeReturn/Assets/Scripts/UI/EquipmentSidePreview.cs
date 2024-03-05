using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EquipmentSidePreview : MonoBehaviour
{
    [SerializeField]
    GunSidePreviewSlot[] _slots;
    [SerializeField]
    Transform _abilitySlotParent;
    [SerializeField]
    Transform _throwableSlotParent;

    public void SetEquipment(int index, HandEquipment equipment)
    {
        _slots[index].SetEquipment(equipment);
    }
}
