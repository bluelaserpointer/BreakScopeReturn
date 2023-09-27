using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IzumiTools
{
    //[CreateAssetMenu(fileName = "SnapMoveToolSettings", menuName = "ScriptableObjects/SnapMoveToolSettings", order = 1)]
    public class DuctSnapToolSettings : ScriptableObject
    {
        public UpAxis UpAxis = UpAxis.Y;
        public float SnapRadius = 0.5f;
        public float WorldBaseY = 10f;
        public float DepthOffset = 0f;
        public bool AddObjectBoundsToDepthOffset = false;
    }

    public enum UpAxis
    {
        X = 0,
        Y = 1,
        Z = 2,
        NegativeX = 3,
        NegativeY = 4,
        NegativeZ = 5,
    }
}

