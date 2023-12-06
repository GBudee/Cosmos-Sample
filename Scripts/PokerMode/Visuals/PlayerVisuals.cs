using System;
using Animations;
using PokerMode.Dialogue;
using PokerMode.Visuals.Reactions;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;
using static Utilities.MoreLinq;
using Action = UI.PlayerDisplay.Action;
using Random = UnityEngine.Random;

namespace PokerMode
{
    public class PlayerVisuals : MonoBehaviour
    {
        private static readonly int Reset = Animator.StringToHash("Reset");
        
        private static readonly int Draw = Animator.StringToHash("Draw");
        private static readonly int Check = Animator.StringToHash("Check");
        private static readonly int Bid = Animator.StringToHash("Bid");
        private static readonly int AllIn = Animator.StringToHash("AllIn");
        private static readonly int Fold = Animator.StringToHash("Fold");
        private static readonly int Showdown = Animator.StringToHash("Showdown");
        private static readonly int WinShowdown = Animator.StringToHash("WinShowdown");
        private static readonly int EndShowdown = Animator.StringToHash("EndShowdown");
        private static readonly int IdleOffset = Animator.StringToHash("IdleOffset");

        [Header("Character References")] 
        [SerializeField] private AnimEventHelper _AnimEventHelper;
        [SerializeField] private Animator _Animator;
        [SerializeField] private Transform _RigHand;
        
        [Header("Generic References")] 
        [SerializeField] private ReactionController _ReactionController;
        [SerializeField] private HandVisuals _HandVisuals;
        [FormerlySerializedAs("_CreditPile")] [FormerlySerializedAs("_CreditDisplay")] [SerializeField] private CreditVisuals _CreditVisuals;
        [SerializeField] private TurnIndicator _TurnIndicator;
        [SerializeField] private Transform _DealerButtonAnchor;
        [Header("Values")] 
        [SerializeField] private string _Name;
        [SerializeField] private Archetype _Archetype;
        [SerializeField] private bool _IsHuman;
        [SerializeField] private string _Trait;
        [SerializeField] private string _ObjectiveLabel;
        [SerializeField] private bool _BadLiar;
        [SerializeField] private int _DEBUG_CustomStartingCash;
        
        public enum Archetype { None, Naive, Mid, Shark }
        
        public HandVisuals HandVisuals => _HandVisuals;
        public CreditVisuals CreditVisuals => _CreditVisuals;
        public TurnIndicator TurnIndicator => _TurnIndicator;
        public ReactionController ReactionController => _ReactionController;
        public Transform DealerButtonAnchor => _DealerButtonAnchor;
        public string Name => _Name;
        public bool IsHuman => _IsHuman;
        public string Trait => _Trait;
        
        public (Action action, int credits) DisplayAction { get; private set; }
        public int DEBUG_CustomStartingCash => _DEBUG_CustomStartingCash;
        
        private bool _showdownAnim;

        void Start()
        {
            if (_Animator != null) _Animator.SetFloat(IdleOffset, Random.Range(0f, 1f));
            if (_ReactionController != null) _ReactionController.Initialize(_Archetype.ToString());
            if (_HandVisuals != null) _HandVisuals.Initialize(_RigHand);
        }
        
        public void Draw_Anim()
        {
            if (_Animator != null) _Animator.SetTrigger(Draw);
        }
        
        public void EnterShowdown_Anim(bool win, string callbackLabel, System.Action callback)
        {
            if (_Animator != null)
            {
                _AnimEventHelper.Register(callbackLabel, callback);
                _Animator?.SetBool(WinShowdown, win);
                _Animator?.SetTrigger(Showdown);
                _showdownAnim = true;
            }
        }
        
        public void ExitShowdown_Anim()
        {
            if (_Animator != null && _showdownAnim)
            {
                _Animator?.SetTrigger(EndShowdown);
                _showdownAnim = false;
            }
        }
        
        public void ShowBet(Action action, int bet, int playerCredits, CreditVisuals potVisuals, int potCredits, GameObject audioLayer)
        {
            ShowAction(playerCredits == 0 ? Action.AllIn : action, bet, "ChipUpdate",
                () =>
                {
                    CreditVisuals.Show(playerCredits);
                    Service.AudioController.Play("Bid", CreditVisuals.transform.position, randomizer: 3);
                    potVisuals.PlayPotIncreaseSound(potCredits, audioLayer);
                    potVisuals.Show(potCredits);
                });
        }
        
        public void ShowAction(Action action, int credits = 0, string callbackLabel = null, System.Action callback = null)
        {
            // Play animations (for ai players)
            bool registeredCallback = false;
            if (_Animator != null)
                switch (action)
                {
                    case Action.Check:
                        PlayAnim(Check);
                        break;
                    case Action.Call:
                    case Action.Bet:
                    case Action.Raise:
                        PlayAnim(Bid);
                        break;
                    case Action.AllIn:
                        PlayAnim(AllIn);
                        break;
                    case Action.Fold:
                        PlayAnim(Fold);
                        break;
                }
            
            void PlayAnim(int animID)
            {
                _AnimEventHelper.Register(callbackLabel, callback);
                _Animator.SetTrigger(animID);
                registeredCallback = true;
            }
            
            // Animation-callback should be executed immediately if it was not registered
            if (callback != null && !registeredCallback) callback();
            
            // Flag action for UI display
            if (action == Action.Fold) HandVisuals.Folded = true;
            DisplayAction = (action, credits);
        }
        
        public void ChangeActionCredits(int credits)
        {
            DisplayAction = (DisplayAction.action, credits);
        }
        
        public void ClearAction() => DisplayAction = default;
        
        public void ResetState(bool notPlaying)
        {
            HandVisuals.Revealed = false;
            HandVisuals.Folded = false;
            if (notPlaying) ShowAction(Action.Lost);
            else ClearAction();
            ReactionController.Clear();
        }
        
        public void KillAnimations()
        {
            if (_Animator != null)
            {
                _AnimEventHelper.Clear();
                foreach (var animID in CreateEnumerable(Check, Bid, AllIn, Fold, Showdown, EndShowdown))
                    _Animator.ResetTrigger(animID);
                _Animator.SetTrigger(Reset);
            }
        }
    }
}