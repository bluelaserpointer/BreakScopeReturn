using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BijointAim : MonoBehaviour
{
    [Header("Mobility")]
    [SerializeField]
    float _rotateSmoothTime = 10;
    [SerializeField]
    Vector2 _xRotateRange = new Vector2(-60, 75), _yRotateRange = new Vector2(-180, 180);

    [Header("Joint")]
    [Tooltip("Must be a fixed anchor relative to this object space")]
    [SerializeField]
    Transform _jointLookBase;
    [SerializeField]
    Transform _yRotationJoint;
    [SerializeField]
    Transform _xRotationJoint;

    [HideInInspector]
    public Vector3 targetAimPosition;

    Vector3 _gunRotateVelocity;

    private void FixedUpdate()
    {
        Vector3 localDiff = _jointLookBase.InverseTransformPoint(targetAimPosition);
        Vector3 horzDelta = localDiff.Set(y: 0);
        float horzDistance = Mathf.Sqrt(horzDelta.x * horzDelta.x + horzDelta.z * horzDelta.z);
        Vector2 newAngle = Vector2.zero;
        if (horzDelta != Vector3.zero)
        {
            newAngle.x = IzumiTools.ExtendedMath.ClampAndGetOverflow(Mathf.Rad2Deg * Mathf.Atan2(-localDiff.y, horzDistance), _xRotateRange.x, _xRotateRange.y, out _);
            newAngle.y = IzumiTools.ExtendedMath.ClampAndGetOverflow(Mathf.Rad2Deg * Mathf.Atan2(horzDelta.x, horzDelta.z), _yRotateRange.x, _yRotateRange.y, out _);
        }
        else
        {
            newAngle.x = 0;
            newAngle.y = 0;
        }
        _xRotationJoint.localEulerAngles = _xRotationJoint.localEulerAngles.Set(x: Mathf.SmoothDampAngle(_xRotationJoint.localEulerAngles.x, newAngle.x, ref _gunRotateVelocity.x, _rotateSmoothTime * Time.fixedDeltaTime));
        _yRotationJoint.localEulerAngles = _yRotationJoint.localEulerAngles.Set(y: Mathf.SmoothDampAngle(_yRotationJoint.localEulerAngles.y, newAngle.y, ref _gunRotateVelocity.y, _rotateSmoothTime * Time.fixedDeltaTime));
    }
}
