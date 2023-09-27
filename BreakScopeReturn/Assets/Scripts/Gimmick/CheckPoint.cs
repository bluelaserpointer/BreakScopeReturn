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
        if (GameManager.Instance.TrySetSpawnPoint(this))
            GameManager.Instance.CheckPointNotification.gameObject.SetActive(true);
    }
}
