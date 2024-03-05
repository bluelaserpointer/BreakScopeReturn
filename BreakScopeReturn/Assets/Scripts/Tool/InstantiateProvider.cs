using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InstantiateProvider : MonoBehaviour
{
    public GameObject target;
    public Transform parent;
    public void Instantiate()
    {
        GameObject duplicated = Instantiate(target, transform.position, transform.rotation);
        if (parent != null)
            duplicated.transform.SetParent(parent, true);
    }
}
