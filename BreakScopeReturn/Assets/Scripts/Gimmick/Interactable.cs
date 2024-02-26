using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IInteractable : IComponentInterface
{
    public Sprite InteractIcon { get; }
    public TranslatableSentence ActionName { get; }
    public void Interact();
}
[DisallowMultipleComponent]
public class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] protected bool oneTime;
    [SerializeField] protected Sprite interactIcon;
    [SerializeField] protected TranslatableSentenceSO actionName;
    public UnityEvent onInteract;

    public Sprite InteractIcon => interactIcon;
    public TranslatableSentence ActionName => actionName != null ? actionName : new TranslatableSentence() { defaultString = "?missing?" };

    public void Interact()
    {
        if (oneTime)
            gameObject.SetActive(false);
        onInteract.Invoke();
    }
}
