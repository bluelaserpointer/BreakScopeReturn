using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectRicochetMirror : MonoBehaviour
{
    [SerializeField]
    RicochetMirror _mirror;

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
        _mirror.expand = cond;
    }
}
