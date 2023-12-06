using System.Collections.Generic;
using MEC;
using NavigationMode;
using PokerMode;
using PokerMode.Cheats;
using UI.Interaction;
using UI.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace InteractionMode
{
    public class TravelInteraction : InteractionDescription
    {
        [SerializeField] private EscapeMenu _EscapeMenu;

        void Awake()
        {
            if (_EscapeMenu == null) Debug.LogError("TravelInteraction missing reference to EscapeMenu", gameObject);
        }
        
        public override IEnumerator<float> Implementation(CosmoController cosmoController, InteractionHud hud)
        {
            yield return Timing.WaitForSeconds(.5f);
            
            // Player input
            bool exit = false;
            bool inputReady = false;
            hud.TravelMenu.OnSelectionChanged = UpdateMenu;
            hud.TravelMenu.Show(cosmoController.HasUnlockedPlanet);
            void UpdateMenu()
            {
                var selectedLocal = hud.TravelMenu.Selected == SceneManager.GetActiveScene().name;
                var headerText = $"{(selectedLocal ? "Current location: " : "Travel to ")}<b>{PlanetName(hud.TravelMenu.Selected)}</b>{(selectedLocal ? "." : "?")}";
                var unlockThreshold = hud.TravelMenu.Selected switch
                {
                    "TheBlackHole" => 5000,
                    "FarOutDiner" => 200000,
                    "NewRenoStation" => 10000000,
                    _ => 0
                };
                bool canAccess = cosmoController.HasUnlockedPlanet(hud.TravelMenu.Selected) || cosmoController.Credits >= unlockThreshold;
                hud.DynamicMenu(headerText, "EXIT", canAccess ? "TRAVEL" : "NOT YET", !selectedLocal && canAccess, inputAction: buttonIndex =>
                {
                    exit = buttonIndex == 0;
                    inputReady = true;
                });
            }
            yield return Timing.WaitUntilTrue(() => inputReady);
            hud.TravelMenu.DisableSelection();
            
            // Exit InteractionMode
            if (exit)
            {
                Service.GameController.SetMode(GameController.GameMode.Navigation);
                yield break;
            }
            else
            {
                var targetPlanet = hud.TravelMenu.Selected;
                if (!cosmoController.HasUnlockedPlanet(targetPlanet)) cosmoController.UnlockPlanet(targetPlanet);
                
                // Save and load
                Service.GameController.SaveGame(changingScenes: true);
                _EscapeMenu.LoadScene(targetPlanet);
            }
        }
        
        private static string PlanetName(string label)
        {
            return label switch
            {
                "BuzzGazz" => "Buzz's Gazz",
                "TheBlackHole" => "The Black Hole Donut Shop",
                "FarOutDiner" => "The Far Out Diner",
                "NewRenoStation" => "New Reno Station",
                _ => label
            };
        }
    }
}