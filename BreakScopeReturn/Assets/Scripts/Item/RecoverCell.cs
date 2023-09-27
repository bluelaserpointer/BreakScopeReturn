using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoverCell : Item
{
    [SerializeField] float recoverAmount;
    public override bool TryPickUp()
    {
        if (GameManager.Instance.Player.Health.Ratio < 1)
        {
            GameManager.Instance.Player.Heal(recoverAmount);
            return true;
        }
        return false;
    }
}
