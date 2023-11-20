 using UnityEngine;
 using System.Collections;

namespace ScifiWarehouse
{
    [DisallowMultipleComponent]
    [RequireComponent (typeof(AudioSource))]
    public class PlayRandomSound : MonoBehaviour
    {
        public bool autoLoopOnAwake;
        public int autoLoopDelay = 5;
        public AudioClip[] audioSources;
        public AudioSource AudioSource { get; private set; }

        private void Start()
        {
            AudioSource = GetComponent<AudioSource>();
            if (autoLoopOnAwake)
                Invoke(nameof(AudioAutoLoop), autoLoopDelay);
        }
        private void AudioAutoLoop()
        {
            AudioRandomPlay();
            Invoke(nameof(AudioAutoLoop), autoLoopDelay);
        }
        public void AudioRandomPlay()
        {
            AudioSource.clip = audioSources[Random.Range(0, audioSources.Length)];
            AudioSource.Play();
        }
    }
}