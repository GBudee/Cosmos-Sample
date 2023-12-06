using System;
using UnityEngine;

namespace UI
{
    public class InteractableCollider : MonoBehaviour
    {
        [SerializeField] private Interactable _Target;
        
        void OnMouseEnter() => _Target.OnMouseEnter();
        void OnMouseExit() => _Target.OnMouseExit();
        void OnMouseDown() => _Target.OnMouseDown();
        void OnMouseUp() => _Target.OnMouseUp();
        void OnMouseDrag() => _Target.OnMouseDrag();
    }
}