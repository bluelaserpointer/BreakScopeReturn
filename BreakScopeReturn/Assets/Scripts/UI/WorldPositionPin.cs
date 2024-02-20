using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class WorldPositionPin : MonoBehaviour
{
    public Transform target;
    public enum PinMode { TransformPivot, ColliderBoundsCenter, RigidbodyCenterOfMass, Unit }
    public PinMode pinMode;

    [Header("Reference")]
    [SerializeField]
    RectTransform _graphicRoot;
    [SerializeField]
    RectTransform _normalCircle;
    [SerializeField]
    TextMeshProUGUI _normalTargetDistanceText;
    [SerializeField]
    RectTransform _inMirrorCircle;
    [SerializeField]
    TextMeshProUGUI _inMirrorTargetDistanceText;

    public Vector3 PinPosition { get; private set; }
    private Camera PlayerCamera => GameManager.Instance.Player.Camera;

    public void Init(Transform target, PinMode pinMode)
    {
        this.target = target;
        this.pinMode = pinMode;
    }
    private void Update()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }
        if (!target.gameObject.activeInHierarchy)
        {
            _graphicRoot.gameObject.SetActive(false);
            return;
        }
        _graphicRoot.gameObject.SetActive(true);
        UpdatePinPosition();
        UpdateNormalPin();
        UpdateInMirrorPin();
    }
    private void UpdatePinPosition()
    {
        switch (pinMode)
        {
            case PinMode.TransformPivot:
                PinPosition = target.position;
                break;
            case PinMode.ColliderBoundsCenter:
                PinPosition = target.GetComponent<Collider>().bounds.center;
                break;
            case PinMode.RigidbodyCenterOfMass:
                PinPosition = target.GetComponent<Rigidbody>().centerOfMass;
                break;
            case PinMode.Unit:
                if (target.GetComponent<Unit>().IsDead)
                {
                    gameObject.SetActive(false); //reuse is managed by ObjectiveUI
                    return;
                }
                PinPosition = target.GetComponent<Unit>().PinCentre.position;
                break;
            default:
                return;
        }
    }
    private void UpdateNormalPin()
    {
        Vector3 playerCameraPosition = PlayerCamera.transform.position;
        _normalCircle.position = PlayerCamera.WorldToScreenPoint(PinPosition);
        if (_normalCircle.position.z < 0)
        {
            _normalCircle.gameObject.SetActive(false);
            return;
        }
        _normalCircle.gameObject.SetActive(true);
        _normalTargetDistanceText.text = (int)Vector3.Distance(playerCameraPosition, PinPosition) + "m";
    }
    private void UpdateInMirrorPin()
    {
        Vector3 playerCameraPosition = PlayerCamera.transform.position;
        RicochetMirror mirror = GameManager.Instance.Player.ProjectRicochetMirror.Mirror;
        Plane mirrorPlane = mirror.GetMirrorPlane();
        if (!mirrorPlane.GetSide(PinPosition))
        {
            _inMirrorCircle.gameObject.SetActive(false);
            return;
        }
        Vector3 pinVirtualPosition = PinPosition - mirrorPlane.normal * mirrorPlane.GetDistanceToPoint(PinPosition) * 2;
        if (!mirror.MirrorPlaneCollider.Raycast(new Ray(playerCameraPosition, pinVirtualPosition - playerCameraPosition)
            , out RaycastHit hitInfo, float.MaxValue))
        {
            _inMirrorCircle.gameObject.SetActive(false);
            return;
        }
        _inMirrorCircle.gameObject.SetActive(true);
        _inMirrorCircle.transform.position = PlayerCamera.WorldToScreenPoint(hitInfo.point);
        _inMirrorTargetDistanceText.text = (int)Vector3.Distance(playerCameraPosition, pinVirtualPosition) + "m"; //TODO: this info could be removed
    }
}
