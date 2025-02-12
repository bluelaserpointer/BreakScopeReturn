using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EventDropItem : DropItem
{
    public EventSignal eventSignal; 
    public override bool TryPickUp()
    {
        eventSignal.onEvent.Invoke();
        Destroy(gameObject);
        return true;
    }
    public override void Deserialize(string data)
    {
        eventSignal = GameManager.Instance.Stage.FindEventSignal(data);
    }

    public override string Serialize()
    {
        return eventSignal.name;
    }
}
