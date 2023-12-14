using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rig))]
public class HandsRigger : MonoBehaviour
{
    public HandsRigAdvice rigAdvice;
    [Header("References")]
    [SerializeField]
    Animator _animator;
    [SerializeField]
    TwoBoneIKConstraint _leftHandIK, _rightHandIK;
    [SerializeField]
    ChainIKConstraint _leftHandThumb, _rightHandThumb;
    [SerializeField]
    ChainIKConstraint _leftHandIndex, _rightHandIndex;
    [SerializeField]
    ChainIKConstraint _leftHandMiddle, _rightHandMiddle;
    [SerializeField]
    ChainIKConstraint _leftHandRing, _rightHandRing;
    [SerializeField]
    ChainIKConstraint _leftHandPinky, _rightHandPinky;

    public Rig Rig { get; private set; }
    public bool RightHandIKWeightBelowOne => _rightHandIK.weight < 1;

    [ContextMenu("Auto Setup Rig")]
    private void SetUp()
    {
        if (_animator == null)
        {
            _animator = GetComponentInParent<Animator>();
        }
        if (_animator == null)
        {
            print(name + " is not inside of an " + nameof(Animator));
            return;
        }
        _leftHandIK.data.root = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        _leftHandIK.data.mid = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        _leftHandIK.data.tip = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
        _leftHandThumb.data.root = _animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
        _leftHandThumb.data.tip = _animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
        _leftHandIndex.data.root = _animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
        _leftHandIndex.data.tip = _animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
        _leftHandMiddle.data.root = _animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        _leftHandMiddle.data.tip = _animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
        _leftHandRing.data.root = _animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
        _leftHandRing.data.tip = _animator.GetBoneTransform(HumanBodyBones.LeftRingDistal);
        _leftHandPinky.data.root = _animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
        _leftHandPinky.data.tip = _animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
        _rightHandIK.data.root = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        _rightHandIK.data.mid = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        _rightHandIK.data.tip = _animator.GetBoneTransform(HumanBodyBones.RightHand);
        _rightHandThumb.data.root = _animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
        _rightHandThumb.data.tip = _animator.GetBoneTransform(HumanBodyBones.RightThumbDistal);
        _rightHandIndex.data.root = _animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
        _rightHandIndex.data.tip = _animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
        _rightHandMiddle.data.root = _animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
        _rightHandMiddle.data.tip = _animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        _rightHandRing.data.root = _animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
        _rightHandRing.data.tip = _animator.GetBoneTransform(HumanBodyBones.RightRingDistal);
        _rightHandPinky.data.root = _animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
        _rightHandPinky.data.tip = _animator.GetBoneTransform(HumanBodyBones.RightLittleDistal);
    }
    [ContextMenu("UpdateEffectorTransform")]
    private void UpdateEffectorTransform()
    {
        if (rigAdvice == null)
        {
            return;
        }
        _leftHandIK.data.target.SetPositionAndRotation(rigAdvice.leftHand);
        _leftHandIK.data.hint.SetPositionAndRotation(rigAdvice.leftHandHint);
        _leftHandThumb.data.target.SetPositionAndRotation(rigAdvice.leftThumb);
        _leftHandIndex.data.target.SetPositionAndRotation(rigAdvice.leftIndex);
        _leftHandMiddle.data.target.SetPositionAndRotation(rigAdvice.leftMiddle);
        _leftHandRing.data.target.SetPositionAndRotation(rigAdvice.leftRing);
        _leftHandPinky.data.target.SetPositionAndRotation(rigAdvice.leftPinky);
        _rightHandIK.data.target.SetPositionAndRotation(rigAdvice.rightHand);
        _rightHandIK.data.hint.SetPositionAndRotation(rigAdvice.rightHandHint);
        _rightHandThumb.data.target.SetPositionAndRotation(rigAdvice.rightThumb);
        _rightHandIndex.data.target.SetPositionAndRotation(rigAdvice.rightIndex);
        _rightHandMiddle.data.target.SetPositionAndRotation(rigAdvice.rightMiddle);
        _rightHandRing.data.target.SetPositionAndRotation(rigAdvice.rightRing);
        _rightHandPinky.data.target.SetPositionAndRotation(rigAdvice.rightPinky);
    }
    private void Awake()
    {
        Rig = GetComponent<Rig>();
    }
    private void Update()
    {
        UpdateEffectorTransform();
    }
}
