using System.Collections.Generic;
using System.IO;
using System.Linq;
using NavigationMode;
using PokerMode.Cheats;
using UI;
using UI.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace PokerMode
{
    public class CosmoController : MonoBehaviour, ISaveable
    {
        [SerializeField] private PokerHud _PokerHud;
        [SerializeField] private int _StartingCash = 200;
        
        private const int MAX_GUMPTION = 3;
        
        public IEnumerable<CheatCard> CheatHand => _sleeve.Hand;
        public IEnumerable<CheatCard> CheatDeck => _sleeve.Deck;
        public IEnumerable<CheatCard> CheatDiscard => _sleeve.Discard;
        public int Gumption => _sleeve.Gumption;
        public int CheatSlots => _sleeve.CheatSlots;
        public int CheatCount => _sleeve.CheatCount;
        public int Credits { get; set; }
        public int XP { get; private set; }
        public int XPThreshold => _level == 1 ? 100 : (Mathf.RoundToInt(Mathf.Pow(2, _level * .65f) * 2f) * 100);
        public bool HasLevelUp => _level > _spentLevel;
        public int CurrentObjective { get; set; }
        public bool FLAG_ChangedObjective { get; set; }
        public bool FLAG_CheatButtonPressed { get; set; }
        
        private TableController _currentTable;
        private bool _allowCheats;
        private int _level;
        private int _spentLevel;
        private List<string> _secrets = new();
        private List<string> _unlockedPlanets = new();
        private Sleeve _sleeve = new();
        
        public void Initialize(TableController activeTable, bool loadedSave)
        {
            if (!loadedSave)
            {
                Credits = _StartingCash;
                #if UNITY_EDITOR
                if (SceneManager.GetActiveScene().name == "NewRenoStation") Credits = 5000000;
                #endif
                _level = _spentLevel = 1;
                _sleeve.LoadDefault();
                _unlockedPlanets.Add("BuzzGazz");
                CurrentObjective = 0;
            }
            
            if (activeTable != null) JoinTable(activeTable);
            else _PokerHud.Hide();
        }
        
        public void Save(int version, BinaryWriter writer, bool changingScenes)
        {
            writer.Write(Credits + (_currentTable?.CurrentHumanCash ?? 0));
            writer.Write(_level);
            writer.Write(_spentLevel);
            writer.Write(XP);
            writer.WriteList(_secrets);
            writer.WriteList(_unlockedPlanets);
            writer.Write(CurrentObjective);
            _sleeve.Save(version, writer, changingScenes);
        }
        
        public void Load(int version, BinaryReader reader)
        {
            Credits = reader.ReadInt32();
            _level = reader.ReadInt32();
            _spentLevel = reader.ReadInt32();
            XP = reader.ReadInt32();
            reader.ReadList(_secrets);
            reader.ReadList(_unlockedPlanets);
            CurrentObjective = reader.ReadInt32();
            _sleeve.Load(version, reader);
        }
        
        public void JoinTable(TableController table)
        {
            bool CanBuyIn() => Credits >= table.BuyIn;
            void BuyIn() => Credits -= table.BuyIn;
            void CashOut(int credits) => Credits += credits;
            void GainGumption()
            {
                if (_sleeve.Gumption < MAX_GUMPTION) Service.SimpleTutorial.TriggerBeat("EARNING_GUMPTION");
                _sleeve.Gumption = Mathf.Clamp(_sleeve.Gumption + 1, 0, MAX_GUMPTION);
            }
            void LocalGainXP(int incoming)
            {
                GainXP(incoming);
                if (_level > 1) Service.SimpleTutorial.TriggerBeat("LEVEL_UP");
            }
            void AllowCheats(bool value) => _allowCheats = value;
            void WonTournament()
            {
                if (CurrentObjective == ObjectiveMenu.MAX_OBJECTIVE) return;
                CurrentObjective = ObjectiveMenu.MAX_OBJECTIVE;
                FLAG_ChangedObjective = true;
                Debug.Log("Won Tournament");
            }
            
            table.Join(_PokerHud, CanBuyIn, BuyIn, CashOut, LocalGainXP, GainGumption, AllowCheats, WonTournament);
            
            if (table.BuyIn > 100) Service.SimpleTutorial.SuppressBeat("FINISH_FIRST_TABLE");
            _currentTable = table;
        }
        
        public void LeaveTable()
        {
            Debug.Log($"Leaving table: {_currentTable.gameObject.name}");
            _currentTable.Leave();
            
            _currentTable = null;
        }
        
#if UNITY_EDITOR
        void Update()
        {
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T))
            {
                Credits += 1000000;
                GainXP(1000000);
            }
        }
#endif
        
        public void GainXP(int incoming)
        {
            XP += incoming;
            while (XP >= XPThreshold)
            {
                XP -= XPThreshold;
                _level++;
            }
            
            var effectiveCredits = Credits + (_currentTable?.CurrentHumanCash ?? 0);
            CheckObjective(effectiveCredits);
        }
        public bool HasSecret(string secret) => _secrets.Contains(secret);
        public void RegisterEarnedSecret(string secret) => _secrets.Add(secret);
        public bool HasUnlockedPlanet(string planet) => _unlockedPlanets.Contains(planet);
        public void UnlockPlanet(string planet) => _unlockedPlanets.Add(planet);
        public void SpendLevel() => _spentLevel++;
        public void AddCheat(CheatCard cheat) => _sleeve.AddCheat(cheat);
        public void RemoveCheat(CheatCard cheat) => _sleeve.RemoveCheat(cheat);
        public void AddCheatSlot() => _sleeve.AddCheatSlot();
        public bool CanCast(CheatCard cheat) => _currentTable != null && Gumption >= cheat.Cost;
        
        public IEnumerable<CheatCard> GetLevelUpOptions(int count)
        {
            // Populate possible cheats
            var options = new List<CheatCard>();
            options.Add(new Cheat_AllemandeLeft());
            options.Add(new Cheat_HandDown());
            options.Add(new Cheat_HandUp());
            options.Add(new Cheat_NaNHand());
            options.Add(new Cheat_RiverDown());
            options.Add(new Cheat_RiverRedraw());
            options.Add(new Cheat_RiverUp());
            options.Add(new Cheat_SwapToRiver());
            //options.Add(new Cheat_PocketAce());
            
            // Shuffle options
            int n = options.Count;
            for (int i = 0; i < (n - 1); i++)
            {
                int toSwap = i + Random.Range(0, n - i);
                (options[toSwap], options[i]) = (options[i], options[toSwap]);
            }
            return options.Take(count);
        }
        
        public bool AllowCheatActivation(CheatCard cheat)
        {
            var roundController = _currentTable?.HandController.RoundController;
            return roundController != null && _allowCheats && cheat.Allow(roundController.CurrentRound);
        }
        
        public void ActivateCheat_HandTarget(CheatCard cheat, int handIndex)
        {
            // Identify target card
            var table = _currentTable.Table;
            Player humanPlayer = table.Players.FirstOrDefault(x => x.IsHuman);
            var target = humanPlayer.Hand.ElementAt(handIndex);
            
            // Use cheat
            cheat.Apply(table, target);
            OnCheatActivated(cheat);
        }
        
        public void ActivateCheat_RiverTarget(CheatCard cheat, int riverIndex)
        {
            var table = _currentTable.Table;
            var target = table.River[riverIndex];
            
            cheat.Apply(table, target);
            OnCheatActivated(cheat);
        }
        
        public void ActivateCheat_SelfTarget(CheatCard cheat)
        {
            var table = _currentTable.Table;
            
            cheat.Apply(table, null);
            OnCheatActivated(cheat);
        }
        
        private void OnCheatActivated(CheatCard cheat)
        {
            _sleeve.Gumption -= cheat.Cost;
            _sleeve.DiscardCard(cheat);
            _sleeve.DrawCard();
            
            Player humanPlayer = _currentTable.Table.Players.FirstOrDefault(x => x.IsHuman);
            var newBestHand = HandEvaluator.GetBestHand(_currentTable.Table, humanPlayer);
            _PokerHud.UpdateBestHand(newBestHand);
        }
        
        private void CheckObjective(int effectiveCredits)
        {
            while (CurrentObjective < ObjectiveMenu.MAX_OBJECTIVE && effectiveCredits >= ObjectiveMenu.ObjectiveCredits(CurrentObjective))
            {
                CurrentObjective++;
                FLAG_ChangedObjective = true;
                Debug.Log($"New objective index: {CurrentObjective}");
            }
        }
    }
}