using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayableDirector))]
public class Cutscene : MonoBehaviour
{
    [SerializeField]
    bool _hideOnAwake;
    [SerializeField]
    UnityEvent _onStop;
    public PlayableDirector PlayableDirector { get; private set; }
    public bool Playing => PlayableDirector.state == PlayState.Playing;
    public bool SkipOrder { get; private set; }
    private void Awake()
    {
        PlayableDirector = GetComponent<PlayableDirector>();
        PlayableDirector.stopped += _ => OnStoppedInternal();
        if (_hideOnAwake)
            gameObject.SetActive(false);
    }
    public void Play()
    {
        GameManager.Instance.CutsceneBlackout.SetFadeIn(true, () =>
        {
            GameManager.Instance.CutsceneBlackout.SetFadeIn(false);
            GameManager.Instance.ActiveCutscene?.Skip();
            GameManager.Instance.ActiveCutscene = this;
            GameManager.Instance.Player.gameObject.SetActive(false);
            GameManager.Instance.MinimapUI.gameObject.SetActive(false);
            gameObject.SetActive(true);
            SkipOrder = false;
            GameManager.Instance.CutsceneUI.SetActive(true);
            PlayableDirector.Play();
        });
    }
    public void Skip()
    {
        SkipOrder = true;
        PlayableDirector.Stop();
        GameManager.Instance.ActiveCutscene = null;
    }
    private void OnStoppedInternal()
    {
        _onStop.Invoke();
        gameObject.SetActive(false);
        if (GameManager.Instance.CutsceneUI)
            GameManager.Instance.CutsceneUI.SetActive(false);
        //TODO: detect application quit
        if (GameManager.Instance.Player != null)
        {
            GameManager.Instance.Player.gameObject.SetActive(true);
        }
        if (GameManager.Instance.MinimapUI != null)
        {
            GameManager.Instance.MinimapUI.gameObject.SetActive(true);
        }
    }
}
