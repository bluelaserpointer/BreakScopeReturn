using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PatrolAnchor : MonoBehaviour
{
    [SerializeField]
    float _stayDuration;
    [SerializeField]
    bool _lookFowardOnReach;

    public float StayDuration => _stayDuration;
    public bool LookFowardOnReach => _lookFowardOnReach;
}
