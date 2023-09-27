using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TeamWalkie : Item
{
    public override bool TryPickUp()
    {
        GameManager.Instance.CurrentStage.GetComponent<Stage1Scenario>().ActivateRaminEnemyCounter();
        return true;
    }
}
