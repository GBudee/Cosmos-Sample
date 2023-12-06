using System.Collections.Generic;
using EPOOutline;
using UnityEngine;

namespace UI
{
    public class PhysicalLever : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform _LeverOrigin;
        [Header("Values")]
        [SerializeField] private float _LeverMinAngle;
        [SerializeField] private float _LeverMaxAngle;
        [SerializeField] private float _LeverMinProximity = .2f;
        
        public event System.Action<float> OnValueChange;
        
        private float _leverValue; // 0 to 1, min to max
        
        // TODO: Button-style OnMouseDown/OnMouseUp manipulation of lever
        // - Define orientation anchors for 0 and 1
        // - Calculate intended orientation with axis from lever origin to mouse
        //   - Projects mouse position into lever plane
        // - Update slider position based on such
        // - Update lever position based on slider
        // - Hovering slider highlights lever
        
        void Awake()
        {
            _leverValue = 0f;
            _LeverOrigin.localEulerAngles = new Vector3(0, 0, _LeverMinAngle);
        }
        
        public override void OnMouseDrag()
        {
            _leverValue += Input.GetAxis("Mouse X") * .05f;
            _leverValue = Mathf.Clamp01(_leverValue);
            
            // Set lever orientation
            _LeverOrigin.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(_LeverMinAngle, _LeverMaxAngle, _leverValue));
            
            OnValueChange?.Invoke(_leverValue);
        }
    }
}