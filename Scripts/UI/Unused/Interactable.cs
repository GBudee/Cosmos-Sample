using System;
using System.Collections.Generic;
using EPOOutline;
using UnityEngine;

namespace UI
{
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Interactable")]
        [SerializeField] private List<Outlinable> _Outlines;
        
        protected bool _mouseOver;
        protected bool _mouseDown;
        
        private void OnEnable()
        {
            foreach (var outline in _Outlines) outline.enabled = _mouseOver;
        }
        
        private void OnDisable()
        {
            foreach (var outline in _Outlines) outline.enabled = false;
            OnMouseUp();
        }
        
        public void OnMouseEnter() 
        {
            _mouseOver = true;
            if (!enabled) return;
            foreach (var outline in _Outlines) outline.enabled = true; 
        }
        
        public void OnMouseExit()
        {
            _mouseOver = false;
            if (!enabled || _mouseDown) return;
            foreach (var outline in _Outlines) outline.enabled = false;
        }
        
        
        public virtual void OnMouseDown()
        {
            if (!enabled) return;
            _mouseDown = true;
        }
        
        public virtual void OnMouseUp()
        {
            if (_mouseDown && !_mouseOver)
                foreach (var outline in _Outlines) outline.enabled = false;
            _mouseDown = false;
        }
        
        public virtual void OnMouseDrag() { }
    }
}