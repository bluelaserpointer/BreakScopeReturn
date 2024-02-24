using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : EnterEventBase
{
    public override bool DisableAfterStepIn => true;
    public override bool DisableAfterStepOut => false;

    public override void OnStepIn()
    {
        GameManager.Instance.CheckPointUI.ReachNewCheckPoint();
    }
    public override void OnStepOut()
    {
    }
}
