using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RicochetMirror : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] float mirrorSize;
    [SerializeField] SmoothDampTransition _expanding = new SmoothDampTransition(0.5F);
    //[SerializeField] float maxAlphaDistance = 0.5F;
    //[SerializeField] AnimationCurve _alphaCurveByDistance;

    [Header("SE")]
    [SerializeField] HitSound _hitSound;

    [Header("Flat Check")]
    [SerializeField] float _flatCheckHeight;
    [SerializeField] float _flatCheckDistance;

    [Header("Internal Reference")]
    [SerializeField] Collider _mirrorPlaneCollider;
    [SerializeField] GameObject renderParent;
    [SerializeField] MirrorScript mirrorScript;
    [SerializeField] GameObject expandAnchor;

    public Collider MirrorPlaneCollider => _mirrorPlaneCollider;
    public HitSound HitSound => _hitSound;

    [HideInInspector]
    [NonSerialized]
    public bool expand;
    private Transform PlayerCameraPose => GameManager.Instance.Player.Camera.transform;

    public static bool IsRicochetMirrorCollider(Collider collider)
    {
        return collider.CompareTag(nameof(RicochetMirror));
    }
    public static bool TryGetRicochetMirror(Collider collider, out RicochetMirror mirror)
    {
        if (!IsRicochetMirrorCollider(collider))
        {
            mirror = null;
            return false;
        }
        mirror = collider.GetComponentInParent<RicochetMirror>();
        return true; 
    }
    public void Init()
    {
        mirrorScript.cameraLookingAtThisMirror = GameManager.Instance.Player.Camera;
    }
    private void Update()
    {
        _expanding.SmoothTowards(expand ? 1 : 0);
        if (_expanding.NearZero)
        {
            renderParent.SetActive(false);
        }
        else
        {
            UpdatePose();
            UpdateAlpha();
        }
    }
    public void CloseImmediate()
    {
        expand = false;
        _expanding.value = 0;
        renderParent.SetActive(false);
    }
    private void UpdatePose()
    {
        expandAnchor.transform.localScale = new Vector3(1, 1, 0) * _expanding.Lerp(0, mirrorSize) + Vector3.forward;
        List<RaycastHit> hits = new List<RaycastHit>();
        foreach (var hitInfo in Physics.RaycastAll(PlayerCameraPose.position, PlayerCameraPose.forward))
        {
            if (hitInfo.collider.isTrigger || transform.IsChildOf(hitInfo.transform))
                continue;
            int i = 0;
            for (; i < hits.Count; i++)
            {
                if (hitInfo.distance < hits[i].distance)
                    break;
            }
            hits.Insert(i, hitInfo);
        }
        bool foundValidHit = false;
        foreach (var hitInfo in hits)
        {
            if (hitInfo.collider.TryGetComponent<RicochetMirrorIgnoreLayer>(out var ignoreLayer))
            {
                foundValidHit = !ignoreLayer.ignore;
            }
            else if (_flatCheckDistance > 0)
            {
                bool leftFlat = FlatCheck(hitInfo, _flatCheckDistance * new Vector2(-1, 0), out bool leftHit);
                bool rightFlat = FlatCheck(hitInfo, _flatCheckDistance * new Vector2(1, 0), out bool rightHit);
                bool downFlat = FlatCheck(hitInfo, _flatCheckDistance * new Vector2(0, -1), out bool downHit);
                bool upFlat = FlatCheck(hitInfo, _flatCheckDistance * new Vector2(0, 1), out bool upHit);
                if (leftHit && rightHit && downHit && upHit)
                    break; //TODO: Improve this behavior 
                foundValidHit = (leftFlat || rightFlat) && (downFlat || upFlat);
            }
            else
            {
                foundValidHit = true;
            }
            if (foundValidHit)
            {
                renderParent.SetActive(true);
                transform.position = hitInfo.point;
                transform.forward = hitInfo.normal;
                break;
            }
        }
        if (!foundValidHit)
            renderParent.SetActive(false);
    }
    /// <summary>
    /// 1. Decrease alpha when player camera get close to mirror.
    /// 2. Using depth texture to reshape mirror plane match within the back-object's flat surface
    /// </summary>
    private void UpdateAlpha()
    {
        Vector3 cameraPosition = GameManager.Instance.Player.Camera.transform.position;
        /*
        float playerDistance = Vector3.Distance(transform.position, cameraPosition);
        float mirrorAlpha;
        if (playerDistance > maxAlphaDistance)
        {
            mirrorAlpha = 1;
        }
        else
        {
            mirrorAlpha = _alphaCurveByDistance.Evaluate(playerDistance / maxAlphaDistance);
        }
        Color mirrorColor = mirrorScript.MirrorMaterial.color;
        mirrorColor.a = mirrorAlpha;
        mirrorScript.MirrorMaterial.color = mirrorColor;
        */
    }
    private bool FlatCheck(RaycastHit raycastHit, Vector2 extend, out bool hitSurface)
    {
        Vector3 extend3d;
        Vector3 rightVector = Vector3.Cross(raycastHit.normal, Vector3.up).normalized;
        Vector3 upVector = Vector3.Cross(raycastHit.normal, rightVector).normalized;
        if (rightVector.Equals(Vector3.zero))
        {
            extend3d = new Vector3() { x = extend.x, z = extend.y };
        }
        else
        {
            extend3d = rightVector * extend.x + upVector * extend.y;
        }
        Ray ray = new Ray(raycastHit.point + extend3d + raycastHit.normal * _flatCheckHeight, -raycastHit.normal);
        if(raycastHit.collider.Raycast(ray, out RaycastHit hitInfo, float.MaxValue))
        {
            hitSurface = true;
            if (Mathf.Abs(hitInfo.distance - _flatCheckHeight) < _flatCheckHeight * 0.01)
                return true;
        }
        hitSurface = false;
        return false;
    }
    public void SetCameraLookingAtThisMirror(Camera camera, bool immediateRender = false)
    {
        mirrorScript.cameraLookingAtThisMirror = camera;
        if (immediateRender)
        {
            mirrorScript.UpdateCameraProperties();
            mirrorScript.RenderMirror();
        }
    }
    public Plane GetMirrorPlane()
    {
        return new Plane(transform.forward, transform.position);
    }
}
