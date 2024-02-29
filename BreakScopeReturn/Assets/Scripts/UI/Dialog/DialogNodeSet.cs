using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DialogNodeSet : MonoBehaviour
{
    public int priority;
    public DialogNode CurrentNode
    {
        get => _currentNode;
        set
        {
            if (_currentNode == value)
                return;
            _currentNode?.gameObject.SetActive(false);
            (_currentNode = value)?.gameObject.SetActive(true);
        }
    }
    public float CurrentNodeDisplayTime => CurrentNode != null ? CurrentNode.LifeTime() : 0;
    public bool ReachEnd => CurrentNode == null;

    DialogNode _currentNode;

    public void Init()
    {
        CurrentNode = null;
        foreach(Transform child in transform)
            child.gameObject.SetActive(false);
    }
    public void SetFirstNode()
    {
        SetNode(0);
    }
    public void Next()
    {
        if (CurrentNode != null)
            SetNode(CurrentNode.transform.GetSiblingIndex() + 1);
    }
    public void End()
    {
        CurrentNode = null;
    }
    public bool SetNode(int index)
    {
        if (0 <= index && index < transform.childCount)
        {
            CurrentNode = transform.GetChild(index).GetComponent<DialogNode>();
            return true;
        }
        else
        {
            CurrentNode = null;
            return false;
        }
    }
    public void SetToDialogUI()
    {
        GameManager.Instance.DialogUI.SetDialog(this);
    }
}
