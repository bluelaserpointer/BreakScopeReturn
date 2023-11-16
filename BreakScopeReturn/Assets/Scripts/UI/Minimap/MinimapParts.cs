using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MinimapParts : MonoBehaviour
{
    [SerializeField]
    int _originalOrderInLayer;
    public int OriginalOrderInLayer => _originalOrderInLayer;

    protected Color TransColorByHeightDifference(Color color)
    {
        return TransColorByHeightDifference(color, transform.position.y);
    }
    protected Color TransColorByHeightDifference(Color color, float height)
    {
        return TransColorByHeightDifference(color, height, height);
    }
    protected Color TransColorByHeightDifference(Color color, float topY, float bottomY)
    {
        float playerY = GameManager.Instance.Player.FootPosition.y;
        float heightDiff;
        bool isLowerThenPlayer;
        if (topY < playerY)
        {
            heightDiff = playerY - topY;
            isLowerThenPlayer = true;
        }
        else if (playerY < bottomY)
        {
            heightDiff = playerY - bottomY;
            isLowerThenPlayer = false;
        }
        else
        {
            return color;
        }
        float verticalVisibleDistance = isLowerThenPlayer ? MinimapSetting.CurrentSetting.DownVisibleDistance : MinimapSetting.CurrentSetting.UpVisibleDistance;
        float linearValue = Mathf.Clamp01((verticalVisibleDistance - Mathf.Abs(heightDiff)) / verticalVisibleDistance);
        float easedValue = Mathf.Pow(linearValue, MinimapSetting.CurrentSetting.VerticalVisibilityEase);
        if (isLowerThenPlayer)
        {
            float alpha = color.a;
            color *= easedValue;
            color.a = alpha;
            return color;
        }
        else
        {
            color.a *= easedValue;
            return color;
        }
    }
    protected int GetOrderInLayer()
    {
        return GetOrderInLayer(transform.position.y);
    }
    protected int GetOrderInLayer(float topY)
    {
        return OriginalOrderInLayer + (int)(topY / MinimapSetting.CurrentSetting.VerticalOrderInLayerUnitDistance);
    }
}
