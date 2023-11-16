using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectMeshKnife : MonoBehaviour
{
    [SerializeField] StickyProjectile _knifePrefab;
    [SerializeField] GameObject _placeGuide;

    bool isPlacing;
    Transform placeTransform;
    Vector3 placePosition;
    Vector3 placeNormal;

    StickyProjectile generatedKnife;
    RicochetMirror generatedMirror;

    private Player Player => GameManager.Instance.Player;

    private void Awake()
    {
        _placeGuide.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            if (!isPlacing)
            {
                _placeGuide.gameObject.SetActive(true);
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
            _placeGuide.transform.position = placePosition + placeNormal * 0.1F;
            _placeGuide.transform.forward = -placeNormal;
        }
        else if (isPlacing)
        {
            isPlacing = false;
            _placeGuide.gameObject.SetActive(false);
            SpawnKnife(placeTransform, placePosition, placeNormal);
        }
    }
    public void SpawnKnife(Transform nest, Vector3 position, Vector3 normal)
    {
        if (generatedKnife)
            Destroy(generatedKnife.gameObject);
        if (generatedMirror)
            Destroy(generatedMirror.gameObject);
        generatedKnife = Instantiate(_knifePrefab);
        generatedKnife.transform.SetParent(nest);
        generatedKnife.hitCondition = collider => !Player.IsMyCollider(collider);
        generatedKnife.transform.position = GameManager.Instance.Player.Camera.transform.position;
        generatedKnife.Eject(GameManager.Instance.Player.Camera.transform.forward * 40);
        GameManager.Instance.CurrentStage.OnDestroy.AddListener(() => Destroy(generatedKnife.gameObject));
    }
}
