using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class PlayerHands : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField]
    Animator _handsAnimator;

    [Header("UI")]
    [SerializeField]
    Sprite _icon;

    public Animator HandsAnimator => _handsAnimator;
    public Sprite Icon => _icon;
    public bool IsAiming { get; protected set; }
    public float MouseSensitivityModify { get; protected set; }
    public virtual void WithdrawItemAndDestroy()
    {
        Destroy(gameObject);
    }
}
