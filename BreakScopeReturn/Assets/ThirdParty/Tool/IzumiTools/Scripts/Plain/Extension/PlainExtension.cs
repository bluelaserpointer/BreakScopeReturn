using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlainExtension
{
    public static bool RaycastAndTryGetPoint(this Plane plane, Ray ray, out Vector3 point)
    {
        if(plane.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }
        point = Vector3.zero;
        return false;
    }
}
