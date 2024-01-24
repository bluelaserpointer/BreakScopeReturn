using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoverCell : DropItem
{
    public float recoverAmount;
    [SerializeField]
    AudioClip _useSE;

    public override bool TryPickUp()
    {
        if (GameManager.Instance.Player.Health.Ratio < 1)
        {
            GameManager.Instance.Player.Heal(recoverAmount);
            AudioSource.PlayClipAtPoint(_useSE, transform.position);
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
