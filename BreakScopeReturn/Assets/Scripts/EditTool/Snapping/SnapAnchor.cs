using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SnapAnchor : MonoBehaviour
{
    [SerializeField]
    SnapGroup _snapGroup;

    public SnapGroup SnapGroup => _snapGroup;
}
