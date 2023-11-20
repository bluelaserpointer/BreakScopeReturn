using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectRicochetMirror : MonoBehaviour
{
    [SerializeField]
    RicochetMirror _mirror;
    [SerializeField]
    AudioSource _moduleAudioSource;

    public RicochetMirror Mirror => _mirror;
    public bool MirrorExpanded => _mirror.expand;


    private void Start()
    {
        _mirror.Init();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            SetMirror(!MirrorExpanded);
    }
    public void SetMirror(bool cond)
    {
        if (_mirror.expand == cond)
            return;
        _moduleAudioSource.Play();
        _mirror.expand = cond;
    }
}
