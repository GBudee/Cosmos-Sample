using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MEC;
using PokerMode;
using TMPro;
using UI.Cheats;
using UI.Menus;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

namespace UI
{
    public class PokerHud : MonoBehaviour
    {
        [FormerlySerializedAs("_CheatController")] [SerializeField] private CosmoController _CosmoController;
        [SerializeField] private ObjectiveMenu _ObjectiveMenu;
        [SerializeField] private CheatDeckMenu _CheatDeckMenu;
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private WinMessage _WinMessage;
        
        [Header("Particles")] 
        [SerializeField] private FireworkController _FireworkController;

        [Header("HUD Displays")] 
        [SerializeField] private PlayerDisplayMgr _PlayerDisplayMgr;
        [SerializeField] private DialogueDisplayMgr _DialogueDisplayMgr;
        [FormerlySerializedAs("_Miniriver")] [SerializeField] private RiverDisplay _RiverDisplay;
        [FormerlySerializedAs("_HandDisplay")] [SerializeField] private BigHandDisplay _BigHandDisplay;
        [SerializeField] private NumberDisplay _PotDisplay;
        [SerializeField] private LevelDisplay _LevelDisplay;
        [SerializeField] private NumberDisplay _XPDisplay;
        [SerializeField] private NumberDisplay _XPThresholdDisplay;
        [SerializeField] private SleeveDisplay _SleeveDisplay;
        
        [Header("Default Buttons")]
        [SerializeField] private GameObject _DefaultContainer;
        [SerializeField] private HudButton _LeaveTable;
        [SerializeField] private HudButton _BuyIn;
        [SerializeField] private TooltipTarget _BuyInTooltip;
        [SerializeField] private TooltipTarget _CashRemainingTooltip;
        [SerializeField] private HudButton _StartHand;
        
        [Header("Play Mode Buttons")]
        [SerializeField] private GameObject _PlayModeContainer;
        [SerializeField] private HudButton _CallButton;
        [SerializeField] private TooltipTarget _CallTooltip;
        [SerializeField] private HudButton _RaiseButton;
        [SerializeField] private HudButton _CheckButton;
        [SerializeField] private HudButton _FoldButton;
        [SerializeField] private HudButton _CheatButton;
        
        [Header("Raise Mode Buttons")]
        [SerializeField] private GameObject _RaiseModeContainer;
        [SerializeField] private HudButton _RaiseBack;
        [SerializeField] private HudButton _RaiseConfirm;
        [SerializeField] private HudSlider _RaiseSlider;
        [SerializeField] private TMP_Text _SliderText;

        [Header("Showdown Buttons")] 
        [SerializeField] private GameObject _ShowdownModeContainer;
        [SerializeField] private HudButton _ShowdownButton;
        [SerializeField] private HudButton _ShowdownCheatButton;

        private enum Mode { None, Default, Play, Raise, Showdown }
        private Mode _mode;
        
        public bool AllowInput => !EscapeMenu.Paused && !_ObjectiveMenu.IsActive && !_CheatDeckMenu.IsActive;
        
        void Awake()
        {
            _RaiseButton.SetListener(() => SetMode(Mode.Raise));
            _RaiseBack.SetListener(() => SetMode(Mode.Play));
        }
        
        public void Show(IEnumerable<PlayerVisuals> playerVisuals, CreditVisuals potVisuals, HandVisuals humanHand, RiverVisuals targetRiver)
        {
            _PlayerDisplayMgr.Initialize(playerVisuals);
            _DialogueDisplayMgr.Initialize(playerVisuals);
            _PotDisplay.Initialize(() => potVisuals.Credits);
            _LevelDisplay.Initialize(_CosmoController);
            _XPDisplay.Initialize(() => _CosmoController.XP);
            _XPThresholdDisplay.Initialize(() => _CosmoController.XPThreshold);
            _BigHandDisplay.Initialize(humanHand);
            _RiverDisplay.Initialize(targetRiver);
            _SleeveDisplay.Initialize(_CosmoController, _BigHandDisplay, _RiverDisplay, () => AllowInput);
            _BuyInTooltip.SetDynamicContents(getContents: () => "To play at this table, you need to buy in with credits from your wallet. If you don't have enough, try a cheaper table, or talk to the shady guy at the gas station." +
                                                                $"\n\n<color=white>Wallet: {CreditVisuals.CREDIT_SYMBOL}{_CosmoController.Credits}</color>");
            _CashRemainingTooltip.SetDynamicContents(getContents: () => "These are the credits you're risking at this table. If you run out, you will have the option to buy in again." +
                                                                        $"\n\n<color=white>Wallet: {CreditVisuals.CREDIT_SYMBOL}{_CosmoController.Credits}</color>");
            
            SetMode(Mode.None);
            
            this.DOKill();
            gameObject.SetActive(true);
            DOTween.Sequence().SetTarget(this).SetUpdate(isIndependentUpdate: true)
                .Insert(.5f, _CanvasGroup.DOFade(1f, .5f));
        }
        
        public void Hide()
        {
            this.DOKill();
            _CanvasGroup.DOFade(0, .4f).SetTarget(this).SetUpdate(isIndependentUpdate: true)
                .OnComplete(() => gameObject.SetActive(false));
        }
        
        public void ShowWinMessage()
        {
            _WinMessage.Show();
            _FireworkController.CelebrateVictory();
        }
        public void HideWinMessage() => _WinMessage.Hide();
        public void UpdateBestHand(HandEvaluator.HandType hand) => _BigHandDisplay.ShowHandValue(hand);
        
        // *** BUTTON CONTROL ***
        public delegate void DefaultAction(bool exit);
        public void EnterDefaultMode(bool hasCredits, bool canPlay, Action buyInAction, int buyInCost, DefaultAction inputAction)
        {
            _LeaveTable.SetListener(() => inputAction(exit: true));
            _LeaveTable.SetText(hasCredits ? "CASH OUT" : "LEAVE TABLE");
            _StartHand.SetListener(() => inputAction(exit: false));
            
            _BuyIn.gameObject.SetActive(!hasCredits);
            _BuyIn.SetText($"BUY IN: {CreditVisuals.CREDIT_SYMBOL}{buyInCost}");
            _StartHand.gameObject.SetActive(hasCredits);
            _BuyIn.interactable = _StartHand.interactable = canPlay;
            
            if (!hasCredits)
            {
                _BuyIn.SetListener(() =>
                {
                    _LeaveTable.SetText("CASH OUT");
                    _BuyIn.gameObject.SetActive(false);
                    _StartHand.gameObject.SetActive(true);
                    
                    buyInAction();
                });
            }
            
            SetMode(Mode.Default);
        }
        
        public void EnterPlayMode_Waiting()
        {
            // Disable buttons
            _CallButton.SetText($"CALL");
            _CallButton.interactable = false;
            _CallTooltip.SetDynamicContents();
            _RaiseButton.interactable = false;
            _CheckButton.interactable = false;
            _FoldButton.interactable = false;
            _CheatButton.interactable = false;
            
            SetMode(Mode.Play);
        }
        
        public delegate void PlayAction(int bid, bool fold);
        public void EnterPlayMode_HumanTurn(bool callRequired, bool raiseRequired, int minBet, int minRaise, int maxRaise, PlayAction inputAction)
        {
            _CallButton.SetText($"{(callRequired ? "CALL" : "BET")} {minBet}");
            _CallTooltip.SetDynamicContents(() => callRequired ? "Call" : "Bet", () => callRequired 
                ? "Click \"Call\" to match your opponents previous bet: if they bet ¢50, this will button will let you bet exactly ¢50." 
                : "Click \"Bet\" to bet a small amount. Use \"Check\" instead if you want to take the absolute minimum risk.");
            
            // Configure dynamic buttons
            _CallButton.SetListener(() => inputAction(minBet, false));
            _CheckButton.SetListener(() => inputAction(0, false));
            _FoldButton.SetListener(() => inputAction(0, true));
            _CheatButton.SetListener(() => _CosmoController.FLAG_CheatButtonPressed = true);
            
            _RaiseButton.SetText(callRequired ? "RAISE" : "BET");
            _RaiseConfirm.SetText(callRequired ? "RAISE" : "BET");
            int raiseVal = minRaise;
            _RaiseSlider.SetListener((float t) =>
            {
                raiseVal = Mathf.RoundToInt(Mathf.Lerp(minRaise, maxRaise, t) / 5f) * 5;
                _SliderText.text = $"{CreditVisuals.CREDIT_SYMBOL}{raiseVal.ToString()}";
            });
            _RaiseSlider.value = 0;
            _SliderText.text = $"{CreditVisuals.CREDIT_SYMBOL}{raiseVal.ToString()}";
            _RaiseConfirm.SetListener(() => inputAction(raiseVal, false));
            
            // Configure button interactivity
            _CallButton.interactable = !raiseRequired;
            _RaiseButton.interactable = true;
            _CheckButton.interactable = !callRequired;
            _FoldButton.interactable = true;
            _CheatButton.interactable = true;
            
            SetMode(Mode.Play);
        }
        
        public void EnterShowdownMode(bool interactable, Action inputAction = null)
        {
            if (inputAction != null)
            {
                _ShowdownButton.SetListener(inputAction);
                _ShowdownCheatButton.SetListener(() => _CosmoController.FLAG_CheatButtonPressed = true);
            }
            _ShowdownButton.interactable = _ShowdownCheatButton.interactable = interactable;
            
            SetMode(Mode.Showdown);
        }
        
        private void SetMode(Mode value)
        {
            _DefaultContainer.SetActive(value == Mode.Default);
            _PlayModeContainer.SetActive(value == Mode.Play);
            _RaiseModeContainer.SetActive(value == Mode.Raise);
            _ShowdownModeContainer.SetActive(value == Mode.Showdown);
            _mode = value;
        }
        
        private void LateUpdate()
        {
            if (_CosmoController.FLAG_ChangedObjective)
            {
                _ObjectiveMenu.Show(ObjectiveMenu.Mode.NextObjective, _CosmoController.CurrentObjective);
                
                _CosmoController.FLAG_ChangedObjective = false;
            }
        }
    }
}