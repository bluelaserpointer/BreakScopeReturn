using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideGate : MonoBehaviour
{
    public bool isOpen;

    [SerializeField] Transform _slideObject;
    [SerializeField] Vector3 _slideVector;
    [SerializeField] float _lerpFactor;

    public void Open(bool cond)
    {
        isOpen = cond;
    }
    private void FixedUpdate()
    {
        Vector3 targetPos = isOpen ? transform.position + _slideVector : transform.position;
        _slideObject.position = Vector3.Lerp(_slideObject.position, targetPos, _lerpFactor * Time.fixedDeltaTime);
    }
}
