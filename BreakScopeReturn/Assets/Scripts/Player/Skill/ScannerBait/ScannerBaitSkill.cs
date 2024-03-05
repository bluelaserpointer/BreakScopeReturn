using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ScannerBaitSkill : MonoBehaviour
{
    [SerializeField]
    ScannerBaitProjectile _baitPrefab;
    [SerializeField]
    AudioSource _activationSE;
    [SerializeField]
    SkillSlot _skillSlot;

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
    }
    public void LaunchBait()
    {
        if (_activationSE != null)
            _activationSE.Play();
        var bait = Instantiate(_baitPrefab, GameManager.Instance.Stage.transform);
        bait.transform.SetPositionAndRotation(GameManager.Instance.Player.Camera.transform);
    }
}
