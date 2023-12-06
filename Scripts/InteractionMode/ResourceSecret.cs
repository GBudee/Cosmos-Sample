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
    public class ResourceSecret : InteractionDescription
    {
        [SerializeField] private string _SecretLabel;
        [SerializeField] private int _Exp;
        [SerializeField] private int _Credits;
        [SerializeField] private ParticleSystem _ParticleSystem;
        [SerializeField] private NavigationTrigger _NavigationTrigger;
        
        public override void Initialize(CosmoController cosmoController)
        {
            if (!cosmoController.HasSecret(_SecretLabel) && _ParticleSystem != null) _ParticleSystem.Play();
            else _NavigationTrigger.HideText();
        }
        
        public override IEnumerator<float> Implementation(CosmoController cosmoController, InteractionHud hud)
        {
            string headerText;
            bool allowConfirm = !cosmoController.HasSecret(_SecretLabel);
            if (allowConfirm)
            {
                headerText = _Exp > 0 ? $"You found {_Exp} experience!" : $"You found {CreditVisuals.CREDIT_SYMBOL}{_Credits}!";
                
                cosmoController.Credits += _Credits;
                cosmoController.GainXP(_Exp);
                cosmoController.RegisterEarnedSecret(_SecretLabel);
                Service.GameController.SaveGame();
            
                if (_ParticleSystem != null) _ParticleSystem.Stop();
                _NavigationTrigger.HideText();
            }
            else headerText = "You already found this secret...";
            var cancelText = "EXIT";
            
            // Player input
            bool exit = false;
            bool inputReady = false;
            hud.DynamicMenu(headerText, cancelText, null, false, inputAction: buttonIndex =>
            {
                exit = buttonIndex == 0;
                inputReady = true;
            });
            yield return Timing.WaitUntilTrue(() => inputReady);

            Service.GameController.SetMode(GameController.GameMode.Navigation);
        }
    }
}