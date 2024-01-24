using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectRicochetMirror : MonoBehaviour
{
    [SerializeField]
    RicochetMirror _mirrorPrefab;
    [SerializeField]
    AudioSource _moduleAudioSource;

    public RicochetMirror Mirror { get; private set; }
    public bool MirrorExpanded => Mirror.expand;


    private void Start()
    {
        Mirror = Instantiate(_mirrorPrefab);
        Mirror.Init();
    }
    private void Update()
    {
        if (GameManager.Instance.Player.Controllable && Input.GetKeyDown(KeyCode.Q))
            SetMirror(!MirrorExpanded);
    }
    public void SetMirror(bool cond)
    {
        if (Mirror.expand == cond)
            return;
        _moduleAudioSource.Play();
        Mirror.expand = cond;
    }
}
