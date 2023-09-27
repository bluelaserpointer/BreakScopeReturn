using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : Interactable
{
    protected virtual void Awake()
    {
        onStepIn.AddListener(() => {
            if(TryPickUp())
            {
                Destroy(gameObject);
            }
        });
    }
    public abstract bool TryPickUp();
}
