using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float hitNoiseDistance = 20;
	[Tooltip("Prefab of wall damange hit. The object needs 'LevelPart' tag to create decal on it.")]
	public GameObject decalHitWall;
	[Tooltip("Decal will need to be sligtly infront of the wall so it doesnt cause rendeing problems so for best feel put from 0.01-0.1.")]
	public float floatInfrontOfWall;
	[Tooltip("Put Weapon layer and Player layer to ignore bullet raycast.")]
	public LayerMask ignoreLayer;

    [HideInInspector]
    public float damage;
    [HideInInspector]
    public float speed;
    RicochetMirror latestRicochetMirror;
    public Rigidbody Rigidbody { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }
    /*
	* Uppon bullet creation with this script attatched,
	* bullet creates a raycast which searches for corresponding tags.
	* If raycast finds somethig it will create a decal of corresponding tag.
	*/
    void FixedUpdate () {
		Travel();
	}
    public void Travel()
    {
        RaycastHit closestValidHit = new();
        closestValidHit.distance = float.MaxValue;
        RicochetMirror validMirror = null;
        foreach (var hit in Physics.RaycastAll(transform.position, transform.forward, speed * Time.fixedDeltaTime, ~ignoreLayer))
        {
            if (hit.distance > closestValidHit.distance)
                continue;
            bool isCandidate = false;
            if (hit.collider.TryGetComponent(out DamageCollider damageCollider))
            {
                isCandidate = true;
            }
            else
            {
                RicochetMirror mirror = hit.collider.GetComponentInParent<RicochetMirror>();
                if (mirror == null)
                {
                    isCandidate = !hit.collider.isTrigger;
                }
                else if (mirror != latestRicochetMirror)
                {
                    isCandidate = true;
                    validMirror = mirror;
                }
            }
            if (isCandidate)
                closestValidHit = hit;
        }
        if (closestValidHit.collider != null)
        {
            transform.position = closestValidHit.point;
            SoundSource hitNoise = new SoundSource(transform.position, hitNoiseDistance);
            hitNoise.emergence = true;
            SoundSource.MakeSound(hitNoise);
            if (validMirror != null)
            {
                latestRicochetMirror = validMirror;
                transform.forward = Vector3.Reflect(transform.forward, closestValidHit.normal);
            }
            else if (closestValidHit.collider.TryGetComponent(out DamageCollider damageCollider))
            {
                damageCollider.Hit(this);
                Destroy(gameObject);
            }
            else
            {
                if (decalHitWall)
                {
                    GameObject generatedEffect = Instantiate(decalHitWall, closestValidHit.point + closestValidHit.normal * floatInfrontOfWall, Quaternion.LookRotation(closestValidHit.normal));
                    generatedEffect.transform.SetParent(closestValidHit.collider.transform);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            transform.position += transform.forward * speed * Time.fixedDeltaTime;
        }
    }

}
