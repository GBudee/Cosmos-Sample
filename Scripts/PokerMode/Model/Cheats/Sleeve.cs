using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Managers;
using Shapes;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace PokerMode.Cheats
{
    public class Sleeve : ISaveable
    {
        public IEnumerable<CheatCard> Hand => _hand;
        public IEnumerable<CheatCard> Deck => _deck;
        public IEnumerable<CheatCard> Discard => _discard;
        public int Gumption { get; set; }
        public int CheatSlots { get; private set; }
        public int CheatCount => _hand.Count + _deck.Count + _discard.Count;
        
        private List<CheatCard> _hand = new();
        private List<CheatCard> _deck = new();
        private List<CheatCard> _discard = new();
        
        public void LoadDefault()
        {
            Gumption = 3;
            CheatSlots = 1;
            
            // Populate deck
            _deck.Add(new Cheat_RiverRedraw());
#if UNITY_EDITOR
            if (SceneManager.GetActiveScene().name == "NewRenoStation")
            {
                CheatSlots = 5;
                _deck.Add(new Cheat_RiverRedraw());
                _deck.Add(new Cheat_PocketAce());
                _deck.Add(new Cheat_AllemandeLeft());
                _deck.Add(new Cheat_RiverDown());
                _deck.Add(new Cheat_HandUp());
                //_deck.Add(new Cheat_NaNHand());
                //_deck.Add(new Cheat_RiverDown());
                //_deck.Add(new Cheat_RiverUp());
                //_deck.Add(new Cheat_SwapToRiver());
            }
#endif
            ShuffleDeck();
            
            // Draw a hand of cards
            int deckCount = _deck.Count;
            for (int i = 0; i < Mathf.Min(deckCount, CheatSlots); i++) DrawCard();
        }
        
        public void Save(int version, BinaryWriter writer, bool changingScenes)
        {
            writer.Write(Gumption);
            writer.Write(CheatSlots);
            writer.WriteList(_hand, x => x.GetType().ToString());
            writer.WriteList(_deck, x => x.GetType().ToString());
            writer.WriteList(_discard, x => x.GetType().ToString());
        }
        
        public void Load(int version, BinaryReader reader)
        {
            Gumption = reader.ReadInt32();
            CheatSlots = reader.ReadInt32();
            reader.ReadList(_hand, x => Activator.CreateInstance(Type.GetType(x)!) as CheatCard);
            reader.ReadList(_deck, x => Activator.CreateInstance(Type.GetType(x)!) as CheatCard);
            reader.ReadList(_discard, x => Activator.CreateInstance(Type.GetType(x)!) as CheatCard);
        }
        
        public void DiscardCard(CheatCard card)
        {
            _hand.Remove(card);
            _discard.Add(card);
        }
        
        public void DrawCard()
        {
            if (_deck.Count == 0)
            {
                DiscardToDeck();
                ShuffleDeck();
            }
            if (_deck.Count > 0)
            {
                var toDraw = _deck[0];
                _hand.Add(toDraw);
                _deck.RemoveAt(0);
            }
        }
        
        public void AddCheat(CheatCard cheat)
        {
            if (_hand.Count < CheatSlots) _hand.Add(cheat);
            else
            {
                _deck.Add(cheat);
                ShuffleDeck();
            }
        }

        public void RemoveCheat(CheatCard cheat)
        {
            if (_hand.Contains(cheat))
            {
                _hand.Remove(cheat);
                DrawCard();
            }
            else if (_deck.Contains(cheat))
            {
                _deck.Remove(cheat);
            }
            else if (_discard.Contains(cheat))
            {
                _discard.Remove(cheat);
            }
        }
        
        public void AddCheatSlot()
        {
            CheatSlots++;
            DrawCard();
        }

        private void DiscardToDeck()
        {
            // Discard -> deck
            foreach (var cheat in _discard) _deck.Add(cheat);
            _discard.Clear();
        }
        
        private void ShuffleDeck()
        {
            // Shuffle deck
            int n = _deck.Count;
            for (int i = 0; i < (n - 1); i++)
            {
                int toSwap = i + UnityEngine.Random.Range(0, n - i);
                (_deck[toSwap], _deck[i]) = (_deck[i], _deck[toSwap]);
            }
        }
    }
}
