using System.Collections.Generic;
using UnityEngine;

namespace Animations
{
    public class AnimEventHelper : MonoBehaviour
    {
        private List<(string label, System.Action callback)> _callbacks = new();
        
        // Unity event callback
        public void Callback(string label)
        {
            for (int i = 0; i < _callbacks.Count; i++)
                if (_callbacks[i].label == label)
                {
                    _callbacks[i].callback();
                    _callbacks.RemoveAt(i);
                    i--;
                }
        }
        
        public void Register(string label, System.Action callback)
        {
            if (callback == null) return;
            _callbacks.Add((label, callback));
        }

        public void Clear()
        {
            _callbacks.Clear();
        }
    }
}