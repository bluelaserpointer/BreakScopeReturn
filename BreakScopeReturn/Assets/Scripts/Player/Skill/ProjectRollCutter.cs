using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectRollCutter : MonoBehaviour
{
    [SerializeField]
    RollCutter _rollCutterPrefab;

    [SerializeField] GameObject placeGuide;

    bool isPlacing;
    Transform placeTransform;
    Vector3 placePosition;
    Vector3 placeNormal;

    RollCutter generatedCutter;

    private Player Player => GameManager.Instance.Player;

    private void Awake()
    {
        placeGuide.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            if (!isPlacing)
            {
                placeGuide.gameObject.SetActive(true);
                isPlacing = true;
            }
            Transform cameraTf = GameManager.Instance.Player.Camera.transform;
            Ray cameraRay = new Ray(cameraTf.position, cameraTf.forward);
            RaycastHit closestHit = new();
            closestHit.distance = float.MaxValue;
            foreach (var hitInfo in Physics.RaycastAll(cameraRay))
            {
                if (hitInfo.collider.isTrigger || hitInfo.collider.GetComponentInParent<Player>() != null)
                    continue;
                if (hitInfo.distance < closestHit.distance)
                {
                    closestHit = hitInfo;
                }
            }
            placeTransform = closestHit.collider.transform;
            placePosition = closestHit.point + closestHit.normal * 0.001F;
            placeNormal = closestHit.normal;
            placeGuide.transform.position = placePosition + placeNormal * 0.1F;
            placeGuide.transform.forward = -placeNormal;
        }
        else if (isPlacing)
        {
            isPlacing = false;
            placeGuide.gameObject.SetActive(false);
            SpawnMirrorKnife(placeTransform, placePosition, placeNormal);
        }
    }
    public void SpawnMirrorKnife(Transform nest, Vector3 position, Vector3 normal)
    {
        if (generatedCutter)
            Destroy(generatedCutter.gameObject);
        generatedCutter = Instantiate(_rollCutterPrefab);
        generatedCutter.enabled = false;
        generatedCutter.transform.SetParent(nest);
        generatedCutter.StickyProjectile.hitCondition = collider => !Player.IsMyCollider(collider);
        generatedCutter.transform.position = GameManager.Instance.Player.Camera.transform.position;
        generatedCutter.StickyProjectile.Eject(GameManager.Instance.Player.Camera.transform.forward * 40);
        generatedCutter.StickyProjectile.onHit.AddListener(hitInfo =>
        {
            generatedCutter.enabled = true;
            generatedCutter.transform.rotation = 
                Quaternion.LookRotation(hitInfo.normal, Vector3.ProjectOnPlane(generatedCutter.transform.forward, hitInfo.normal));
        });
    }
}
