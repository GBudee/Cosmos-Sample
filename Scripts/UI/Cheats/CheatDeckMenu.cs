using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using PokerMode;
using PokerMode.Cheats;
using TMPro;
using UnityEngine;
using Utilities;

namespace UI.Cheats
{
    public class CheatDeckMenu : MonoBehaviour
    {
        [SerializeField] private CheatMenuSlot prefab_MenuSlot;
        [Header("Internal References")]
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private RectTransform _CardContainer;
        [SerializeField] private TMP_Text _Details;
        [SerializeField] private TMP_Text _CardInfo;
        [SerializeField] private HudButton _RemoveButton;
        [SerializeField] private HudButton _ExitButton;

        public bool IsActive => gameObject.activeSelf;
        public bool DisableHotkey => _removeMode;
        
        private List<CheatMenuSlot> _displayedCards = new();
        private bool _removeMode;
        private CheatMenuSlot _selectedCard;
        private Tween _fader;
        
        void Awake()
        {
            gameObject.SetActive(false);
            _fader = _CanvasGroup.DOFade(1f, .3f).From(0f)
                .OnPlay(() => gameObject.SetActive(true))
                .OnRewind(() => gameObject.SetActive(false))
                .SetAutoKill(false).Pause();
        }
        
        public void Show(CosmoController cosmoController, bool removeCardsMode, System.Action<CheatCard> inputAction = null)
        {
            if (removeCardsMode) _removeMode = true;
            
            // Fill slots with hand, deck, and discard cards
            int slotCount = 0;
            foreach (var card in cosmoController.CheatHand)
            {
                SpawnCardSlot(card, CheatMenuSlot.State.InSleeve);
                slotCount++;
            }
            foreach (var card in cosmoController.CheatDeck)
            {
                SpawnCardSlot(card, CheatMenuSlot.State.InDeck);
                slotCount++;
            }
            foreach (var card in cosmoController.CheatDiscard)
            {
                SpawnCardSlot(card, CheatMenuSlot.State.Discarded);
                slotCount++;
            }
            
            // Show empty slots if below 10 cards
            for (int i = slotCount; i < 10; i++)
            {
                SpawnCardSlot(null);
            }
            
            _RemoveButton.gameObject.SetActive(removeCardsMode);
            if (removeCardsMode)
            {
                _RemoveButton.interactable = false;
                _RemoveButton.SetListener(() =>
                {
                    var selectedCard = _selectedCard.DisplayedCard.Cheat;
                    Hide();
                    inputAction?.Invoke(selectedCard);
                });
            }
            
            _ExitButton.Text.text = removeCardsMode ? "DECLINE" : "EXIT";
            _ExitButton.SetListener(() =>
            {
                Hide();
                inputAction?.Invoke(null);
            });
            
            _Details.text = $"Max Cards in Sleeve: {cosmoController.CheatSlots}";
            _fader.PlayForward();
        }
        
        public void Hide()
        {
            _removeMode = false;
            foreach (var slot in _displayedCards) LeanPool.Despawn(slot);
            _displayedCards.Clear();
            _selectedCard = null;
            
            _fader.Rewind();
        }
        
        void LateUpdate()
        {
            CheatMenuSlot hoverTarget = null;
            foreach (var cardSlot in _displayedCards)
            {
                cardSlot.ManagedUpdate(out bool mouseOver);
                if (mouseOver)
                {
                    hoverTarget = cardSlot;
                }
            }
            
            _CardInfo.text = hoverTarget != null ? $"{hoverTarget.DisplayedCard.Cheat.Name}: <i>Currently {hoverTarget.CardState.ToString().AddSpacesToCamelCase()}</i>" : "";
            
            bool mouseOverGrid = RectTransformUtility.RectangleContainsScreenPoint(_CardContainer.parent as RectTransform, Input.mousePosition, null);
            if (_removeMode && mouseOverGrid && Input.GetMouseButtonDown(0)) SelectCard(hoverTarget);
        }
        
        private void SelectCard(CheatMenuSlot target)
        {
            foreach (var card in _displayedCards) card.ShowSelected(card == target);
            _RemoveButton.interactable = target != null;
            _selectedCard = target;
        }
        
        private void SpawnCardSlot(CheatCard card, CheatMenuSlot.State state = CheatMenuSlot.State.None)
        {
            var newSlot = LeanPool.Spawn(prefab_MenuSlot, _CardContainer);
            newSlot.Initialize(card, state);
            _displayedCards.Add(newSlot);
        }
    }
}