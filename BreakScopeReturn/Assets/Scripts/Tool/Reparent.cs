using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Reparent : MonoBehaviour
{
    public void SetParent(Transform parent)
    {
        transform.SetParent(parent, true);
    }
}
