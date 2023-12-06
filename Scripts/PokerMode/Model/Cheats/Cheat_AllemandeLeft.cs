using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UI;
using UnityEngine;

namespace PokerMode.Cheats
{
    public class Cheat_AllemandeLeft : CheatCard
    {
        public override TargetingType Targeting => TargetingType.Hand;
        public override BackgroundType Background => BackgroundType.Purple;
        public override string Name => "Allemande Left";
        public override string Icon => "allemande_left";
        public override string Description => "Pick a hand card and send it clockwise.";
        public override int Cost => 1;
        
        public override void Apply(Table table, Card card)
        {
            var humanPlayer = table.HumanPlayer;
            int targetIndex = humanPlayer.Hand.ToList().IndexOf(card);
            
            var swapActions = new List<System.Action>();
            Player player, nextPlayer = humanPlayer;
            do
            {
                player = nextPlayer;
                nextPlayer = table.NextPlayer(player, skipFolded: true);
                
                // Swap cards
                var playerCard = player.Hand.ElementAt(targetIndex);
                var nextPlayerCard = nextPlayer.Hand.ElementAt(targetIndex);
                var localState = (playerCard.Rank, playerCard.Suit);
                swapActions.Add(() =>
                {
                    nextPlayerCard.Rank = localState.Rank;
                    nextPlayerCard.Suit = localState.Suit;
                    nextPlayerCard.Visuals.Initialize(nextPlayerCard.Rank, nextPlayerCard.Suit, affectedByCheat: true);
                });
            } while (nextPlayer != humanPlayer);
            foreach (var action in swapActions) action();
        }
    }
}