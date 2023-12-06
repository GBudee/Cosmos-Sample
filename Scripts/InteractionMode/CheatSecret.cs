using System.Collections.Generic;
using MEC;
using NavigationMode;
using PokerMode;
using PokerMode.Cheats;
using UI.Interaction;
using UnityEngine;
using UnityEngine.Rendering.UI;
using Utilities;

namespace InteractionMode
{
    public class CheatSecret : InteractionDescription
    {
        [SerializeField] private string _SecretLabel;
        [SerializeField] private string _CheatName;
        [SerializeField] private ParticleSystem _ParticleSystem;
        [SerializeField] private NavigationTrigger _NavigationTrigger;
        
        public override void Initialize(CosmoController cosmoController)
        {
            if (!cosmoController.HasSecret(_SecretLabel)) _ParticleSystem.Play();
            else _NavigationTrigger.HideText();
        }
        
        public override IEnumerator<float> Implementation(CosmoController cosmoController, InteractionHud hud)
        {
            string headerText;
            bool allowConfirm = !cosmoController.HasSecret(_SecretLabel);
            headerText = allowConfirm ? "You found a new cheat card!" : "You already found this secret...";
            var confirmText = "ACQUIRE";
            var cancelText = "EXIT";
            
            CheatCard newCheat = allowConfirm ? CheatCard.CreateInstance(_CheatName) : null;
            if (newCheat != null)
            {
                hud.DynamicMenu(headerText, cancelText, confirmText, false);
                yield return Timing.WaitUntilDone(hud.RevealCheatCard(newCheat));
            }
            
            // Player input
            bool exit = false;
            bool inputReady = false;
            hud.DynamicMenu(headerText, cancelText, confirmText, allowConfirm, inputAction: buttonIndex =>
            {
                exit = buttonIndex == 0;
                inputReady = true;
            });
            yield return Timing.WaitUntilTrue(() => inputReady);
            
            // Exit InteractionMode
            if (exit)
            {
                hud.DespawnCheatCards();
            }
            else
            {
                _ParticleSystem.Stop();
                _NavigationTrigger.HideText();
                
                // Confirm accepting new cheat
                hud.DynamicMenu($"{newCheat.Name} has been added to your sleeve.", cancelText, confirmText, false);
                cosmoController.AddCheat(newCheat);
                cosmoController.RegisterEarnedSecret(_SecretLabel);
                Service.GameController.SaveGame();
                
                yield return Timing.WaitUntilDone(hud.DiscardCheatCards());
            }
            
            Service.GameController.SetMode(GameController.GameMode.Navigation);
        }
    }
}