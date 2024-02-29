using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class SkillSlot : MonoBehaviour
{
    [SerializeField]
    GameObject[] _activatelits;
    [SerializeField]
    TextMeshProUGUI _keyText;

    public void ActivateLit(bool cond)
    {
        foreach (var lit in  _activatelits)
        {
            lit.SetActive(cond);
        }
    }
}
