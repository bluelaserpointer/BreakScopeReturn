using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class MinimapLine : MinimapParts
{
    [SerializeField]
    Color _color;
    [SerializeField]
    Collider _verticalSizeReferenceCollider;
    public LineRenderer LineRenderer { get; private set; }

    private void Awake()
    {
        LineRenderer = GetComponent<LineRenderer>();
    }
    private void Update()
    {
        float topY, bottomY;
        if (_verticalSizeReferenceCollider)
        {
            float playerHeight = GameManager.Instance.Player.Movement.CharacterController.height;
            topY = _verticalSizeReferenceCollider.bounds.max.y - playerHeight;
            bottomY = _verticalSizeReferenceCollider.bounds.min.y - playerHeight;
        }
        else
        {
            topY = bottomY = transform.position.y;
        }
        LineRenderer.startColor = LineRenderer.endColor = TransColorByHeightDifference(_color, topY, bottomY);
        LineRenderer.sortingOrder = GetOrderInLayer(topY);
    }
}
