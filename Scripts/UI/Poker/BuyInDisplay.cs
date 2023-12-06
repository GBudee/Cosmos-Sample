using System;
using System.Collections.Generic;
using System.Linq;
using Lean.Pool;
using PokerMode;
using UnityEngine;

namespace UI
{
    public class BuyInDisplay : MonoBehaviour
    {
        [SerializeField] private BuyInPip prefab_Circle;
        [SerializeField] private BuyInPip prefab_Rocket;
        
        private PlayerVisuals _target;
        private List<BuyInPip> _displayedPips = new();
        private bool _displayedRocket;
        
        public void Initialize(PlayerVisuals target)
        {
            // Reset
            foreach (var pip in _displayedPips) LeanPool.Despawn(pip);
            _displayedPips.Clear();
            _displayedRocket = false;
            
            _target = target;
        }

        public void UpdateFade(float value)
        {
            foreach (var pip in _displayedPips) pip.UpdateFade(value);
        }
        
        private void LateUpdate()
        {
            if (_target == null) return;
            /*
            if ((_displayedPips.Count, _displayedRocket) == _target.BuyInState) return;
            
            // Add new pips
            for (int i = 0; i < _target.BuyInState.count; i++)
                if (_displayedPips.Count == i)
                {
                    bool rocket = _target.BuyInState.objectiveHolder && i == 0;
                    var newPip = LeanPool.Spawn(rocket ? prefab_Rocket : prefab_Circle, transform);
                    newPip.Initialize();
                    _displayedPips.Add(newPip);
                }
            
            // Despawn used pips
            while (_displayedPips.Count > _target.BuyInState.count)
            {
                _displayedPips[^1].DespawnAnim();
                _displayedPips.RemoveAt(_displayedPips.Count - 1);
            }
            */
        }
    }
}