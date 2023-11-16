using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TrajectoryDrawer : MonoBehaviour {
    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPhysicsFrameIterations = 100;

    public void SimulateSphereTrajectory(Grenade grenade, SphereCollider sphereCollider, Vector3 initialPosition, Vector3 initialVelocity)
    {
        _line.enabled = true;
        List<Vector3> positions = new List<Vector3>();
        Vector3 position = initialPosition;
        Vector3 velocity = initialVelocity;
        for (var i = 0; i < _maxPhysicsFrameIterations; i++) {
            Grenade.ProjectileUpdate(grenade, ref position, ref velocity, sphereCollider.radius, sphereCollider.sharedMaterial);
            positions.Add(position);
        }
        _line.positionCount = positions.Count;
        _line.SetPositions(positions.ToArray());
    }
    public void Clear()
    {
        _line.enabled = false;
    }
}