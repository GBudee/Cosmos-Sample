using System;
using System.Collections.Generic;
using DG.Tweening;
using InteractionMode;
using Lean.Pool;
using MEC;
using NavigationMode;
using PokerMode.Cheats;
using PokerMode.Dialogue;
using TMPro;
using UI.Cheats;
using UI.Menus;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Interaction
{
    [ExecuteBefore(typeof(GameController))]
    public class InteractionHud : MonoBehaviour
    {
        [SerializeField] private CheatCardDisplay prefab_CheatCard;
        [SerializeField] private CheatCardDisplay prefab_GainASlot;
        [SerializeField] private CheatCardDisplay prefab_RemoveACard;
        [SerializeField] private InteractionController _InteractionController;
        [SerializeField] private CheatDeckMenu _CheatDeckMenu;
        [Header("Internal References")]
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private List<RectTransform> _CheatSlots;
        [SerializeField] private RectTransform _CheatDiscardAnchor;
        [SerializeField] private DialogueDisplay _DialogueDisplay;
        [SerializeField] private TravelMenu _TravelMenu;
        [SerializeField] private CanvasGroup _ButtonGroup;
        [FormerlySerializedAs("_DialogMessage")] [SerializeField] private TMP_Text _HeaderMessage;
        [SerializeField] private HudButton _CancelButton;
        [FormerlySerializedAs("_ConfirmButton")] [SerializeField] private HudButton _Confirm1Button;
        [SerializeField] private HudButton _Confirm2Button;

        public TravelMenu TravelMenu => _TravelMenu;
        
        private List<(ShadyGuy.CheatOptions option, CheatCardDisplay card)> _displayedCheatCards = new();
        private Tween _buttonFader;
        
        private void Awake()
        {
            _InteractionController.OnActivePointChanged = activePoint =>
            {
                if (activePoint != null) Show(activePoint);
                else Hide();
                return this;
            };
            _buttonFader = _ButtonGroup.DOFade(1f, .5f).From(0f)
                .OnRewind(() => _ButtonGroup.gameObject.SetActive(false))
                .OnPlay(() => _ButtonGroup.gameObject.SetActive(true))
                .SetAutoKill(false).Pause();
        }
        
        public void Show(InteractionPoint target)
        {
            gameObject.SetActive(true);
            this.DOKill();
            _TravelMenu.Hide();
            _buttonFader.Rewind();
            if (target.HasCustomCamera)
            {
                _CanvasGroup.alpha = 0f;
                DOVirtual.DelayedCall(target.HasCustomCamera ? .5f : 0f, () => _CanvasGroup.alpha = 1f, ignoreTimeScale: false).SetTarget(this);
            }
            else _CanvasGroup.alpha = 1f;
            
            _DialogueDisplay.Initialize(target.DialogueAnchor);
        }
        
        public void Hide()
        {
            this.DOKill();
            gameObject.SetActive(false);
        }
        
        public void DynamicMenu(string headerText, string cancelText, string confirmText, bool allowConfirm1,
            string confirm2Text = null, bool allowConfirm2 = false, Action<int> inputAction = null)
        {
            _CanvasGroup.alpha = 1f;
            _HeaderMessage.text = headerText;
            
            if (cancelText != null)
            {
                _CancelButton.SetListener(() => inputAction?.Invoke(0));
                _CancelButton.SetText(cancelText);
                _CancelButton.interactable = true;
                _CancelButton.gameObject.SetActive(true);
            }
            else _CancelButton.gameObject.SetActive(false);
            
            if (confirmText != null)
            {
                _Confirm1Button.SetListener(() => inputAction?.Invoke(1));
                _Confirm1Button.SetText(confirmText);
                _Confirm1Button.interactable = allowConfirm1;
                _Confirm1Button.gameObject.SetActive(true);
            }
            else _Confirm1Button.gameObject.SetActive(false);
            
            if (confirm2Text != null)
            {
                _Confirm2Button.SetListener(() => inputAction?.Invoke(2));
                _Confirm2Button.Text.text = confirm2Text;
                _Confirm2Button.interactable = allowConfirm2;
                _Confirm2Button.gameObject.SetActive(true);
            }
            else _Confirm2Button.gameObject.SetActive(false);
            
            _ButtonGroup.gameObject.SetActive(true);
            _buttonFader.PlayForward();
        }
        
        public IEnumerator<float> PickACheat(string headerText, string confirmText, IEnumerable<(ShadyGuy.CheatOptions, CheatCard)> choices, Action<ShadyGuy.CheatOptions, CheatCard> selectAction)
        {
            // Update message window
            _HeaderMessage.text = headerText;
            _CancelButton.gameObject.SetActive(false);
            bool declined = false;
            _Confirm1Button.SetListener(() =>
            {
                declined = true;
                _Confirm1Button.interactable = false;
            });
            _Confirm1Button.SetText(confirmText);
            _Confirm1Button.interactable = true;
            _Confirm1Button.gameObject.SetActive(true);
            _Confirm2Button.gameObject.SetActive(false);
            
            yield return Timing.WaitForSeconds(.2f);
            
            int slotIndex = 0;
            foreach (var (option, card) in choices)
            {
                var prefab = option switch
                {
                    ShadyGuy.CheatOptions.GainSlot => prefab_GainASlot,
                    ShadyGuy.CheatOptions.RemoveCard => prefab_RemoveACard,
                    _ => prefab_CheatCard
                };
                var newCard = LeanPool.Spawn(prefab, transform);
                newCard.SetSlot(_CheatSlots[slotIndex]);
                newCard.Initialize(card);
                newCard.DrawAnim(.25f);
                Service.AudioController.Play("CheatDraw");
                _displayedCheatCards.Add((option, newCard));
                slotIndex++;
                yield return Timing.WaitForSeconds(.15f);
                if (slotIndex > _CheatSlots.Count) break;
            }
            yield return Timing.WaitForSeconds(.35f);
            
            while (true)
            {
                yield return Timing.WaitForOneFrame;
                if (declined)
                {
                    foreach (var (option, card) in _displayedCheatCards) card.DiscardAnim(.5f, _CheatDiscardAnchor);
                    _displayedCheatCards.Clear();
                    Service.AudioController.Play("CheatDiscard");
                    yield return Timing.WaitForSeconds(.5f);
                    selectAction(ShadyGuy.CheatOptions.None, null);
                    yield break;
                }
                
                CheatCardDisplay hoveredCard = null;
                foreach (var (option, card) in _displayedCheatCards)
                {
                    card.ManagedUpdate(out bool mouseOver);
                    if (mouseOver) hoveredCard = card;
                }
                
                if (Input.GetMouseButtonDown(0) && hoveredCard != null)
                {
                    _Confirm1Button.interactable = false;
                    
                    ShadyGuy.CheatOptions selection = default;
                    for (var i = 0; i < _displayedCheatCards.Count; i++)
                    {
                        var (option, card) = _displayedCheatCards[i];
                        if (card == hoveredCard)
                        {
                            selection = option;
                            Timing.RunCoroutine(card.ActivateAnim());
                        }
                        else
                        {
                            card.DiscardAnim(.5f, _CheatDiscardAnchor);
                            _displayedCheatCards.RemoveAt(i);
                            i--;
                        }
                    }
                    
                    Service.AudioController.Play("CheatDiscard");
                    yield return Timing.WaitForSeconds(.25f);
                    
                    Service.AudioController.Play("CheatPlay");
                    hoveredCard.transform.DOMove(_CheatSlots[1].position, .5f).SetEase(Ease.OutQuad);
                    
                    yield return Timing.WaitForSeconds(.5f);
                    selectAction(selection, hoveredCard.Cheat);
                    yield break;
                }
            }
        }
        
        public IEnumerator<float> RevealCheatCard(CheatCard card)
        {
            _CancelButton.interactable = false;
            _Confirm1Button.interactable = false;

            var newCard = LeanPool.Spawn(prefab_CheatCard, transform);
            newCard.SetSlot(_CheatSlots[1]);
            newCard.Initialize(card);
            newCard.DrawAnim(.5f);
            Service.AudioController.Play("CheatDiscard");
            _displayedCheatCards.Add((default, newCard));
            yield return Timing.WaitForSeconds(.5f);
            
            newCard.ActHovered();
            
            yield return Timing.WaitForSeconds(.125f);
            
            Service.AudioController.Play("CheatPlay");
            yield return Timing.WaitUntilDone(newCard.ActivateAnim());
        }
        
        public IEnumerator<float> DiscardCheatCards()
        {
            _CanvasGroup.DOFade(0f, .5f).SetDelay(.5f).SetTarget(this);
            foreach (var (option, card) in _displayedCheatCards)
            {
                Service.AudioController.Play("CheatDiscard");
                card.DiscardAnim(.5f, _CheatDiscardAnchor);
            }
            yield return Timing.WaitForSeconds(1f);
            _displayedCheatCards.Clear();
        }
        
        public void DespawnCheatCards()
        {
            foreach (var (option, card) in _displayedCheatCards) LeanPool.Despawn(card);
            _displayedCheatCards.Clear();
        }
    }
}