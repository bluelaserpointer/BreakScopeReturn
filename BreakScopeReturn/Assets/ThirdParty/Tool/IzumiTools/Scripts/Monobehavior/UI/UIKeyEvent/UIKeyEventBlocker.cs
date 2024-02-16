using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IzumiTools
{
    public class UIKeyEventBlocker : UIKeyEventBase
    {
        private void OnEnable()
        {
            manager.BlockEnabled(this);
        }
        private void OnDisable()
        {
            manager.BlockDisabled(this);
        }

    }

}