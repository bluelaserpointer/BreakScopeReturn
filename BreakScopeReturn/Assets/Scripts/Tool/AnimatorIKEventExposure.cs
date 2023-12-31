using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class AnimatorIKEventExposure : MonoBehaviour
{
    public string lookAtIKControlLayerName;
    [Range(0f, 1f)]
    public float lookAtWeight = 1, lookAtBodyWeight, lookAtClampWeight = 0.5F;
    public string leftHandIKControlLayerName;
    [Range(0f, 1f)]
    public float leftHandPositionWeight = 1, leftHandRotationWeight = 1;
    public string rightHandIKControlLayerName;
    [Range(0f, 1f)]
    public float rightHandPositionWeight = 1, rightHandRotationWeight = 1;
    [Range(0f, 1f)]
    public float equipmentFollowRightHandPositionWeight = 1, equipmentFollowRightHandRotationWeight = 1;

    public UnityEvent<int> onAnimatorIK = new UnityEvent<int>();

    public Animator Animator => _animator ?? (_animator = GetComponent<Animator>());
    private Animator _animator;
    private float _lookAtIKControlLayer;
    private float _leftHandIKControlLayer;
    private float _rightHandIKControlLayer;
    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
        _lookAtIKControlLayer = GetLayerIndex_WarnNotFound(lookAtIKControlLayerName);
        _leftHandIKControlLayer = GetLayerIndex_WarnNotFound(leftHandIKControlLayerName);
        _rightHandIKControlLayer = GetLayerIndex_WarnNotFound(rightHandIKControlLayerName);
    }
    private int GetLayerIndex_WarnNotFound(string layerName)
    {
        int layerIndex = _animator.GetLayerIndex(layerName);
        if (layerIndex == -1)
            print("<!> Animation layer \"" + layerName + "\" does not exist in Animator \"" + _animator.name + "\"");
        return layerIndex;
    }
    private void OnAnimatorIK(int layerIndex)
    {
        onAnimatorIK.Invoke(layerIndex);
        if (layerIndex == _lookAtIKControlLayer)
        {
            Animator.SetLookAtWeight(lookAtWeight, lookAtBodyWeight, 1, 0, lookAtClampWeight);
        }
        if (layerIndex == _leftHandIKControlLayer)
        {
            Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandPositionWeight);
            Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandRotationWeight);
        }
        if (layerIndex == _rightHandIKControlLayer)
        {
            Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandPositionWeight);
            Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandRotationWeight);
        }
    }
}
