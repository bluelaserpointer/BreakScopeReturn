using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class BreakableByCollision : MonoBehaviour
{
    public float breakVelocityThrehold;
    public UnityEvent onBrake;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > breakVelocityThrehold)
        {
            onBrake.Invoke();
            Destroy(gameObject);
        }
    }
}
