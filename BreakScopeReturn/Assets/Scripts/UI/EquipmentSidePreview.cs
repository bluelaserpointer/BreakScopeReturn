using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EquipmentSidePreview : MonoBehaviour
{
    [SerializeField]
    GunSidePreviewSlot[] _gunSlots;
    [SerializeField]
    Transform _abilitySlotParent;
    [SerializeField]
    Transform _throwableSlotParent;

    public void SetGun(int index, Gun gun)
    {
        _gunSlots[index].SetGun(gun);
    }
}
