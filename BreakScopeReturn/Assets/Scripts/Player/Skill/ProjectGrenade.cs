using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectGrenade : MonoBehaviour
{
    public float force;
    public Grenade grenadePrefab;
    public Transform throwAnchor;

    [SerializeField]
    Projection _trajectoryDrawer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Grenade ghostGrenade = GenerateGrenade();
            _trajectoryDrawer.SimulateTrajectory(ghostGrenade.gameObject, () =>
            {
                ghostGrenade.isGhost = true;
                Throw(ghostGrenade);
            });
        }
        else if (Input.GetKeyUp(KeyCode.G))
        {
            Throw(GenerateGrenade());
        }
    }
    public Grenade GenerateGrenade()
    {
        return Instantiate(grenadePrefab, GameManager.Instance.CurrentStage.transform);
    }
    public void Throw(Grenade grenade)
    {
        grenade.transform.SetPositionAndRotation(throwAnchor.position, throwAnchor.transform.rotation);
        grenade.Rigidbody.AddForce(GameManager.Instance.Player.Camera.transform.forward * force);
    }
}
