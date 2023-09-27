using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extension
{
    public static float AngleRad(this Vector2 vector)
    {
        return Mathf.Atan2(vector.y, vector.x);
    }
    public static float Angle(this Vector2 vector)
    {
        return Mathf.Rad2Deg * AngleRad(vector);
    }
}
