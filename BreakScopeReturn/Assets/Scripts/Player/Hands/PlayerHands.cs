using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class PlayerHands : MonoBehaviour
{
    public abstract HandsType HandsType { get; }
    public Animator Animator => GameManager.Instance.Player.Animator;
    public bool IsAiming { get; protected set; }
    public float MouseSensitivityModify { get; protected set; }
    public abstract void Init(HandEquipment equipment);
    public virtual void WithdrawItemAndDisable()
    {
        gameObject.SetActive(false);
    }
}
