using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BulletMirrorSkill : MonoBehaviour
{
    [SerializeField]
    RicochetMirror _mirrorPrefab;
    [SerializeField]
    AudioSource _moduleAudioSource;

    [Header("UI")]
    [SerializeField] SkillSlot _skillSlot;

    public RicochetMirror Mirror { get; private set; }
    public bool MirrorExpanded => Mirror.expand;


    private void Start()
    {
        Mirror = Instantiate(_mirrorPrefab);
        Mirror.Init();
    }
    private void Update()
    {
        if (!GameManager.Instance.Player.AIEnable)
            return;
        bool keyInput = Input.GetKeyDown(KeyCode.Q);
        if (keyInput)
            SetMirror(!MirrorExpanded);
        _skillSlot.ActivateLit(Mirror.expand);
    }
    public void SetMirror(bool cond)
    {
        if (Mirror.expand == cond)
            return;
        _moduleAudioSource.Play();
        Mirror.expand = cond;
    }
    public void CloseMirrorImmediate()
    {
        Mirror.CloseImmediate();
    }
}
