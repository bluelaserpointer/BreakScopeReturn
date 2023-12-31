using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectGrenade : MonoBehaviour
{
    public float velocityPerDistance;
    public Grenade grenadePrefab;
    public Transform throwAnchor;

    [Header("TrajectoryDraw")]
    [SerializeField]
    LineRenderer _trajectoryLine;
    [SerializeField]
    int _trajectoryDrawMaxFrames;

    Player Player => GameManager.Instance.Player;

    private void Update()
    {
        Transform cameraTransform = Player.Camera.transform;
        Vector3 throwVector = cameraTransform.forward * velocityPerDistance * Vector3.Distance(cameraTransform.position, Player.AimPosition);
        /*
        if (Input.GetKey(KeyCode.G))
        {
            _trajectoryLine.enabled = true;
            List<Vector3> positions = new List<Vector3>();
            Vector3 position = throwAnchor.position;
            Vector3 velocity = throwVector;
            for (var i = 0; i < _trajectoryDrawMaxFrames; i++)
            {
                Grenade.ProjectileUpdate(grenadePrefab, ref position, ref velocity, grenadePrefab.SphereCollider.radius, grenadePrefab.SphereCollider.sharedMaterial);
                positions.Add(position);
            }
            _trajectoryLine.positionCount = positions.Count;
            _trajectoryLine.SetPositions(positions.ToArray());
        }
        else if (Input.GetKeyUp(KeyCode.G))
        {
            _trajectoryLine.enabled = false;
            Throw(GenerateGrenade(), throwVector);
        }*/
    }
    public Grenade GenerateGrenade()
    {
        return Instantiate(grenadePrefab, GameManager.Instance.CurrentStage.transform);
    }
    public void Throw(Grenade grenade, Vector3 initialVelocity)
    {
        grenade.transform.SetPositionAndRotation(throwAnchor.position, throwAnchor.transform.rotation);
        grenade.velocity = initialVelocity;
        Player.GunInventory.Hands.Animator.SetTrigger("toss");
    }
}
