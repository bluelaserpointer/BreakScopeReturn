using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extension
{
    public static Vector3 Set(this Vector3 vector, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
    }
    public static Vector3 Set(ref Vector3 vector, float? x = null, float? y = null, float? z = null)
    {
        if (x != null)
            vector.x = x.Value;
        if (y != null)
            vector.y = y.Value;
        if (z != null)
            vector.z = z.Value;
        return vector;
    }
    public static float XAngleRad(this Vector3 vector)
    {
        return Mathf.Atan2(vector.z, vector.y);
    }
    public static float YAngleRad(this Vector3 vector)
    {
        return Mathf.Atan2(vector.x, vector.z);
    }
    public static float ZAngleRad(this Vector3 vector)
    {
        return Mathf.Atan2(vector.y, vector.x);
    }
    public static float XAngle(this Vector3 vector)
    {
        return Mathf.Rad2Deg * XAngleRad(vector);
    }
    public static float YAngle(this Vector3 vector)
    {
        return Mathf.Rad2Deg * YAngleRad(vector);
    }
    public static float ZAngle(this Vector3 vector)
    {
        return Mathf.Rad2Deg * ZAngleRad(vector);
    }
}
