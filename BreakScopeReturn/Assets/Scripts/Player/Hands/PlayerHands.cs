using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class PlayerHands : MonoBehaviour
{
    public Animator Animator => GameManager.Instance.Player.Animator;
    public bool IsAiming { get; protected set; }
    public float MouseSensitivityModify { get; protected set; }
    public virtual void WithdrawItemAndDestroy()
    {
        Destroy(gameObject);
    }
}
