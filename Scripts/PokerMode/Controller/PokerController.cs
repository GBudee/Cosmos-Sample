using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using static PokerMode.TableController;

namespace PokerMode
{
    [ExecuteBefore(typeof(GameController))]
    public class PokerController : MonoBehaviour, ISaveable
    {
        [SerializeField] private List<TableController> _PokerTables;
        [FormerlySerializedAs("_CheatController")] [SerializeField] private CosmoController _CosmoController;
        
        public TableController ActiveTable { get; private set; }
        public IEnumerable<TableController> Tables => _PokerTables;
        
        void Awake()
        {
            // Find active table in scene
            bool foundActiveTable = false;
            foreach (var table in _PokerTables)
                if (table.enabled)
                {
                    if (foundActiveTable)
                    {
                        Debug.LogError("Please disable all but one poker table.");
                        break;
                    }
                    
                    ActiveTable = table;
                    foundActiveTable = true;
                }
        }
        
        public void Initialize(bool loadedSave)
        {
            if (Service.GameController.CurrentMode != GameController.GameMode.Poker) ActiveTable = null;
            
            // Tell non-active tables to play auto-poker
            foreach (var table in _PokerTables.Where(table => table != ActiveTable))
                table.Leave();
            
            // Join the active table
            _CosmoController.Initialize(ActiveTable, loadedSave);
            
            Service.AudioController.SetRadioSpatialization(ActiveTable == null);
            if (ActiveTable != null) Service.AudioController.ActivateLayer(ActiveTable.gameObject);
        }
        
        public void Save(int version, BinaryWriter writer, bool changingScenes)
        {
            _CosmoController.Save(version, writer, changingScenes);
            writer.Write(changingScenes);
            if (!changingScenes)
            {
                writer.Write(_PokerTables.Count);
                foreach (var table in _PokerTables) table.Save(version, writer);
            }
        }
        
        public void Load(int version, BinaryReader reader)
        {
            _CosmoController.Load(version, reader);
            var changedScenes = reader.ReadBoolean();
            if (!changedScenes)
            {
                var tableCount = reader.ReadInt32();
                for (int i = 0; i < tableCount; i++) _PokerTables[i].Load(version, reader);
            }
        }
        
        public void JoinTable(TableController table)
        {
            Debug.Assert(ActiveTable == null, "Can't join a poker table with one already active");
            Service.AudioController.SetRadioSpatialization(false);
            Service.AudioController.ActivateLayer(table.gameObject);
            _CosmoController.JoinTable(table);
            
            ActiveTable = table;
        }
        
        public void LeaveTable()
        {
            Service.AudioController.SetRadioSpatialization(true);
            Service.AudioController.DeactivateLayer(ActiveTable.gameObject);
            _CosmoController.LeaveTable();
            
            ActiveTable = null;
        }
    }
}