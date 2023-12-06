using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lean.Pool;
using MEC;
using PokerMode;
using PokerMode.Cheats;
using UI.Menus;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

namespace UI.Cheats
{
    public class SleeveDisplay : MonoBehaviour
    {
        [Header("References")]
        [FormerlySerializedAs("prefab_DisplayedCards")] [SerializeField] private CheatCardDisplay prefab_CheatCard;
        [SerializeField] private HudButton _SleeveButton;
        [SerializeField] private List<CanvasGroup> _CardSlots;
        [SerializeField] private CheatReticle _CheatReticle;
        [SerializeField] private GumptionDisplay _GumptionDisplay;
        [SerializeField] private Image _Scrim;
        
        [Header("Sleeve Positioning")]
        [FormerlySerializedAs("_Anchor")] [SerializeField] private RectTransform _Container;
        [SerializeField] private RectTransform _DiscardAnchor;
        [SerializeField] private RectTransform _SelectedCheatAnchor;
        [SerializeField] private RectTransform _DrawnCheatAnchor;
        [SerializeField] private float _ClosedX;
        [SerializeField] private float _SelectedX;
        [SerializeField] private float _OpenX;
        
        [Header("Deck Icon Positioning")]
        [SerializeField] private CheatDeckIcon _DeckIcon;
        [SerializeField] private TooltipTarget _DeckIconTooltip;
        [SerializeField] private float _SelectedDeckX;
        [FormerlySerializedAs("_BaseDeckX")] [SerializeField] private float _DefaultDeckX;
        
        private CosmoController _target;
        private BigHandDisplay _bigHandDisplay;
        private RiverDisplay _riverDisplay;
        private List<CheatCardDisplay> _displayedCards = new();
        private int _cheatSlots;
        private CheatCardDisplay _selectedCheat;
        private System.Func<bool> _allowInput;
        
        private Tween _hoverAnim;
        private Tween _selectedAnim;
        private Tween _openAnim;
        
        private bool _firstOpen;
        private bool _open;
        
        protected void Awake()
        {
            _SleeveButton.OnStateTransition += state =>
            {
                if (state == IHudSelectable.State.Normal || _open) _hoverAnim.PlayBackwards();
                else if (state == IHudSelectable.State.Highlighted) _hoverAnim.PlayForward();
            };
        }
        
        public void Initialize(CosmoController cosmoController, BigHandDisplay bigHandDisplay, RiverDisplay riverDisplay, System.Func<bool> allowInput)
        {
            _target = cosmoController;
            _bigHandDisplay = bigHandDisplay;
            _riverDisplay = riverDisplay;
            _allowInput = allowInput;
            _GumptionDisplay.Initialize(_target, getActiveGumption: () => _selectedCheat?.Cheat.Cost ?? 0);
            _DeckIconTooltip.SetDynamicContents(getContents: () =>
            {
                var deckString = _target.CheatDeck.OrderBy(x => x.Name).ToVerboseString(x => x.Name, false);
                var discardString = _target.CheatDiscard.ToVerboseString(x => x.Name, false);
                if (string.IsNullOrEmpty(deckString)) deckString = "<i>None</i>";
                if (string.IsNullOrEmpty(discardString)) discardString = "<i>None</i>";
                return "How many of your cheats you still have left to draw. When you've drawn them all, your discarded cheats reshuffle." +
                    $"\n\n<color=white>To Draw: </color>{deckString}\n<color=white>Discarded: </color>{discardString}";
            });
            
            // Clear
            foreach (var card in _displayedCards) LeanPool.Despawn(card);
            _displayedCards.Clear();
            
            // Spawn cards
            _cheatSlots = cosmoController.CheatSlots;
            int slotIndex = 0;
            foreach (var cheat in cosmoController.CheatHand)
            {
                _CardSlots[slotIndex].gameObject.SetActive(true);
                SpawnCard(cheat, slotIndex);
                slotIndex++;
            }
            for (int i = slotIndex; i < 5; i++) _CardSlots[i].gameObject.SetActive(slotIndex < _cheatSlots);
            
            // Show deck
            _DeckIcon.SetCount(_target.CheatDeck.Count());
            
            // Reset state
            InitStateAnims();
            _hoverAnim.Rewind();
            _selectedAnim.Rewind();
            _openAnim.Rewind();
            _SleeveButton.interactable = true;
            _SleeveButton.SetListener(Open);
            _firstOpen = true;
            _open = false;
            _selectedCheat = null;
        }
        
        void Update()
        {
            if (_target == null || !_allowInput()) return;
            
            bool cardHover = false;
            bool targetHover = false;
            bool selectedNewCheat = false;
            bool lClick = Input.GetMouseButtonDown(0);
            bool rClick = Input.GetMouseButtonDown(1);

            if (_target.FLAG_CheatButtonPressed)
            {
                _target.FLAG_CheatButtonPressed = false;
                if (!_open && _SleeveButton.interactable) Open();
            }
            
            // Update cards
            foreach (var card in _displayedCards)
            {
                bool castable = _target.CanCast(card.Cheat);
                card.ShowCastable(castable);
                card.ManagedUpdate(out bool mouseOver);
                cardHover |= mouseOver;
                
                // Check for selection input
                if (_selectedCheat == null && _SleeveButton.interactable && mouseOver && lClick)
                {
                    if (castable)
                    {
                        SelectCheat(card);
                        selectedNewCheat = true;
                    }
                    else _GumptionDisplay.CannotCast_Anim();
                }
            }
            
            if (_selectedCheat != null)
            {
                bool allowActivation = _target.AllowCheatActivation(_selectedCheat.Cheat);
                
                // TODO: Check the targeting-type of the selected cheat for hover validity
                // Targeting for Human Hand Cards
                var handTargeted = _selectedCheat.Cheat.Targeting == CheatCard.TargetingType.Hand;
                _bigHandDisplay.ManagedUpdate(allowActivation && handTargeted, out BigCardDisplay hoveredBigCard);
                if (allowActivation && handTargeted && hoveredBigCard != null)
                {
                    targetHover = true;
                    if (lClick)
                    {
                        _target.ActivateCheat_HandTarget(_selectedCheat.Cheat, hoveredBigCard.HandIndex);
                        Service.AudioController.Play("CheatPlay");
                        Timing.RunCoroutine(OnActivatedCheat());
                        return;
                    }
                }
                
                // Targeting for River Cards
                var riverTargeted = _selectedCheat.Cheat.Targeting == CheatCard.TargetingType.River;
                _riverDisplay.ManagedUpdate(allowActivation && riverTargeted, out MiniCardDisplay hoveredRiverCard);
                if (allowActivation && riverTargeted && hoveredRiverCard != null)
                {
                    targetHover = true;
                    if (lClick)
                    {
                        _target.ActivateCheat_RiverTarget(_selectedCheat.Cheat, hoveredRiverCard.Index);
                        Service.AudioController.Play("CheatPlay");
                        Timing.RunCoroutine(OnActivatedCheat());
                        return;
                    }
                }
                
                // Targeting for "self-targeted" cards
                var selfTargeted = _selectedCheat.Cheat.Targeting == CheatCard.TargetingType.Self;
                if (selfTargeted)
                {
                    targetHover = allowActivation && !RectTransformUtility.RectangleContainsScreenPoint(_Container, Input.mousePosition, null);
                    _selectedCheat.ForceNoGlow(!targetHover);
                    if (targetHover && lClick)
                    {
                        _target.ActivateCheat_SelfTarget(_selectedCheat.Cheat);
                        Service.AudioController.Play("CheatPlay");
                        Timing.RunCoroutine(OnActivatedCheat());
                        return;
                    }
                }
                
                // Check for deselection input (right click always, left click if off-card)
                bool deselectionZone = (!cardHover || selfTargeted && !selectedNewCheat) && !targetHover;
                if (deselectionZone && lClick || rClick)
                {
                    SelectCheat(null);
                }
                
                _CheatReticle.SetTargetValid(targetHover);
            }
            else
            {
                _bigHandDisplay.ManagedUpdate(false, out var unused1);
                _riverDisplay.ManagedUpdate(false, out var unused2);
                
                // Check for "close sleeve" input
                var sleeveMouseOver = RectTransformUtility.RectangleContainsScreenPoint(_Container, Input.mousePosition, null);
                if (!sleeveMouseOver && _SleeveButton.interactable && lClick || rClick)
                {
                    Close();
                }
            }
        }
        
        private void SelectCheat(CheatCardDisplay target)
        {
            // Update cards on selection state
            int slotIndex = 0;
            foreach (var card in _displayedCards)
            {
                card.ShowSelected(card == target, _CardSlots[slotIndex].transform, _SelectedCheatAnchor);
                slotIndex++;
            }
            
            // Set selection for remaining cards
            if (target != null)
            {
                if (target.Cheat.SelfTargeted) _CheatReticle.Hide();
                else _CheatReticle.Show(target);
                _Scrim.gameObject.SetActive(true);
                _selectedAnim.PlayForward();
                _SleeveButton.interactable = false;
            }
            else
            {
                _CheatReticle.Hide();
                _Scrim.gameObject.SetActive(false);
                _selectedAnim.PlayBackwards();
            }
            
            _selectedCheat = target;
        }
        
        private IEnumerator<float> OnActivatedCheat()
        {
            _SleeveButton.interactable = false;
            Service.SimpleTutorial.SuppressBeat("TRY_CHEATING");
            var activatedCheat = _selectedCheat;
            _displayedCards.Remove(_selectedCheat);
            _selectedCheat = null;
            
            // Play cheat activation anim
            if (activatedCheat.Cheat.SelfTargeted)
            {
                yield return Timing.WaitUntilDone(activatedCheat.ActivateAnim());
            }
            
            // Discard cheat
            const float DISCARD_DURATION = .4f;
            _CheatReticle.Hide();
            activatedCheat.DiscardAnim(DISCARD_DURATION, _DiscardAnchor);
            Service.AudioController.Play("CheatDiscard");
            
            yield return Timing.WaitForSeconds(DISCARD_DURATION);
            
            // Draw new cheat
            const float DRAW_DURATION = .25f;
            _DeckIcon.SetCount(_target.CheatDeck.Count());
            foreach (var cheat in _target.CheatHand)
                if (_displayedCards.All(x => x.Cheat != cheat))
                {
                    SpawnCard(cheat).DrawAnim(DRAW_DURATION); // Draw always appears to happen in slot 0 -- slot is reassigned after sleeve close
                    Service.AudioController.Play("CheatDraw");
                }
            
            yield return Timing.WaitForSeconds(DRAW_DURATION);
            yield return Timing.WaitForOneFrame;
            
            // Close sleeve
            const float CLOSE_DURATION = .2f;
            Service.AudioController.Play("SleeveClose");
            _Container.DOAnchorPosX(_ClosedX, CLOSE_DURATION).SetEase(Ease.InQuad);
            
            yield return Timing.WaitForSeconds(CLOSE_DURATION);
            yield return Timing.WaitForOneFrame;
            
            // Rearrange cards in hand
            int slotIndex = 0;
            foreach (var card in _displayedCards)
            {
                card.SetSlot(_CardSlots[slotIndex].transform);
                slotIndex++;
            }
            
            // Reset state
            InitStateAnims();
            _SleeveButton.interactable = true;
            _SleeveButton.SetListener(Open);
            _open = false;
        }
        
        private void Open()
        {
            // When first opening the sleeve, show the cheat cards spawning in
            if (_firstOpen)
            {
                Timing.RunCoroutine(CardSpawnAnim());
                IEnumerator<float> CardSpawnAnim()
                {
                    yield return Timing.WaitForSeconds(.2f);
                    foreach (var card in _displayedCards)
                    {
                        card.DrawAnim(.25f);
                        Service.AudioController.Play("CheatDraw");
                        yield return Timing.WaitForSeconds(.15f);
                    }
                }
                _firstOpen = false;
            }
            
            Service.AudioController.Play("SleeveOpen");
            _SleeveButton.interactable = false;
            _SleeveButton.SetListener(Close);
            _openAnim.PlayForward();
            _open = true;
        }
        
        private void Close()
        {
            if (!_open) return;
            if (_selectedCheat != null)
            {
                SelectCheat(null);
                return;
            }
            
            Service.AudioController.Play("SleeveClose");
            _SleeveButton.interactable = false;
            _SleeveButton.SetListener(Open);
            _openAnim.PlayBackwards();
            _open = false;
        }
        
        private CheatCardDisplay SpawnCard(CheatCard cheat, int? slot = null)
        {
            var newCard = LeanPool.Spawn(prefab_CheatCard);
            newCard.SetSlot(slot != null ? _CardSlots[slot.Value].transform : _DrawnCheatAnchor);
            newCard.Initialize(cheat);
            _displayedCards.Add(newCard);
            return newCard;
        }
        
        private void InitStateAnims()
        {
            const float HOVER_DURATION = .1f;
            const float SELECT_DURATION = .2f;
            const float OPEN_DURATION = .25f;
            
            DOTween.Kill(_hoverAnim);
            DOTween.Kill(_selectedAnim);
            DOTween.Kill(_openAnim);
            
            var widthOffset = 214 * (_cheatSlots - 3); // Workaround for the fact that values based on the right-hand side of sleeve were chosen with exactly 3 fixed slots
            _Container.SetAnchorX(_ClosedX);
            _DeckIcon.rectTransform.SetAnchorX(_DefaultDeckX + widthOffset);
            foreach (var slot in _CardSlots) slot.alpha = 1f;
            _Scrim.gameObject.SetActive(false);
            
            _hoverAnim = _Container.DOBlendableLocalMoveBy(new Vector3(-8, 0, 0), HOVER_DURATION).SetEase(Ease.InOutQuad)
                .SetAutoKill(false).Pause();
            _selectedAnim = DOTween.Sequence()
                .Join(_Container.DOBlendableLocalMoveBy(new Vector3(_SelectedX - (_OpenX - widthOffset), 0, 0), SELECT_DURATION).SetEase(Ease.InOutQuad))
                .Join(DOVirtual.Float(1f, 0f, SELECT_DURATION, t => { foreach (var slot in _CardSlots) slot.alpha = t; }))
                .Join(_DeckIcon.rectTransform.DOAnchorPosX(_SelectedDeckX, SELECT_DURATION).SetEase(Ease.InOutQuad))
                .Join(_DeckIcon.CanvasGroup.DOFade(0f, SELECT_DURATION * .5f).From(1f).SetEase(Ease.OutQuad).SetLoops(2))
                .OnRewind(() => _SleeveButton.interactable = true)
                .SetAutoKill(false).Pause();
            _openAnim = _Container.DOBlendableLocalMoveBy(new Vector3((_OpenX - widthOffset) - _ClosedX + 10, 0, 0), OPEN_DURATION).SetEase(Ease.OutQuad)
                .OnRewind(() => _SleeveButton.interactable = true)
                .OnComplete(() => _SleeveButton.interactable = true)
                .SetAutoKill(false).Pause();
        }
    }
}