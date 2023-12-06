using System;
using System.Collections.Generic;
using DG.Tweening;
using PokerMode;
using UnityEngine;
using UnityEngine.UI;
using State = UI.Cheats.GumptionIcon.State;

namespace UI.Cheats
{
    public class GumptionDisplay : MonoBehaviour
    {
        [SerializeField] private List<GumptionIcon> _Icons;
        
        private CosmoController _target;
        private Func<int> _getActiveGumption;
        
        private (int active, int total) _displayedGumption;
        private Tween _shakeTween;
        
        public void Initialize(CosmoController target, Func<int> getActiveGumption)
        {
            _target = target;
            _getActiveGumption = getActiveGumption;
            
            foreach (var icon in _Icons) icon.SetState(State.Empty);
            _displayedGumption = default;
        }
        
        private void LateUpdate()
        {
            if (_target == null) return;
            var newGumption = (active: _getActiveGumption(), total: _target.Gumption);
            if (newGumption == _displayedGumption) return;
            
            int iconIndex = 0;
            foreach (var icon in _Icons)
            {
                var state = State.Empty;
                if (iconIndex < newGumption.total)
                {
                    state = (iconIndex >= newGumption.total - newGumption.active) ? State.Active : State.Full;
                }
                icon.SetState(state);
                iconIndex++;
            }
            
            _displayedGumption = newGumption;
        }
        
        public void CannotCast_Anim()
        {
            _shakeTween.Complete();
            _shakeTween = ((RectTransform) transform).DOShakeAnchorPos(.5f, strength: 6f, vibrato: 10).SetAutoKill(false);
        }
    }
}