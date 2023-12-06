using System;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TooltipManager : MonoBehaviour
    {
        [SerializeField] private Canvas _ParentCanvas;
        [SerializeField] private Tooltip prefab_Tooltip;

        private Tooltip _activeTooltip;
        private TooltipTarget _activeTarget;
        
        private void OnEnable()
        {
            if (_activeTarget != null) Spawn(_activeTarget);
        }
        
        private void OnDisable()
        {
            Despawn();
        }
        
        public void Show(TooltipTarget target)
        {
            if (!Settings.Current.ShowTooltips) return;
            if (_activeTarget == target) return;
            if (_activeTarget != null) Hide(_activeTarget);
            
            if (enabled) Spawn(target);
            
            _activeTarget = target;
        }
        
        public void Hide(TooltipTarget target)
        {
            if (_activeTarget != target) return;
            
            Despawn();
            
            _activeTarget = null;
        }
        
        private void Spawn(TooltipTarget target)
        {
            _activeTooltip = LeanPool.Spawn(prefab_Tooltip, transform);
            _activeTooltip.Show(_ParentCanvas, target.Anchor, target.Header, target.Contents);
        }
        
        private void Despawn()
        {
            if (_activeTooltip == null) return;
            LeanPool.Despawn(_activeTooltip);
            _activeTooltip = null;
        }
    }
}