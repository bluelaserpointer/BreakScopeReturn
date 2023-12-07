using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RicochetMirror : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] float mirrorSize;
    [SerializeField] SmoothDampTransition _expanding = new SmoothDampTransition(0.5F);

    [Header("SE")]
    [SerializeField] HitSound _hitSound;

    [Header("Flat Check")]
    [SerializeField] float _flatCheckHeight;
    [SerializeField] float _flatCheckDistance;

    [Header("Internal Reference")]
    [SerializeField] GameObject renderParent;
    [SerializeField] MirrorScript mirrorScript;
    [SerializeField] GameObject expandAnchor;

    public HitSound HitSound => _hitSound;

    [HideInInspector]
    public bool expand;
    private Transform PlayerCameraPose => GameManager.Instance.Player.Camera.transform;

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
        }
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
            bool leftFlat = FlatCheck(hitInfo, _flatCheckDistance * new Vector2(-1, 0), out bool leftHit);
            bool rightFlat = FlatCheck(hitInfo, _flatCheckDistance * new Vector2(1, 0), out bool rightHit);
            bool downFlat = FlatCheck(hitInfo, _flatCheckDistance * new Vector2(0, -1), out bool downHit);
            bool upFlat = FlatCheck(hitInfo, _flatCheckDistance * new Vector2(0, 1), out bool upHit);
            if ((leftFlat || rightFlat) && (downFlat || upFlat)) //Valid flat surface
            {
                foundValidHit = true;
                renderParent.SetActive(true);
                transform.position = hitInfo.point;
                transform.forward = hitInfo.normal;
                break;
            }
            if (leftHit && rightHit && downHit && upHit) { //Interrupt searching
                break;
            }
        }
        if (!foundValidHit)
            renderParent.SetActive(false);
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
}
