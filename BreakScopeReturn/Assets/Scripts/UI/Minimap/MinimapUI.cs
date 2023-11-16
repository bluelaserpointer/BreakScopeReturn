using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MinimapUI : MonoBehaviour
{
    [SerializeField]
    Camera _minimapCamera;
    [SerializeField]
    Vector3 _cameraOffset;
    [SerializeField]
    float _cameraSize;
    [SerializeField]
    RawImage _minimapRTViewer;
    [SerializeField]
    LineRenderer _aimLine;

    private void Update()
    {
        _minimapRTViewer.transform.eulerAngles = Vector3.forward * GameManager.Instance.Player.Camera.transform.eulerAngles.y;
        _minimapCamera.transform.position = GameManager.Instance.Player.transform.position + _cameraOffset;
        _minimapCamera.orthographicSize = _cameraSize;
    }
    public void SetAimLine(params Vector3[] positions)
    {
        _aimLine.positionCount = positions.Length;
        for (int i = 0; i < positions.Length; ++i)
        {
            Vector3 position = positions[i];
            _aimLine.SetPosition(i, new Vector3(position.x, 0, position.z));
        }
    }
    public void SetAimLineVisibility(bool cond)
    {
        _aimLine.enabled = cond;
    }
}
