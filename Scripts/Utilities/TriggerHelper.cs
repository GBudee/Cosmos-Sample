using System;
using UnityEngine;

namespace Utilities
{
    public class TriggerHelper : MonoBehaviour
    {
        public bool TriggerOccupied => _triggerElements > 0;
        public event System.Action OnTriggerEnterEvent;
        public event System.Action OnTriggerExitEvent;
        
        private int _triggerElements = 0;
        
        private void OnTriggerEnter(Collider other)
        {
            _triggerElements++;
            OnTriggerEnterEvent?.Invoke();
        }
        
        private void OnTriggerExit(Collider other)
        {
            _triggerElements--;
            OnTriggerExitEvent?.Invoke();
        }
    }
}