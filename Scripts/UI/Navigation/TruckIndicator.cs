using System;
using NavigationMode;
using PokerMode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TruckIndicator : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private RectTransform _DirectionalIcon;
        [SerializeField] private RectTransform _StaticIcon;
        [SerializeField] private Image _Image;
        [SerializeField] private Sprite _Truck;
        [SerializeField] private Sprite _EddieSymbol;
        
        private TruckController _truck;
        private Transform _eddie;
        private CosmoController _cosmoController;
        private NavigationController _navigationCtrl;
        private RectTransform _rectTransform;
        
        void Awake()
        {
            _rectTransform = transform as RectTransform;
        }
        
        public void Initialize(TruckController truck, Transform eddie, CosmoController cosmoController, NavigationController navigationCtrl)
        {
            _truck = truck;
            _eddie = eddie;
            _cosmoController = cosmoController;
            _navigationCtrl = navigationCtrl;
            
            if (_truck == null) Debug.Log("Reminder: Set up a TruckController in the scene to have the truck-direction-symbol appear in the hud");
        }
        
        private void LateUpdate()
        {
            if (_truck == null) return;

            bool showEddieInstead = _cosmoController.HasLevelUp || _cosmoController.Credits < 100;
            _Image.sprite = showEddieInstead ? _EddieSymbol : _Truck;
            
            var targetTransform = showEddieInstead ? _eddie : _truck.transform;
            var bodyTransform = _navigationCtrl.transform;
            var cameraTransform = _navigationCtrl.CameraAnchor.transform;
            
            // Calculate direction to truck in local-planar-space for player
            var planarDir = Vector3.ProjectOnPlane(targetTransform.position - bodyTransform.position, bodyTransform.up).normalized;
            var effectiveForward = Vector3.ProjectOnPlane(cameraTransform.forward, bodyTransform.up);
            if (Mathf.Approximately(effectiveForward.magnitude, 0)) return; // Don't update for straight-up-or-down camera
            var planarOrientation = Quaternion.LookRotation(effectiveForward, bodyTransform.up);
            var localDir = Quaternion.Inverse(planarOrientation) * planarDir;
            
            // Orient container based on local rotation 
            _DirectionalIcon.localRotation = Quaternion.LookRotation(Vector3.forward, new Vector3(-localDir.z, localDir.x));
            
            // Keep static icon oriented normally
            _StaticIcon.rotation = _rectTransform.rotation;
            
            // Place container on rect boundary
            var rectDir = new Vector2(localDir.x, localDir.z);
            var onUnitSquare = MathG.UnitCircleToUnitSquare(rectDir);
            var boundaryRect = _rectTransform.rect;
            var xPos = Mathf.Lerp(boundaryRect.xMin, boundaryRect.xMax, onUnitSquare.x);
            var yPos = Mathf.Lerp(boundaryRect.yMin, boundaryRect.yMax, onUnitSquare.y);
            _DirectionalIcon.localPosition = new Vector3(xPos, yPos);
            
            // Hide icon if truck would be on screen
            var truckScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, targetTransform.position);
            bool hideTruck = _truck.VisibleZone.TriggerOccupied // In visible zone for truck
                && Vector3.Dot(cameraTransform.forward, targetTransform.position - bodyTransform.position) > 0 // Facing toward truck
                && RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, truckScreenPos, null); // Truck is in screen rect
            _CanvasGroup.alpha = hideTruck ? 0f : 1f;
        }
    }
}