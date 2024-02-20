using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

[DisallowMultipleComponent]
[RequireComponent(typeof(VisualEffect))]
public class RepeatVFXWithEvent : MonoBehaviour
{
    public float intervalMin, intervalMax;
    public VisualEffect VisualEffect { get; private set; }
    float waitedTime;
    float nextInterval;

    public SoundSetSO soundSet;
    public AudioSource seSource;
    public UnityEvent onVFXPlay;
    private void Awake()
    {
        VisualEffect = GetComponent<VisualEffect>();
        waitedTime = 0;
        nextInterval = Random.Range(intervalMin, intervalMax);
    }
    private void Update()
    {
        if ((waitedTime += Time.deltaTime) > nextInterval)
        {
            waitedTime = 0;
            nextInterval = Random.Range(intervalMin, intervalMax);
            PlayVFX();
        }
    }
    public void PlayVFX()
    {
        VisualEffect.Play();
        onVFXPlay.Invoke();
        if (soundSet != null && soundSet.clips.Length > 0)
        {
            if (seSource)
            {
                seSource.clip = soundSet.GetRandomClip();
                seSource.Play();
            }
            else
            {
                AudioSource.PlayClipAtPoint(soundSet.GetRandomClip(), transform.position);
            }
        }
    }
}
