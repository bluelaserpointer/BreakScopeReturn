using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class LookAtIKControl : MonoBehaviour
{
    public Vector3 lookAtPosition;
    public float lookAtWeight;

    Animator _animator;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (lookAtWeight == 0)
            return;
        _animator.SetLookAtPosition(lookAtPosition);
        _animator.SetLookAtWeight(lookAtWeight);
    }
    public void SetLookAtPosition(Vector3 position, float weight = 1)
    {
        lookAtPosition = position;
        lookAtWeight = weight;
    }
}
