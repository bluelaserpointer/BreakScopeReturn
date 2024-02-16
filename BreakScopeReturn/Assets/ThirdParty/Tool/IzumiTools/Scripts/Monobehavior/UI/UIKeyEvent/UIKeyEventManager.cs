using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IzumiTools
{
    [DisallowMultipleComponent]
    public class UIKeyEventManager : MonoBehaviour
    {
        [SerializeField]
        List<UIKeyEventBase> _uiKeyEvents = new List<UIKeyEventBase>();

        int _topBlockIndex = -1;

        [ContextMenu(nameof(LinkUIKeyEventsInChildren))]
        private void LinkUIKeyEventsInChildren()
        {
            _uiKeyEvents.Clear();
            foreach (var uiKeyEvent in GetComponentsInChildren<UIKeyEventBase>(includeInactive: true))
            {
                _uiKeyEvents.Add(uiKeyEvent);
                uiKeyEvent.manager = this;
            }
        }
        public void BlockEnabled(UIKeyEventBlocker blocker)
        {
            int blockIndex = _uiKeyEvents.IndexOf(blocker);
            if (blockIndex <= _topBlockIndex)
                return;
            for (int i = _topBlockIndex + 1; i < blockIndex; ++i)
            {
                if (_uiKeyEvents[i].GetType().Equals(typeof(UIKeyEvent)))
                {
                    _uiKeyEvents[i].enabled = true;
                }
            }
            _topBlockIndex = blockIndex;
        }
        public void BlockDisabled(UIKeyEventBlocker blocker)
        {
            int blockIndex = _uiKeyEvents.IndexOf(blocker);
            if (blockIndex < _topBlockIndex)
                return;
            int newTopBlockIndex = _uiKeyEvents.FindLastIndex(uiKeyEvent =>
            {
                return uiKeyEvent.enabled && uiKeyEvent.GetType().Equals(typeof(UIKeyEventBlocker));
            });
            for (int i = newTopBlockIndex + 1; i < _topBlockIndex; ++i)
            {
                if (_uiKeyEvents[i].GetType().Equals(typeof(UIKeyEvent)))
                {
                    _uiKeyEvents[i].enabled = false;
                }
            }
        }
    }

}