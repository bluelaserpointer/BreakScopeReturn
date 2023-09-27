using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKControl : MonoBehaviour
{
    public AvatarIKGoal avaterIKGoal;
    public Transform anchor;
    [Range(0, 1)]
    public float positionWeight = 1, rotationWeight = 1;

    Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (anchor == null)
            return;
        _animator.SetIKPosition(avaterIKGoal, anchor.position);
        _animator.SetIKPositionWeight(avaterIKGoal, positionWeight);
        _animator.SetIKRotation(avaterIKGoal, anchor.rotation);
        _animator.SetIKRotationWeight(avaterIKGoal, rotationWeight);
    }
}
