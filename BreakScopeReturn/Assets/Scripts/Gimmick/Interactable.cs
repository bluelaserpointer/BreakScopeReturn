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
            gameObject.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.Player.IsMyCollider(other))
        {
            print(name);
            onStepIn.Invoke();
            if (oneTime)
                gameObject.SetActive(false);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (GameManager.Instance.Player.IsMyCollider(other))
        {
            onStepOut.Invoke();
            if (oneTime)
                gameObject.SetActive(false);
        }
    }
}
