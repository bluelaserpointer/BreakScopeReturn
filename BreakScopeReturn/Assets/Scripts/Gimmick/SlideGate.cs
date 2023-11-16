using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SlideGate : SaveTarget
{
    public bool isOpen;

    [SerializeField] Transform _slideObject;
    [SerializeField] Vector3 _openVector;
    [SerializeField] SmoothDampTransition _openTransition;

    private void Awake()
    {
        _openTransition.value = isOpen ? 1 : 0;
    }
    public void Open(bool cond)
    {
        isOpen = cond;
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
