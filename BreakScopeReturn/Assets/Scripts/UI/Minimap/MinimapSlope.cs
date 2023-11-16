using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class MinimapSlope : MinimapParts
{
    [SerializeField]
    Color _startColor, _endColor;
    public LineRenderer LineRenderer { get; private set; }

    private void Awake()
    {
        LineRenderer = GetComponent<LineRenderer>();
    }
    private void Update()
    {
        if (LineRenderer.positionCount != 2)
        {
            print("<!> MinimapSlope only supports LineRenderer of 2 points");
            return;
        }
        float startY, endY;
        if (LineRenderer.useWorldSpace)
        {
            startY = LineRenderer.GetPosition(0).y;
            endY = LineRenderer.GetPosition(1).y;
        }
        else
        {
            startY = transform.TransformPoint(LineRenderer.GetPosition(0)).y;
            endY = transform.TransformPoint(LineRenderer.GetPosition(1)).y;
        }
        LineRenderer.startColor = TransColorByHeightDifference(_startColor, startY);
        LineRenderer.endColor = TransColorByHeightDifference(_endColor, endY);
        LineRenderer.sortingOrder = GetOrderInLayer(Mathf.Max(startY, endY));
    }
}
