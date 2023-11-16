using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds 0-1 transition of <see cref="Mathf.SmoothDamp(float, float, ref float, float)"/>.
/// </summary>
[System.Serializable]
public struct SmoothDampTransition
{
    public float smoothTime;
    [HideInInspector]
    public float value;
    [HideInInspector]
    public float velocity;

    public SmoothDampTransition(float smoothTime)
    {
        this.smoothTime = smoothTime;
        velocity = 0;
        value = 0;
    }
    public void SmoothTowards(float target)
    {
        value = Mathf.SmoothDamp(value, target, ref velocity, smoothTime);
    }
    public void SmoothTowardsZero()
    {
        SmoothTowards(0);
    }
    public void SmoothTowardsOne()
    {
        SmoothTowards(1);
    }
    public static float PRECISION = 1.0E-5F;
    public bool NearZero => value < PRECISION;
    public bool NearOne => value > 1 - PRECISION;
    public bool LeaveZero => !NearZero;
    public bool LeaveOne => !NearOne;
    public float Lerp(float a, float b)
    {
        return Mathf.Lerp(a, b, value);
    }
    public Vector2 Lerp(Vector2 a, Vector2 b)
    {
        return Vector2.Lerp(a, b, value);
    }
    public Vector3 Lerp(Vector3 a, Vector3 b)
    {
        return Vector3.Lerp(a, b, value);
    }
    public Quaternion Lerp(Quaternion a, Quaternion b)
    {
        return Quaternion.Lerp(a, b, value);
    }
}
