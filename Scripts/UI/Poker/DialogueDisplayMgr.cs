using System.Collections.Generic;
using System.Linq;
using Lean.Pool;
using PokerMode;
using PokerMode.Dialogue;
using UnityEngine;

namespace UI
{
    public class DialogueDisplayMgr : MonoBehaviour
    {
        [SerializeField] private List<DisplayPrefab> _DisplayPrefabs;
        
        [System.Serializable]
        public struct DisplayPrefab
        {
            public DialogueAnchor.Orientation Orientation;
            public DialogueDisplay DialogueDisplay;
        }
        
        private List<DialogueDisplay> _dialogueDisplays = new();
        
        public void Initialize(IEnumerable<PlayerVisuals> targets)
        {
            // Clear
            foreach (var dialogueDisplay in _dialogueDisplays) LeanPool.Despawn(dialogueDisplay);
            _dialogueDisplays.Clear();
            
            // Populate
            foreach (var player in targets.Where(x => !x.IsHuman))
            {
                var orientation = player.ReactionController.DialogueAnchor.GetOrientation();
                var displayPrefab = _DisplayPrefabs.FirstOrDefault(x => x.Orientation == orientation).DialogueDisplay;
                var dialogueDisplay = LeanPool.Spawn(displayPrefab, transform);
                dialogueDisplay.Initialize(player.ReactionController.DialogueAnchor);
                _dialogueDisplays.Add(dialogueDisplay);
            }
        }
    }
}