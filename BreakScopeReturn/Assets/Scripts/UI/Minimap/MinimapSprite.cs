using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class MinimapSprite : MinimapParts
{
    [SerializeField]
    Color _color;
    public SpriteRenderer SpriteRenderer { get; private set; }
    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        SpriteRenderer.color = TransColorByHeightDifference(_color);
        SpriteRenderer.sortingOrder = GetOrderInLayer();
    }
}
