using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogUI : MonoBehaviour
{
    public float displayTimePerWord = 0.5F;
    public float extraDisplayTimePerSentense = 1F;

    public DialogNodeSet CurrentNodeSet { get; private set; }
    float currentNodeDisplayedTime;

    public void SetDialog(DialogNodeSet nodeSetPrefab)
    {
        if (CurrentNodeSet != null)
        {
            Destroy(CurrentNodeSet.gameObject);
        }
        if (nodeSetPrefab == null)
        {
            return; 
        }
        CurrentNodeSet = Instantiate(nodeSetPrefab, transform);
        CurrentNodeSet.Init();
        CurrentNodeSet.gameObject.SetActive(true);
        CurrentNodeSet.SetFirstNode();
        currentNodeDisplayedTime = 0;
    }
    private void Update()
    {
        if (CurrentNodeSet != null)
        {
            currentNodeDisplayedTime += Time.deltaTime;
            if (currentNodeDisplayedTime > extraDisplayTimePerSentense + CurrentNodeSet.CurrentNodeDisplayTime)
            {
                currentNodeDisplayedTime = 0;
                CurrentNodeSet.Next();
                if (CurrentNodeSet.ReachEnd)
                {
                    CurrentNodeSet = null;
                }
            }
        }
    }
}
