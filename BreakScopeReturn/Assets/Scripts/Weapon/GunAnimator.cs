using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(Gun))]
public class GunAnimator : MonoBehaviour
{
    [SerializeField]
    AnimationClip _fireAnimationClip;
    private void Awake()
    {
        Animator _animator = GetComponent<Animator>();
        Gun gunHint = GetComponent<Gun>();
        gunHint.onFireCDSet.AddListener(cd =>
        {
            _animator.SetFloat("fireSpeedMultiplier", _fireAnimationClip.length / Mathf.Max(cd, 0.1F));
        });
        gunHint.onFire.AddListener(() =>
        {
            _animator.SetTrigger("fire");
        });
    }
}
