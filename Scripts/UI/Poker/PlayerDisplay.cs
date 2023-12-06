using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PokerMode;
using PokerMode.Traits;
using PokerMode.Visuals.Reactions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

namespace UI
{
    public class PlayerDisplay : MonoBehaviour
    {
        public enum Action { None, SmallBlind, BigBlind, Check, Call, Bet, Raise, AllIn, Win, Tie, SidePot, Fold, Lost }
        
        [Serializable]
        private struct ActionColor
        {
            public Action Action;
            public Color Color;
        }
        
        [Header("Backgrounds")]
        [SerializeField] private Image _HumanBackground;
        [SerializeField] private List<Image> _AIBackgroundOptions;
        [Header("Contents")]
        [SerializeField] private TMP_Text _Name;
        [SerializeField] private TMP_Text _Nickname;
        [SerializeField] private GameObject _TraitIconContainer;
        [SerializeField] private TooltipTarget _TraitTooltip;
        [SerializeField] private TutorialTarget _TraitHint;
        [SerializeField] private CharacterBackstories _CharacterBackstoriesSO;
        [FormerlySerializedAs("_CreditDisplay")] [SerializeField] private NumberDisplay _NumberDisplay;
        [SerializeField] private CanvasGroup _ActionGroup;
        [SerializeField] private TMP_Text _ActionText;
        [SerializeField] private Image _ActionBackground;
        [SerializeField] private Image _ActionScrim;
        [SerializeField] private ParticleSystem _WinParticle;
        [FormerlySerializedAs("_Minihand")] [SerializeField] private MiniHandDisplay _MiniHandDisplay;
        
        [Header("Colors")] 
        [SerializeField] private Color _Background_Faded;
        [SerializeField] private Color _NameText_Normal;
        [SerializeField] private Color _NameText_Faded;
        [SerializeField] private float _ActionGroup_Faded = .3f;
        [SerializeField] private List<ActionColor> _ActionColors;
        
        private PlayerVisuals _target;
        private (Action action, int credits) _lastAction;
        private Sequence _foldFader;

        private void Awake()
        {
            const float DELAY = .5f;
            const float DURATION = .5f;
            _foldFader = DOTween.Sequence().SetTarget(this).SetAutoKill(false).Pause()
                .Insert(DELAY, _Name.DOColor(_NameText_Faded, DURATION).From(_NameText_Normal))
                .Insert(DELAY, _ActionGroup.DOFade(_ActionGroup_Faded, DURATION).From(1f));
            foreach (var background in _AIBackgroundOptions.Concat(_HumanBackground.ToEnumerable()))
                _foldFader.Insert(DELAY, background.DOColor(_Background_Faded, DURATION).From(Color.white));
        }
        
        public void Initialize(PlayerVisuals target, int index = -1)
        {
            _NumberDisplay.Initialize(() => target.CreditVisuals.Credits);
            _MiniHandDisplay.Initialize(target.HandVisuals);
            
            // Reset visual state
            _Name.text = target.Name;
            if (string.IsNullOrEmpty(target.Trait))
            {
                // No ai trait
                _TraitIconContainer.SetActive(false);
                _Nickname.gameObject.SetActive(false);
                if (target.IsHuman)
                {
                    _TraitTooltip.enabled = false;
                }
                else
                {
                    var characterName = target.Name;
                    string characterDescription = _CharacterBackstoriesSO.GetBackstory(characterName);
                    _TraitTooltip.SetDynamicContents(() => characterName, () => characterDescription);
                    _TraitTooltip.enabled = true;
                }
            }
            else
            {
                // Has ai trait, requiring nickname, skull icon, and tooltip
                var (name, cheatDesc) = Trait.GetDescription(target.Trait);
                _Nickname.text = $"\"{name}\"";
                
                var characterName = target.Name;
                string characterDescription = $"{_CharacterBackstoriesSO.GetBackstory(characterName)}" +
                                              $"\n\n<i><color=white>Cheater!</color></i>" +
                                              $"\n{cheatDesc}";
                _TraitTooltip.SetDynamicContents(() => characterName, () => characterDescription);
                _TraitHint.RegisterCustomKey($"TRAIT_{target.Trait.ToUpper()}", () => "HINT: New Cheater!", () => cheatDesc);
                
                _TraitIconContainer.SetActive(true);
                _Nickname.gameObject.SetActive(true);
                _TraitTooltip.enabled = true;
            }
            _foldFader.Rewind();
            _ActionGroup.gameObject.SetActive(false);
            
            // Set correct background
            _HumanBackground?.gameObject.SetActive(index == -1);
            for (int i = 0; i < 7; i++) _AIBackgroundOptions[i].gameObject.SetActive(i == index);
            
            _lastAction = default;
            _target = target;
        }
        
        void LateUpdate()
        {
            if (_target == null) return;
            
            if (_target.DisplayAction != _lastAction)
            {
                var newCredits = _target.DisplayAction.credits;
                var newAction = _target.DisplayAction.action;
                
                if (newAction == Action.None)
                {
                    // Hide action group and win particle)
                    _ActionGroup.DOFade(0f, .3f).OnComplete(() =>
                    {
                        _ActionGroup.gameObject.SetActive(false);
                        _foldFader.PlayBackwards();
                    });
                    _WinParticle.Stop();
                }
                else
                {
                    // Update action message and color
                    var messageText = newAction switch
                    {
                        Action.SmallBlind => "SM. BLIND",
                        Action.BigBlind => "BIG BLIND",
                        Action.AllIn => "ALL IN",
                        Action.Win => "WIN!",
                        Action.SidePot => "SIDE POT",
                        _ => newAction.ToString().ToUpper(),
                    };
                    if (newCredits != 0) messageText += $" {(newAction is Action.Win or Action.Tie or Action.SidePot ? (newCredits >= 0 ? "+" : "-") : "")}{CreditVisuals.CREDIT_SYMBOL}{Mathf.Abs(newCredits)}";
                    var actionColor = _ActionColors.FirstOrDefault(x => x.Action == newAction).Color;
                    _ActionText.text = messageText;
                    _ActionBackground.color = actionColor;
                    
                    // Show action group
                    _ActionGroup.DOKill();
                    _ActionGroup.gameObject.SetActive(true);
                    _ActionGroup.alpha = 1f;
                    
                    // Flash white scrim
                    _ActionScrim.color = new Color(1, 1, 1, .85f);
                    _ActionScrim.DOKill();
                    _ActionScrim.DOFade(0f, .65f).SetEase(Ease.OutQuad);
                    
                    // Play foldFader if appropriate
                    if (newAction is Action.Fold or Action.Lost) _foldFader.PlayForward();
                    else _foldFader.PlayBackwards();
                    
                    // Win particle
                    if (newAction is Action.Win) _WinParticle.Play();
                    else _WinParticle.Stop();
                }
                
                _lastAction = _target.DisplayAction;
            }
        }
    }
}