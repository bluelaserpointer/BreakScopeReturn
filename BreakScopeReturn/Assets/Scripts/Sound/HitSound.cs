using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitSound : IComponentInterface
{
    public SoundSetSO HitSoundSet { get; }
}
[DisallowMultipleComponent]
public class HitSound : MonoBehaviour, IHitSound
{
    [SerializeField]
    SoundSetSO _soundSet;

    public SoundSetSO HitSoundSet => _soundSet;

    public AudioClip GetRandomClip()
    {
        return _soundSet.GetRandomClip();
    }
}
