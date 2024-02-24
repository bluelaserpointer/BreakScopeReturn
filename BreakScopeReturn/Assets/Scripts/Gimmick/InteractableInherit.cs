using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class InteractableInherit : MonoBehaviour, IInteractable
{
    [Header("Inherit")]
    public Interactable inheritTarget;

    public Sprite InteractIcon => inheritTarget.InteractIcon;

    public TranslatableSentence ActionName => inheritTarget.ActionName;

    public void Interact()
    {
        inheritTarget.Interact();
    }
}
