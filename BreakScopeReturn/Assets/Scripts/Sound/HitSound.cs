using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HitSound : MonoBehaviour
{
    [SerializeField]
    SoundSetSO _soundSet;

    public AudioClip GetRandomClip()
    {
        return _soundSet.GetRandomClip();
    }
}
