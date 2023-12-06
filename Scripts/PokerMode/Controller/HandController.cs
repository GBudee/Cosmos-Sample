using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using UI;
using UnityEngine;

namespace PokerMode
{
    public class HandController : MonoBehaviour
    {
        [SerializeField] private DealerButton _DealerButton;
        [Header("Subsystem")]
        [SerializeField] private RoundController _RoundController;
        
        public RoundController RoundController => _RoundController;
        
        public IEnumerator<float> PlayHand(Table table, PokerHud pokerHud, CreditVisuals potVisuals, RiverVisuals riverVisuals, bool fullPlay, Action<bool> allowCheats)
        {
            table.StartHand(out var shuffleOrder, out Player dealer, out Player smallBlind, out Player bigBlind);
            foreach (var player in table.Players)
                if (!player.NotPlaying)
                    player.Trait?.OnDraw(player);
            
            // Assign dealer
            _DealerButton.GoToPlayer(dealer.Visuals);
            
            // Shuffle deck
            yield return Timing.WaitForSeconds(.1f);
            yield return Timing.WaitUntilDone(table.Deck.Visuals.Shuffle_Anim(shuffleOrder));
            
            // Draw starting hands
            foreach (var player in table.Players.Where(x => !x.NotPlaying))
            {
                player.Visuals.Draw_Anim();
                yield return Timing.WaitForSeconds(.05f);
                int handIndex = 0;
                foreach (var card in player.Hand)
                {
                    // Animate card draw (allow timing overlap)
                    Timing.RunCoroutine(card.Visuals.DeckToHand_Anim(table.Deck.Visuals, player.Visuals.HandVisuals
                        , handIndex, resultHandSize: Player.HAND_SIZE, humanHand: player.IsHuman));
                    yield return Timing.WaitForSeconds(.15f);
                    handIndex++;
                }
                player.EstimatedWinrate = fullPlay ? AIEvaluator.EstimateWinrate(table, player) : .5f;
                player.Visuals.ReactionController.OnDrawCards(table, player.EstimatedWinrate);
            }
            
            // Place small blind and big blind
            smallBlind.Visuals.ShowBet(PlayerDisplay.Action.SmallBlind, smallBlind.ActiveBet, smallBlind.Credits, potVisuals, table.Pot, gameObject);
            yield return Timing.WaitForSeconds(.5f);
            bigBlind.Visuals.ShowBet(PlayerDisplay.Action.BigBlind, bigBlind.ActiveBet, bigBlind.Credits, potVisuals, table.Pot, gameObject);
            bigBlind.Trait?.OnRaiseOrBet(bigBlind, table);
            yield return Timing.WaitForSeconds(.5f);
            
            // *** PLAY ROUNDS ***
            for (int i = 0; i <= 3; i++)
            {
                var roundHandle = Timing.RunCoroutine(_RoundController.PlayRound(table, pokerHud, potVisuals, riverVisuals, startingPlayer: table.NextPlayer(bigBlind), (Round)i, fullPlay, allowCheats), gameObject);
                yield return Timing.WaitUntilDone(roundHandle);
                if (table.Players.Count(x => !x.Folded) < 2) break; // Break if all but one folded
            }
            
            // Allow human player one last opportunity to use cheat cards, if they are involved in a showdown
            bool showdown = table.Players.Count(x => !x.Folded) > 1;
            if (fullPlay && showdown && !table.Players.FirstOrDefault(x => x.IsHuman).Folded)
            {
                Service.SimpleTutorial.TriggerBeat("TRY_CHEATING");
                allowCheats(true);
                bool inputReady = false;
                pokerHud.EnterShowdownMode(interactable: true, inputAction: () =>
                {
                    allowCheats(false);
                    inputReady = true;
                });
                yield return Timing.WaitUntilTrue(() => inputReady);
                
                pokerHud.EnterShowdownMode(interactable: false);
            }
            
            table.EndHand();
        }
    }
}