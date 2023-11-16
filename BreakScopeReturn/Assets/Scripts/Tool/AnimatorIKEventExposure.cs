using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class AnimatorIKEventExposure : MonoBehaviour
{
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
    }
}
