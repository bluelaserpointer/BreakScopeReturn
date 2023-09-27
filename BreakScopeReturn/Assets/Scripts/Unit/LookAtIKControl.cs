using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class LookAtIKControl : MonoBehaviour
{
    public bool controlLookAtPosition;
    public Vector3 lookAtPosition;
    public float lookAtWeight;

    Animator _animator;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (controlLookAtPosition)
        {
            _animator.SetLookAtPosition(lookAtPosition);
            _animator.SetLookAtWeight(lookAtWeight);
        }
        else
        {
            _animator.SetLookAtWeight(0);
        }
    }
    public void SetLookAtPosition(Vector3 position, float weight = 1)
    {
        controlLookAtPosition = true;
        lookAtPosition = position;
        lookAtWeight = weight;
    }
    public void StopControlLookAtPosition()
    {
        controlLookAtPosition = false;
    }
}
