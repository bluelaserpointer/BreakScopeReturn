using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "BreakScope/Audio/SoundSet", fileName = "NewSoundSet")]
public class SoundSetSO : ScriptableObject
{
    public AudioClip[] clips;
    public AudioClip GetRandomClip()
    {
        return clips.Length == 0 ? null : clips[Random.Range(0, clips.Length - 1)];
    }
}
