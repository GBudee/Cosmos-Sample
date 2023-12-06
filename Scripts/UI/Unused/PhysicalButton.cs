using System;
using System.Collections.Generic;
using DG.Tweening;
using EPOOutline;
using UnityEngine;

namespace UI
{
    public class PhysicalButton : Interactable
    {
        [Header("References")]
        [SerializeField] private Light _ActivationLight;
        [SerializeField] private Renderer _ButtonMesh;
        [Header("Values")]
        [SerializeField] private Transform _ButtonDownAnchor;
        [SerializeField] private Transform _ButtonUpAnchor;
        
        public event System.Action OnClick;
        
        public override void OnMouseDown()
        {
            if (!enabled) return;
            
            // Set mesh pos
            this.DOKill();
            _ButtonMesh.transform.position = _ButtonDownAnchor.position;
            
            // Enable "activation light"
            _ActivationLight.gameObject.SetActive(true);
            _ButtonMesh.material.EnableKeyword("_EMISSION");
            
            OnClick?.Invoke();
            base.OnMouseDown();
        }
        
        public override void OnMouseUp()
        {
            if (_mouseDown)
            {
                DOTween.Sequence()
                    .Join(_ButtonMesh.transform.DOMove(_ButtonUpAnchor.position, .35f))
                    .InsertCallback(.5f, () =>
                    {
                        _ActivationLight.gameObject.SetActive(false);
                        _ButtonMesh.material.DisableKeyword("_EMISSION");
                    }).SetTarget(this);
            }
            
            base.OnMouseUp();
        }
    }
}