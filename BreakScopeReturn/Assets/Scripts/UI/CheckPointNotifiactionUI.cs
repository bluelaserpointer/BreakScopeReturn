using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class CheckPointNotifiactionUI : MonoBehaviour
{
    [SerializeField]
    float fadeInTime;
    [SerializeField]
    float fadeHoldTime;
    [SerializeField]
    float fadeOutTime;

    CanvasGroup canvasGroup;
    float enabledTime;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    private void OnEnable()
    {
        enabledTime = Time.time;
    }
    private void Update()
    {
        float passedTime = Time.time - enabledTime;
        if (passedTime < fadeInTime)
        {
            canvasGroup.alpha = passedTime / fadeInTime;
            return;
        }
        passedTime -= fadeInTime;
        if (passedTime < fadeHoldTime)
        {
            canvasGroup.alpha = 1;
            return;
        }
        passedTime -= fadeHoldTime;
        if (passedTime < fadeOutTime)
        {
            canvasGroup.alpha = 1 - passedTime / fadeOutTime;
            return;
        }
        gameObject.SetActive(false);
        
    }
}
