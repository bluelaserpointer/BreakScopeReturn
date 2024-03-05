using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ShapeScannerSkill : MonoBehaviour
{
    [SerializeField]
    ShapeScannerProjectile _projectilePrefab;
    [SerializeField]
    AudioSource _activationSE;
    [SerializeField]
    SkillSlot _skillSlot;
    [SerializeField]
    Camera _shapeScanCamera;
    [SerializeField]
    Material _shapeScanMaterial;

    ShapeScannerProjectile _activeProjectile;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            _skillSlot.ActivateLit(true);
            LaunchBait();
        }
        else
        {
            _skillSlot.ActivateLit(false);
        }
        if (_activeProjectile != null)
        {
            _shapeScanCamera.enabled = true;
            _shapeScanMaterial.SetVector("_ScanPosition", _activeProjectile.transform.position);
        }
        else
        {
            _shapeScanCamera.enabled = false;
        }
    }
    public void LaunchBait()
    {
        if (_activationSE != null)
            _activationSE.Play();
        _activeProjectile = Instantiate(_projectilePrefab, GameManager.Instance.Stage.transform);
        _activeProjectile.transform.SetPositionAndRotation(GameManager.Instance.Player.Camera.transform);
    }
}
