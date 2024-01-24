using UnityEngine;
using UnityEngine.VFX;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public SoundSetSO defaultObstacleHitSoundSet;
    public SoundSetSO defaultBodyHitSoundSet;
    public float hitNoiseDistance = 20;
    [SerializeField]
    GameObject _reinstanceOnReflection;
    [SerializeField]
    Transform _detachBeforeDestory;
    [SerializeField]
    GameObject _destoryVFXPrefab;
    [SerializeField]
    GameObject _reflectVFXPrefab;
    [SerializeField]
    GameObject _bodyHitVFXPrefab;
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
    int _travelPauseForDebug;
    public void Travel()
    {
        if (_travelPauseForDebug > 0)
        {
            _travelPauseForDebug--;
            return;
        }
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
                if (damageCollider.Ignore)
                    continue;
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
                    AudioSource.PlayClipAtPoint(mirror.HitSound.GetRandomClip(), hit.point);
                }
            }
            if (isCandidate)
                closestValidHit = hit;
        }
        if (closestValidHit.collider != null)
        {
            transform.position = closestValidHit.point;
            //make noise (enemy will hear)
            SoundSource hitNoise = new SoundSource(transform.position, hitNoiseDistance);
            hitNoise.emergence = true;
            SoundSource.MakeSound(hitNoise);
            //make SE
            SoundSetSO hitSESet = null;
            if (closestValidHit.transform.TryGetComponent(out HitSound hitSound))
            {
                hitSESet = hitSound.SoundSet;
            }
            //mirror reflection / damage dealt / decal spawn
            if (validMirror != null)
            {
                latestRicochetMirror = validMirror;
                //detach and reinstance trail visual effect on every reflections
                Transform parent = _reinstanceOnReflection.transform.parent;
                Vector3 pos = _reinstanceOnReflection.transform.localPosition;
                Quaternion rot = _reinstanceOnReflection.transform.localRotation;
                _reinstanceOnReflection.transform.SetParent(transform.parent);
                transform.forward = Vector3.Reflect(transform.forward, closestValidHit.normal);
                GameObject copyOnEveryReflection = Instantiate(_reinstanceOnReflection, parent);
                copyOnEveryReflection.transform.SetLocalPositionAndRotation(pos, rot);
                if (_reflectVFXPrefab)
                {
                    GameObject generatedEffect = Instantiate(_reflectVFXPrefab, closestValidHit.point + closestValidHit.normal * floatInfrontOfWall, Quaternion.LookRotation(closestValidHit.normal));
                    generatedEffect.transform.SetParent(closestValidHit.collider.transform);
                }
            }
            else if (closestValidHit.collider.TryGetComponent(out DamageCollider damageCollider))
            {
                if (hitSESet == null)
                    hitSESet = defaultBodyHitSoundSet;
                damageCollider.Hit(this);
                if (damageCollider.DamageRatio > 0 && _bodyHitVFXPrefab)
                {
                    GameObject generatedEffect = Instantiate(_bodyHitVFXPrefab, closestValidHit.point, Quaternion.LookRotation(closestValidHit.normal));
                    generatedEffect.transform.SetParent(closestValidHit.collider.transform);
                }
                Destroy();
            }
            else
            {
                if (hitSESet == null)
                    hitSESet = defaultObstacleHitSoundSet;
                if (_destoryVFXPrefab)
                {
                    GameObject generatedEffect = Instantiate(_destoryVFXPrefab, closestValidHit.point + closestValidHit.normal * floatInfrontOfWall, Quaternion.LookRotation(closestValidHit.normal));
                    generatedEffect.transform.SetParent(closestValidHit.collider.transform);
                }
                Destroy();
            }
            if (hitSESet != null)
                AudioSource.PlayClipAtPoint(hitSESet.GetRandomClip(), closestValidHit.point);
        }
        else
        {
            transform.position += transform.forward * speed * Time.fixedDeltaTime;
        }
    }
    public void Destroy()
    {
        if (_detachBeforeDestory)
        {
            _detachBeforeDestory.DetachChildren();
            Destroy(_detachBeforeDestory.gameObject);
        }
        Destroy(gameObject);
    }
}
