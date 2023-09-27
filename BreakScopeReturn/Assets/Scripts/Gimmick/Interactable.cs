using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Interactable : MonoBehaviour
{
    [SerializeField] protected bool oneTime;
    [SerializeField] protected Sprite interactIcon;
    public UnityEvent onInteract;
    public UnityEvent onStepIn;
    public UnityEvent onStepOut;

    public Sprite InteractIcon => interactIcon;
    public bool ContainsActiveInteract => onInteract.GetPersistentEventCount() > 0;

    public void Interact()
    {
        onInteract.Invoke();
        if (oneTime)
            Destroy(this);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Player>() != null)
        {
            onStepIn.Invoke();
            if (oneTime)
                Destroy(gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<Player>() != null)
        {
            onStepOut.Invoke();
            if (oneTime)
                Destroy(gameObject);
        }
    }
}
