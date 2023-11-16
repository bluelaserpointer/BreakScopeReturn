using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class EventSignal : MonoBehaviour
{
    public UnityEvent onEvent = new();
}
