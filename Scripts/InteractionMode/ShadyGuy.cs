using System.Collections.Generic;
using System.Linq;
using MEC;
using PokerMode;
using PokerMode.Cheats;
using PokerMode.Dialogue;
using UI.Cheats;
using UI.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace InteractionMode
{
    public class ShadyGuy : InteractionDescription
    {
        [SerializeField] private DialogueAnchor _DialogueAnchor;
        [SerializeField] private CheatDeckMenu _CheatDeckMenu;
        
        public override DialogueAnchor DialogueAnchor => _DialogueAnchor;
        
        private enum Mode { Main, CreditsChosen, CardChoose }
        public enum CheatOptions { None, NewCard, GainSlot, RemoveCard }

        void Awake()
        {
            if (_CheatDeckMenu == null) Debug.LogError("ShadyGuy Interaction missing _CheatDeckMenu reference", gameObject);
        }
        
        public override IEnumerator<float> Implementation(CosmoController cosmoController, InteractionHud hud)
        {
            // Configure initial text
            var expectedBuyIn = SceneManager.GetActiveScene().name switch
            {
                "BuzzGazz" => 100,
                "TheBlackHole" => 5000,
                "FarOutDiner" => 200000,
                "NewRenoStation" => 5000000,
            };
            bool tournamentMode = SceneManager.GetActiveScene().name == "NewRenoStation";
            var priorCredits = cosmoController.Credits;
            var mode = Mode.Main;
            bool allowConfirm1, allowConfirm2 = false;
            string headerText = null, cancelText, confirm1Text, confirm2Text = null;
            
            yield return Timing.WaitForSeconds(.5f);
            _DialogueAnchor.ShowDialogue(tournamentMode ? "Good luck out\nthere, Cosmo!" : "Hey there");
            yield return Timing.WaitForSeconds(.5f);
            
            while (true)
            {
                // Set up next menu state
                bool allowCreditMenu = !tournamentMode && cosmoController.Credits < expectedBuyIn;
                bool allowCardMenu = cosmoController.HasLevelUp;
                if (mode != Mode.Main) confirm2Text = null;
                switch (mode)
                {
                    default:
                        var headerIntro = SceneManager.GetActiveScene().name switch
                        {
                            "BuzzGazz" => "Eddie is a high-rollin' poker patron.",
                            "TheBlackHole" => "Hector is a super-high-rollin' poker patron -- his cousin Eddie vouched for you.",
                            "FarOutDiner" => "Tony is a mega-high-rollin' poker patron. He heard you're a real up-and-comer from his cousin Hector.",
                            "NewRenoStation" => "Eddie's real proud of how far you've come!"
                        };
                        headerText = headerIntro + (tournamentMode ? "" : " If you can't afford to buy in at the nearest table, he'll give you the credits, and if you gain enough experience, he'll teach you a new cheat.");
                        cancelText = "LEAVE";
                        confirm1Text = "CREDITS";
                        confirm2Text = "CARDS";
                        allowConfirm1 = allowCreditMenu;
                        allowConfirm2 = allowCardMenu;
                        break;
                    case Mode.CreditsChosen:
                        headerText = $"Eddie gave you {CreditVisuals.CREDIT_SYMBOL}{cosmoController.Credits - priorCredits}.";
                        cancelText = "THANKS";
                        confirm1Text = "ALSO...";
                        allowConfirm1 = true;
                        break;
                    case Mode.CardChoose:
                        cancelText = "THANKS";
                        confirm1Text = allowCreditMenu ? "ALSO..." : null;
                        confirm2Text = allowCardMenu ? "CHOOSE ANOTHER" : null;
                        allowConfirm1 = true;
                        allowConfirm2 = true;
                        break;
                }
                
                // Player input
                int buttonIndex = 0;
                bool inputReady = false;
                hud.DynamicMenu(headerText, cancelText, confirm1Text, allowConfirm1, confirm2Text, allowConfirm2, inputAction: i =>
                {
                    buttonIndex = i;
                    inputReady = true;
                });
                yield return Timing.WaitUntilTrue(() => inputReady);
                
                // Exit InteractionMode
                if (buttonIndex == 0)
                {
                    hud.DespawnCheatCards();
                    Service.GameController.SetMode(GameController.GameMode.Navigation);
                    yield break;
                }
                
                // Navigate from button inputs
                if (mode == Mode.Main)
                {
                    if (buttonIndex == 1) mode = Mode.CreditsChosen;
                    else if (buttonIndex == 2) mode = Mode.CardChoose;
                }
                else if (mode == Mode.CreditsChosen)
                {
                    mode = Mode.Main;
                    _DialogueAnchor.ShowDialogue("What else?");
                }
                else if (mode == Mode.CardChoose)
                {
                    hud.DespawnCheatCards();
                    if (buttonIndex == 1) mode = Mode.CreditsChosen;
                    else if (buttonIndex == 2) mode = Mode.CardChoose;
                }
                
                // Execute special modes
                if (mode == Mode.CreditsChosen)
                {
                    // Refill cosmo's credits
                    cosmoController.Credits = Mathf.Max(cosmoController.Credits, expectedBuyIn);
                    Service.AudioController.Play("Bid", transform.position, randomizer: 3);
                    _DialogueAnchor.ShowDialogue("Pleasure doing business\nwith ya");
                    
                    Service.GameController.SaveGame();
                }
                if (mode == Mode.CardChoose)
                {
                    // Pick a new cheat
                    _DialogueAnchor.HideDialogue();
                    const int MAX_SLOTS = 5;
                    bool offerCheatSlot = cosmoController.CheatSlots < MAX_SLOTS && cosmoController.CheatCount >= cosmoController.CheatSlots * 2;
                    bool offerRemoveCard = cosmoController.CheatSlots == MAX_SLOTS && cosmoController.CheatCount > MAX_SLOTS;
                    if (offerRemoveCard) offerRemoveCard = Random.Range(0f, 1f) > .5f;
                    Debug.Assert(!(offerCheatSlot && offerRemoveCard), "Shouldn't be eligible for both offerCheatSlot and offerRemoveCard");
                    var cheatOptions = cosmoController.GetLevelUpOptions(offerCheatSlot || offerRemoveCard ? 2 : 3).Select(card => (CheatOptions.NewCard, x: card));
                    if (offerCheatSlot) cheatOptions = cheatOptions.Prepend((CheatOptions.GainSlot, (CheatCard) null));
                    if (offerRemoveCard) cheatOptions = cheatOptions.Prepend((CheatOptions.RemoveCard, (CheatCard) null));
                    
                    // Wait for input
                    CheatOptions selection = default;
                    CheatCard cheat = null;
                    inputReady = false;
                    Timing.RunCoroutine(hud.PickACheat("Choose a new cheat to learn.", "DECLINE", cheatOptions, (s, c) =>
                    {
                        selection = s;
                        cheat = c;
                        inputReady = true;
                    }));
                    yield return Timing.WaitUntilTrue(() => inputReady);
                    
                    // Spend level for new cheat
                    if (selection == CheatOptions.None)
                    {
                        headerText = "You declined to add a cheat to your deck.";
                    }
                    else if (selection == CheatOptions.GainSlot)
                    {
                        cosmoController.AddCheatSlot();
                        headerText = $"You can now hold {cosmoController.CheatSlots} cheats in your sleeve.";
                    }
                    else if (selection == CheatOptions.NewCard)
                    {
                        cosmoController.AddCheat(cheat);
                        headerText = $"{cheat.Name} has been added to your cheat deck.";
                    }
                    else if (selection == CheatOptions.RemoveCard)
                    {
                        inputReady = false;
                        CheatCard selectedCard = null;
                        _CheatDeckMenu.Show(cosmoController, removeCardsMode: true, s =>
                        {
                            selectedCard = s;
                            inputReady = true;
                        });
                        yield return Timing.WaitUntilTrue(() => inputReady);
                        
                        yield return Timing.WaitUntilDone(hud.DiscardCheatCards());
                        if (selectedCard != null)
                        {
                            // TODO: Animate card being removed from deck
                            headerText = $"You removed {selectedCard.Name} from your deck.";
                            cosmoController.RemoveCheat(selectedCard);
                        }
                        else
                        {
                            headerText = "You declined to remove a cheat from your deck.";
                        }
                    }
                    cosmoController.SpendLevel();
                    
                    Service.GameController.SaveGame();
                }
            }
        }
    }
}