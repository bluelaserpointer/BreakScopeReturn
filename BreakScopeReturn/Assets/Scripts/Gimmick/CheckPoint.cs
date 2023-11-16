using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : Interactable
{
    [SerializeField]
    int _priority;

    public int Priority => _priority;
    private void Awake()
    {
        onStepIn.AddListener(SetCheckPoint);
    }
    public void SetCheckPoint()
    {
        GameManager.Instance.CheckPointNotification.gameObject.SetActive(true);
        GameManager.Instance.SaveStage();
        gameObject.SetActive(false);
    }
}
