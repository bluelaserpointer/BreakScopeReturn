using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class PenetratableCollider : MonoBehaviour
{
    [SerializeField]
    List<Collider> highPriorityColliders;

    public bool IgnoreThis(RaycastHit[] rayHits)
    {
        foreach (var rayHit in rayHits)
        {
            if (highPriorityColliders.Contains(rayHit.collider))
            {
                return true;
            }
        }
        return false;   
    }
    /// <summary>
    /// Colliders have <see cref="PenetratableCollider"/> will be ignored if <see cref="highPriorityColliders"/> are contained in rayHits
    /// </summary>
    /// <param name="rayHit"></param>
    /// <param name="rayHits"></param>
    /// <returns></returns>
    public static bool IgnoreThis(RaycastHit rayHit, RaycastHit[] rayHits)
    {
        return rayHit.collider.TryGetComponent(out PenetratableCollider pCollider) && pCollider.IgnoreThis(rayHits);
    }
}
