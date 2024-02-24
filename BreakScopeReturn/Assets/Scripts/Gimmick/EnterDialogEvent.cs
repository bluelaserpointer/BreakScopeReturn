using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnterDialogEvent : EnterEventBase
{
    [SerializeField]
    bool oneTime = true;
    [SerializeField]
    DialogNodeSet _stepInDialog, _stepOutDialog;

    public override bool DisableAfterStepIn => oneTime && _stepInDialog != null;
    public override bool DisableAfterStepOut => oneTime && _stepOutDialog != null;
    public override void OnStepIn()
    {
        if (_stepInDialog != null)
            _stepInDialog.SetToDialogUI();
    }
    public override void OnStepOut()
    {
        if (_stepOutDialog != null)
            _stepOutDialog.SetToDialogUI();
    }
}
