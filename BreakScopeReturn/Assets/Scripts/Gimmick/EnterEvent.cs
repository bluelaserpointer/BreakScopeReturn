using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class EnterEventBase : MonoBehaviour
{
    public abstract bool DisableAfterStepIn { get; }
    public abstract bool DisableAfterStepOut { get; }
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.Player.IsMyCollider(other))
        {
            if (DisableAfterStepIn)
                gameObject.SetActive(false);
            OnStepIn();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (GameManager.Instance.Player.IsMyCollider(other))
        {
            if (DisableAfterStepOut)
                gameObject.SetActive(false);
            OnStepOut();
        }
    }
    public abstract void OnStepIn();
    public abstract void OnStepOut();
}
[DisallowMultipleComponent]
public class EnterEvent : EnterEventBase
{
    public bool oneTime = true;
    [SerializeField]
    UnityEvent _onStepIn;
    [SerializeField]
    UnityEvent _onStepOut;

    public override bool DisableAfterStepIn => oneTime && _onStepIn.GetPersistentEventCount() > 0;
    public override bool DisableAfterStepOut => oneTime && _onStepOut.GetPersistentEventCount() > 0;
    public override void OnStepIn() => _onStepIn.Invoke();
    public override void OnStepOut() => _onStepOut.Invoke();
}
