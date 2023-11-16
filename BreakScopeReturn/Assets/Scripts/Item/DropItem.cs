using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class DropItem : SaveTarget
{
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.Player.IsMyCollider(other))
            TryPickUp();
    }
    public abstract bool TryPickUp();
}
