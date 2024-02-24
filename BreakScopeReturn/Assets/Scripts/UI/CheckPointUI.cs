using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class CheckPointUI : MonoBehaviour
{
    [SerializeField]
    Animator _animator;

    public void ReachNewCheckPoint()
    {
        GameManager.Instance.SaveStage();
        _animator.SetTrigger("Play");
    }
}
