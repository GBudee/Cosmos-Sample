using System;
using DG.Tweening;
using NavigationMode;
using PokerMode;
using UI.Cheats;
using UI.Menus;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    [ExecuteBefore(typeof(GameController))]
    public class NavigationHud : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField] private TruckController _Truck;
        [SerializeField] private Transform _Eddie;
        [SerializeField] private CosmoController _CosmoController;
        [SerializeField] private NavigationController _NavigationController;
        [SerializeField] private ObjectiveMenu _ObjectiveMenu;
        [SerializeField] private CheatDeckMenu _CheatDeckMenu;
        [Header("Internal References")]
        [SerializeField] private CanvasGroup _CanvasGroup;
        [FormerlySerializedAs("_Wallet")] [SerializeField] private NumberDisplay _WalletDisplay;
        [SerializeField] private LevelDisplay _LevelDisplay;
        [SerializeField] private NumberDisplay _XPDisplay;
        [SerializeField] private NumberDisplay _XPThresholdDisplay;
        [SerializeField] private TruckIndicator _TruckIndicator;
        [SerializeField] private Image _ObjectiveIcon;
        [SerializeField] private Image _CheatDeckMenuIcon;
        [SerializeField] private Image _FlashlightIcon;
        [SerializeField] private Image _RunIcon;
        [Header("Values")] 
        [SerializeField] private Sprite _SelectedButton;
        [SerializeField] private Sprite _UnselectedButton;
        
        private bool _flashlightDisplay;
        private bool _showObjectiveMenu;
        
        private void Awake()
        {
            _NavigationController.OnEnabledChange += show =>
            {
                if (_CanvasGroup == null || gameObject == null) return; // In case of OnEnabledChange due to Destroy
                if (show || Service.GameController.CurrentMode == GameController.GameMode.Interaction) Show();
                else Hide();
            };
            _WalletDisplay.Initialize(() => _CosmoController.Credits);
            _LevelDisplay.Initialize(_CosmoController);
            _XPDisplay.Initialize(() => _CosmoController.XP);
            _XPThresholdDisplay.Initialize(() => _CosmoController.XPThreshold);
            gameObject.SetActive(false);
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
            this.DOKill();
            DOTween.Sequence().Insert(.5f, _CanvasGroup.DOFade(1f, .5f)).SetTarget(this);
            
            _TruckIndicator.Initialize(_Truck, _Eddie, _CosmoController, _NavigationController);
        }
        
        public void Hide()
        {
            this.DOKill();
            if (_CanvasGroup.alpha == 0) gameObject.SetActive(false);
            else _CanvasGroup.DOFade(0f, .5f).SetTarget(this)
                .OnComplete(() => gameObject.SetActive(false));
        }
        
        private void LateUpdate()
        {
            var flashlightOn = _NavigationController.FlashlightOn;
            if (flashlightOn != _flashlightDisplay)
            {
                _FlashlightIcon.sprite = flashlightOn ? _SelectedButton : _UnselectedButton;
                _flashlightDisplay = flashlightOn;
            }
            
            // Show objective menu from "new objective" trigger
            if (_CosmoController.FLAG_ChangedObjective)
            {
                _ObjectiveMenu.Show(ObjectiveMenu.Mode.NextObjective, _CosmoController.CurrentObjective, () =>
                {
                    _NavigationController.ShowObjectiveMenu = false;
                    _ObjectiveIcon.sprite = _UnselectedButton;
                });
                _ObjectiveIcon.sprite = _SelectedButton;
                _NavigationController.ShowObjectiveMenu = true;
                
                _CosmoController.FLAG_ChangedObjective = false;
            }
            
            _RunIcon.sprite = _NavigationController.Run ? _SelectedButton : _UnselectedButton;
            
            // Show objective menu from input
            var showObjectiveMenu = _NavigationController.ShowObjectiveMenu;
            if (showObjectiveMenu != _ObjectiveMenu.IsActive)
            {
                if (showObjectiveMenu) _ObjectiveMenu.Show(ObjectiveMenu.Mode.TablePreview, _CosmoController.CurrentObjective, () =>
                {
                    _NavigationController.ShowObjectiveMenu = false;
                    _ObjectiveIcon.sprite = _UnselectedButton;
                });
                else _ObjectiveMenu.Hide();
                _ObjectiveIcon.sprite = showObjectiveMenu ? _SelectedButton : _UnselectedButton;
            }
            
            var showCheatDeckMenu = _NavigationController.ShowCheatDeckMenu;
            if (showCheatDeckMenu != _CheatDeckMenu.IsActive && !_CheatDeckMenu.DisableHotkey)
            {
                if (showCheatDeckMenu) _CheatDeckMenu.Show(_CosmoController, removeCardsMode: false, x =>
                {
                    _NavigationController.ShowCheatDeckMenu = false;
                    _CheatDeckMenuIcon.sprite = _UnselectedButton;
                });
                else _CheatDeckMenu.Hide();
                _CheatDeckMenuIcon.sprite = showCheatDeckMenu ? _SelectedButton : _UnselectedButton;
            }
        }
    }
}