using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SnapRoot : MonoBehaviour
{
    [SerializeField]
    List<SnapAnchor> _anchors = new List<SnapAnchor>();

    public List<SnapAnchor> Anchors => _anchors;

    private void OnValidate()
    {
        List<SnapAnchor> uncheckedAnchors = new List<SnapAnchor>(_anchors);
        foreach (var anchor in GetComponentsInChildren<SnapAnchor>())
        {
            if (_anchors.Contains(anchor))
            {
                uncheckedAnchors.Remove(anchor);
            }
            else
            {
                _anchors.Add(anchor);
            }
        }
        _anchors.RemoveAll(anchor => uncheckedAnchors.Contains(anchor));
    }
}
