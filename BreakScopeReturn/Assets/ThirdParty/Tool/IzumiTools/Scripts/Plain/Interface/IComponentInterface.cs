using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automatically link component members (GameObject, Transform) for a child interface 
/// </summary>
public interface IComponentInterface
{
#pragma warning disable IDE1006 // name style
    Transform transform { get; }
    GameObject gameObject { get; }
#pragma warning restore IDE1006 // name style
}
