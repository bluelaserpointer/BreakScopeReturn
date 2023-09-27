using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class ArmDrop : MonoBehaviour
{
    public void Drop()
    {
        transform.parent = GameManager.Instance.CurrentStage.transform;
        foreach(var collider in transform.GetComponentsInChildren<Collider>())
        {
            collider.enabled = true;
        }
        GetComponent<Rigidbody>().isKinematic = false;
    }
}
