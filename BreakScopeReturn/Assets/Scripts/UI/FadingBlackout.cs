using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class FadingBlackout : MonoBehaviour
{
    public SmoothDampTransition alphaTransition;
    public Action endAction;
    public bool FadeIn { get; private set; }
    public CanvasGroup CanvasGroup { get; private set; }

    private void Awake()
    {
        CanvasGroup = GetComponent<CanvasGroup>();
        alphaTransition.value = CanvasGroup.alpha;
        FadeIn = CanvasGroup.alpha == 1;
    }
    void Update()
    {
        float targetAlpha = FadeIn ? 1 : 0;
        alphaTransition.SmoothTowards(targetAlpha);
        alphaTransition.Round();
        if (endAction != null && alphaTransition.value == targetAlpha)
        {
            endAction.Invoke();
            endAction = null;
        }
        CanvasGroup.alpha = alphaTransition.value;
    }
    public void SetFadeIn(bool cond, Action endAction = null)
    {
        FadeIn = cond;
        this.endAction = endAction;
    }
    public void SetInstantChange(bool cond)
    {
        FadeIn = cond;
        alphaTransition.value = FadeIn ? 1 : 0;
    }
}
