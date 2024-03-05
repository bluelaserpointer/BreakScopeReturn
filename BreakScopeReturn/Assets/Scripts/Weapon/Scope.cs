using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class Scope : MonoBehaviour
{
    public Camera Camera {  get; private set; }

    private Camera _playerCamera;

    private void Awake()
    {
        Camera = GetComponent<Camera>();
    }
    private void Start()
    {
        _playerCamera = GameManager.Instance.Player.Camera;
    }
    private void Update()
    {
        Camera.transform.rotation = Quaternion.LookRotation(Camera.transform.position - _playerCamera.transform.position);
        //memo: onprecull deprecated in urp
        //also, put delegate into RenderPipelineManager OnBeginCameraRendering which contains Camera.Render() call causes error.
        OnBeginCameraRendering();
        Camera.Render();
        OnEndCameraRendering();
    }
    void OnBeginCameraRendering()
    {
        RicochetMirror mirror = GameManager.Instance.Player.GetComponentInChildren<BulletMirrorSkill>().Mirror;
        if (mirror == null)
        {
            return;
        }
        mirror.SetCameraLookingAtThisMirror(Camera, true);
    }
    private void OnEndCameraRendering()
    {
        RicochetMirror mirror = GameManager.Instance.Player.GetComponentInChildren<BulletMirrorSkill>().Mirror;
        if (mirror == null)
        {
            return;
        }
        mirror.SetCameraLookingAtThisMirror(_playerCamera, true);
    }
}
