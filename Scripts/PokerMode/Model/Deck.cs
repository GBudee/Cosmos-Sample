using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace PokerMode
{
    public class Deck : IEnumerable<Card>
    {
        public DeckVisuals Visuals { get; private set; }
        private List<Card> _deckCards = new();
        
        public IEnumerator<Card> GetEnumerator() => _deckCards.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public Deck(DeckVisuals visuals)
        {
            // Populate deck
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                for (int i = 2; i <= 14; i++)
                    _deckCards.Add(new Card(i, suit));
            
            Visuals = visuals;
        }
        
        public Deck(Deck toSimulate, IEnumerable<Card> additionalCards)
        {
            // Populate deck based on prev deck
            foreach (var card in toSimulate) 
                _deckCards.Add(new Card(card));
            
            foreach (var card in additionalCards)
                _deckCards.Add(new Card(card));
        }

        public void ResetCards()
        {
            int i = 0;
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                for (int rank = 2; rank <= 14; rank++)
                {
                    _deckCards[i].Rank = rank;
                    _deckCards[i].Suit = suit;
                    i++;
                }
        }
        
        public void Shuffle()
        {
            int n = _deckCards.Count;
            for (int i = 0; i < (n - 1); i++)
            {
                int toSwap = i + UnityEngine.Random.Range(0, n - i);
                (_deckCards[toSwap], _deckCards[i]) = (_deckCards[i], _deckCards[toSwap]);
            }
            
            #if UNITY_EDITOR
            if (Service.GameController.DEBUG_StackDeck)
            {
                //Debug.Log($"Stacking deck: Cards {_deckCards.Count}");
                (_deckCards[^1].Rank, _deckCards[^1].Suit) = (14, Suit.Rockets);
                (_deckCards[^2].Rank, _deckCards[^2].Suit) = (6, Suit.Stars);
                
                (_deckCards[^3].Rank, _deckCards[^3].Suit) = (14, Suit.Moons);
                (_deckCards[^4].Rank, _deckCards[^4].Suit) = (13, Suit.Moons);
                
                (_deckCards[^5].Rank, _deckCards[^5].Suit) = (11, Suit.Planets);
                (_deckCards[^6].Rank, _deckCards[^6].Suit) = (8, Suit.Planets);
                
                (_deckCards[^7].Rank, _deckCards[^7].Suit) = (11, Suit.Stars);
                (_deckCards[^8].Rank, _deckCards[^8].Suit) = (6, Suit.Stars);
                
                (_deckCards[^9].Rank, _deckCards[^9].Suit) = (4, Suit.Stars);
                (_deckCards[^10].Rank, _deckCards[^10].Suit) = (9, Suit.Planets);
                
                (_deckCards[^11].Rank, _deckCards[^11].Suit) = (5, Suit.Rockets);
                (_deckCards[^12].Rank, _deckCards[^12].Suit) = (5, Suit.Moons);
                
                (_deckCards[^13].Rank, _deckCards[^13].Suit) = (8, Suit.Moons);
                (_deckCards[^14].Rank, _deckCards[^14].Suit) = (6, Suit.Stars);
                
                (_deckCards[^15].Rank, _deckCards[^15].Suit) = (5, Suit.Stars);
                (_deckCards[^16].Rank, _deckCards[^16].Suit) = (6, Suit.Rockets);
                (_deckCards[^17].Rank, _deckCards[^17].Suit) = (9, Suit.Planets);
                (_deckCards[^18].Rank, _deckCards[^18].Suit) = (6, Suit.Stars);
                (_deckCards[^19].Rank, _deckCards[^19].Suit) = (6, Suit.Stars);
            }
            #endif
        }
        
        public void Add(Card card) => _deckCards.Add(card);
        
        public void Deal(int count, List<Card> target, bool keepCards = false)
        {
            int lastToPop = Mathf.Max(0, _deckCards.Count - count);
            for (int i = _deckCards.Count - 1; i >= lastToPop; i--)
            {
                target.Add(_deckCards[i]);
                if (!keepCards) _deckCards.RemoveAt(i);
            }
        }
    }
}