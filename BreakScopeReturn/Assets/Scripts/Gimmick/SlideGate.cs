using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class SlideGate : SaveTarget
{
    public bool isOpen;
    public bool isLocked;

    [SerializeField] Transform _slideObject;
    [SerializeField] Vector3 _openVector;
    [SerializeField] SmoothDampTransition _openTransition;
    [SerializeField] UnityEvent _interactWhileLock;

    [Header("SE")]
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _openSE, _closeSE;

    private void Awake()
    {
        _openTransition.value = isOpen ? 1 : 0;
    }
    public void Open(bool cond)
    {
        if (isOpen == cond)
            return;
        if (isLocked)
        {
            _interactWhileLock.Invoke();
            return;
        }
        _audioSource.PlayOneShot(cond ? _openSE : _closeSE);
        isOpen = cond;
    }
    public void Lock(bool cond)
    {
        isLocked = cond;
    }
    private void FixedUpdate()
    {
        _openTransition.SmoothTowards(isOpen ? 1 : 0);
        _slideObject.position = _openTransition.Lerp(transform.position, transform.position + _openVector);
    }
    public override string Serialize()
    {
        return isOpen.ToString();
    }

    public override void Deserialize(string data)
    {
        isOpen = bool.Parse(data);
        Awake();
    }
}
