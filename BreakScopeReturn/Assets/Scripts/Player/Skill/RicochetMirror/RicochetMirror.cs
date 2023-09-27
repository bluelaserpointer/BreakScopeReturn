using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RicochetMirror : MonoBehaviour
{
    [SerializeField] float mirrorSize;
    [SerializeField] float rectExpandLerp;
    [Header("Internal Reference")]
    [SerializeField] MirrorScript mirrorScript;
    [SerializeField] GameObject expandAnchor;

    public void Init(Vector3 normal)
    {
        transform.forward = normal;
        mirrorScript.cameraLookingAtThisMirror = GameManager.Instance.Player.Camera;
        expandAnchor.transform.localScale = Vector3.one * 0.01F;
    }
    private void Update()
    {
        expandAnchor.transform.localScale = Vector3.one * Mathf.Lerp(expandAnchor.transform.localScale.x, mirrorSize, rectExpandLerp * Time.deltaTime);
    }
}
