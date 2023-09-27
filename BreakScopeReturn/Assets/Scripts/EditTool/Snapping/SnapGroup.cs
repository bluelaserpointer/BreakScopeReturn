using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSnapGroup", menuName = "Snap/SnapGroup")]
public class SnapGroup : ScriptableObject
{
    [SerializeField]
    bool hasMirrorVariant;
    [SerializeField]
    Vector3 mirrorAxis;
    [SerializeField]
    bool hasAngleVariant;
    [SerializeField]
    float angleSpan;

    public bool HasMirrorVariant => hasMirrorVariant;
    public Vector3 MirrorAxis => mirrorAxis;
    public bool HasAngleVariant => hasAngleVariant;
    public float AngleSpan => angleSpan;
}
