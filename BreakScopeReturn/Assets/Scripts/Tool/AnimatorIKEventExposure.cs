using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class AnimatorIKEventExposure : MonoBehaviour
{
    [Range(0f, 1f)]
    public float lookAtWeight = 1, lookAtBodyWeight, lookAtClampWeight = 0.5F;
    [Range(0f, 1f)]
    public float leftHandPositionWeight = 1, leftHandRotationWeight = 1;
    [Range(0f, 1f)]
    public float rightHandPositionWeight = 1, rightHandRotationWeight = 1;
    [Range(0f, 1f)]
    public float equipmentFollowRightHandPositionWeight = 1, equipmentFollowRightHandRotationWeight = 1;

    public Animator Animator => _animator ?? (_animator = GetComponent<Animator>());
    private Animator _animator;
    public UnityEvent<int> onAnimatorIK = new UnityEvent<int>();
    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }
    private void OnAnimatorIK(int layerIndex)
    {
        onAnimatorIK.Invoke(layerIndex);
        Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandPositionWeight);
        Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandRotationWeight);
        Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandPositionWeight);
        Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandRotationWeight);
        Animator.SetLookAtWeight(lookAtWeight, lookAtBodyWeight, 1, 0, lookAtClampWeight);
    }
}
