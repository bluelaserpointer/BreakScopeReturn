using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class TrailEffect : MonoBehaviour
{
    [SerializeField]
    Rigidbody refRigidbody;
    [SerializeField]
    int maxlineLength;
    [SerializeField]
    float fadeStartTime;
    [SerializeField]
    float fadeEndTime;
    [SerializeField]
    bool negateDisorientedVelocity;

    LineRenderer lineRenderer;
    Vector3 localOffset;

    class LinePoint
    {
        public Vector3 position;
        public Vector3 velocity;
        public float SpawnTime { get; private set; }
        public LinePoint(Vector3 position, Vector3 velocity)
        {
            this.position = position;
            this.velocity = velocity;
            SpawnTime = Time.timeSinceLevelLoad;
        }
    }
    List<LinePoint> linePoints = new List<LinePoint>();
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        localOffset = transform.localPosition;
    }
    private void FixedUpdate()
    {
        foreach (var point in linePoints)
        {
            point.position += point.velocity * Time.fixedDeltaTime;
        }
    }
    private void LateUpdate()
    {
        if (linePoints.Count > maxlineLength)
        {
            linePoints.RemoveFirst();
        }
        Vector3 newPointVelocity = Vector3.zero;
        if (negateDisorientedVelocity)
        {
            newPointVelocity += refRigidbody.velocity - Vector3.Project(refRigidbody.velocity, refRigidbody.transform.forward);
        }
        linePoints.Add(new LinePoint(refRigidbody.transform.TransformPoint(localOffset), newPointVelocity));
        float fadeStartTargetTime = Time.timeSinceLevelLoad - fadeStartTime;
        float fadeEndTargetTime = Time.timeSinceLevelLoad - fadeEndTime;
        int fadeStartId = -1, fadeEndId = -1;
        for (int i = 0; i < linePoints.Count; ++i)
        {
            if (fadeEndId == -1)
            {
                if (linePoints[i].SpawnTime > fadeEndTargetTime)
                {
                    fadeEndId = i;
                }
            }
            else if (fadeStartId == -1)
            {
                if (linePoints[i].SpawnTime > fadeStartTargetTime)
                {
                    fadeStartId = i;
                }
            }
        }
        float endAlpha = Mathf.Clamp01(1 - (Time.timeSinceLevelLoad - linePoints[fadeEndId].SpawnTime - fadeStartTime) / (fadeEndTime - fadeStartTime));
        Gradient gradient = new Gradient();
        GradientAlphaKey[] gradientAlphaKeys = new GradientAlphaKey[2];
        gradientAlphaKeys[0].time = 1;
        gradientAlphaKeys[0].alpha = 1;
        gradientAlphaKeys[1].time = 0;
        gradientAlphaKeys[1].alpha = endAlpha;
        gradient.alphaKeys = gradientAlphaKeys;
        gradient.colorKeys = lineRenderer.colorGradient.colorKeys;
        linePoints.RemoveRange(0, fadeEndId);
        lineRenderer.colorGradient = gradient;
        lineRenderer.positionCount = linePoints.Count;
        List<Vector3> linePointList = new List<Vector3>();
        foreach (var point in linePoints)
        {
            linePointList.Add(point.position);
        }
        lineRenderer.SetPositions(linePointList.ToArray());
    }
}
