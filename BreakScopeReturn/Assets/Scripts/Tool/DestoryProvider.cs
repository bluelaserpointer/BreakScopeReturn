using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DestoryProvider : MonoBehaviour
{
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
