using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SaveExistance : SaveTarget
{
    public override void Deserialize(string data)
    {
        gameObject.SetActive(bool.Parse(data));
    }

    public override string Serialize()
    {
        return gameObject.activeSelf.ToString();
    }
}
