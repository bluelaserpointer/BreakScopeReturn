using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TransformRelator
{
    public Vector3 RelativePosition { get; private set; }
    public Quaternion RelativeRotation { get; private set;}

    public TransformRelator(Transform child, Transform parent)
    {
        RelativePosition = parent.InverseTransformPoint(child.position);
        RelativeRotation = Quaternion.Inverse(parent.rotation) * child.rotation;
    }
    public void UpdateRelation(Transform child, Transform parent)
    {
        RelativePosition = parent.InverseTransformPoint(child.position);
        RelativeRotation = Quaternion.Inverse(parent.rotation) * child.rotation;
    }
    public Vector3 GetChildPosition(Vector3 parentPosition, Quaternion parentRotation)
    {
        return parentPosition + parentRotation * RelativePosition;
    }
    public Vector3 GetChildPosition(Transform parent)
    {
        return GetChildPosition(parent.position, parent.rotation);
    }
    public Quaternion GetChildRotation(Quaternion parentRotation)
    {
        return parentRotation * RelativeRotation;
    }
    public Quaternion GetChildRotation(Transform parent)
    {
        return GetChildRotation(parent.rotation);
    }
    public void ApplyChildTransform(Transform target, Vector3 parentPosition, Quaternion parentRotation)
    {
        target.SetPositionAndRotation(GetChildPosition(parentPosition, parentRotation), GetChildRotation(parentRotation));
    }
    public void ApplyChildTransform(Transform target, Transform parent)
    {
        target.SetPositionAndRotation(GetChildPosition(parent), GetChildRotation(parent));
    }
}
