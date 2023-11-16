using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoverCell : DropItem
{
    public float recoverAmount;

    public override bool TryPickUp()
    {
        if (GameManager.Instance.Player.Health.Ratio < 1)
        {
            GameManager.Instance.Player.Heal(recoverAmount);
            Destroy(gameObject);
            return true;
        }
        return false;
    }
    public override void Deserialize(string data)
    {
        recoverAmount = float.Parse(data);
    }

    public override string Serialize()
    {
        return recoverAmount.ToString();
    }
}
