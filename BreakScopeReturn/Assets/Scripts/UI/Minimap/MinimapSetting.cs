using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BreakScope/Minimap/Setting")]
public class MinimapSetting : ScriptableObject
{
    [SerializeField]
    float _downVisibleDistance = 18;
    [SerializeField]
    float _upVisibleDistance = 6;
    [SerializeField]
    float _verticalOrderInLayerUnitDistance = 1;
    [SerializeField]
    float _verticalVisibilityEase = 2;
    public float DownVisibleDistance => _downVisibleDistance;
    public float UpVisibleDistance => _upVisibleDistance;
    public float VerticalOrderInLayerUnitDistance => _verticalOrderInLayerUnitDistance;
    public float VerticalVisibilityEase => _verticalVisibilityEase;
    static MinimapSetting _currentSetting;
    public static MinimapSetting CurrentSetting => _currentSetting ?? (_currentSetting = Resources.Load<MinimapSetting>("Setting/Minimap/MinimapSetting"));
}
