using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IzumiTools
{
    public class UIKeyEvent : UIKeyEventBase
    {
        public KeyCode keyCode;
        public UnityEvent keyEvent;
        private void Update()
        {
            if (Input.GetKeyDown(keyCode))
                keyEvent.Invoke();
        }
        [ContextMenu(nameof(CopyEventFromButtonClick))]
        private void CopyEventFromButtonClick()
        {
            Button button = GetComponent<Button>();
            if (button == null)
            {
                print("No Button components exist in the same gameobject.");
                return;
            }
            keyEvent = button.onClick;
        }
    }

}