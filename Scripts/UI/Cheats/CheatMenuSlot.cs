using Lean.Pool;
using PokerMode.Cheats;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Cheats
{
    public class CheatMenuSlot : MonoBehaviour, IPoolable
    {
        [SerializeField] private CheatCardDisplay prefab_CheatCard;
        [SerializeField] private Image _InHandIcon;
        
        public RectTransform RectTransform => transform as RectTransform;
        public CheatCardDisplay DisplayedCard { get; private set; }
        public State CardState { get; private set; }
        
        public enum State { None, InSleeve, InDeck, Discarded }

        public void Initialize(CheatCard card, State state)
        {
            CardState = state;
            
            bool showIcon = state == State.InSleeve;
            if (card != null)
            {
                var newCard = LeanPool.Spawn(prefab_CheatCard, transform);
                newCard.Initialize(card, show: true, castable: state != State.Discarded);
                DisplayedCard = newCard;
                
                if (showIcon) _InHandIcon.transform.SetSiblingIndex(transform.childCount - 1);
            }
            
            _InHandIcon.gameObject.SetActive(showIcon);
        }
        
        public void ManagedUpdate(out bool mouseOver)
        {
            mouseOver = false;
            DisplayedCard?.ManagedUpdate(out mouseOver, CheatCardDisplay.Mode.Menu);
        }
        
        public void OnSpawn() { }
        
        public void OnDespawn() 
        { 
            if (DisplayedCard != null) LeanPool.Despawn(DisplayedCard);
            DisplayedCard = null;
        }

        public void ShowSelected(bool value)
        {
            DisplayedCard?.ShowSelected(value);
        }
    }
}