using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class RagdollRelax : MonoBehaviour
{
    public bool relax;

    readonly List<Rigidbody> rigidbodies = new List<Rigidbody>();
    Animator _animator;

    bool _internalRelax;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        rigidbodies.AddRange(GetComponentsInChildren<Rigidbody>());
        UpdateRelax();
    }
    private void Update()
    {
        Check();
    }
    public void Check()
    {
        if (_internalRelax != relax)
        {
            UpdateRelax();
        }
    }
    void UpdateRelax()
    {
        if (_internalRelax = relax)
        {
            _animator.enabled = false;
            rigidbodies.ForEach(body => body.isKinematic = false);
        }
        else
        {
            _animator.enabled = true;
            rigidbodies.ForEach(body => body.isKinematic = true);
        }
    }
}
