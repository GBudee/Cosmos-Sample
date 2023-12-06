using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lean.Pool;
using Managers;
using PokerMode;
using UnityEngine;
using Utilities;

namespace UI
{
    [ExecuteAfter(typeof(GameController))]
    public class SimpleTutorial : MonoBehaviour, ISaveable
    {
        [SerializeField] private Tooltip prefab_Tooltip;
        [SerializeField] private TooltipManager _TooltipManager;
        [SerializeField] private Canvas _ParentCanvas;
        
        private List<(string key, TutorialTarget target)> _registeredBeats = new();
        private List<Tooltip> _activeBeats = new();
        private HashSet<string> _spentBeats = new();
        
        public void Save(int version, BinaryWriter writer, bool changingScenes)
        {
            writer.WriteSet(_spentBeats);
        }
        
        public void Load(int version, BinaryReader reader)
        {
            reader.ReadSet(_spentBeats);
        }

        private void OnEnable()
        {
            foreach (var element in _activeBeats)
            {
                if (element != null) LeanPool.Despawn(element);
            }
            _activeBeats.Clear();
        }
        
        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.B)) TriggerBeat("INTRO_EXP");
#endif
        }
        
        public void RegisterTarget(string label, TutorialTarget target) => _registeredBeats.Add((label, target));
        public void SuppressBeat(string label) => _spentBeats.Add(label);
        public void TriggerBeat(string label)
        {
            if (_spentBeats.Contains(label)) return;
            if (!Settings.Current.ShowTooltips) return;
            
            var target = _registeredBeats.FirstOrDefault(x => x.key == label).target;
            if (target != null)
            {
                // Spawn tutorial tooltip
                var tooltip = LeanPool.Spawn(prefab_Tooltip, transform);
                tooltip.Show(_ParentCanvas, target.Anchor, target.Header, target.Contents, onClick: () =>
                {
                    _activeBeats.Remove(tooltip);
                    LeanPool.Despawn(tooltip);
                });
                
                // Enter active beat state
                _activeBeats.Add(tooltip);
                _spentBeats.Add(label);
            }
        }
        
        public void UnregisterTarget(TutorialTarget target)
        {
            for (int i = 0; i < _registeredBeats.Count; i++)
                if (_registeredBeats[i].target == target)
                {
                    _registeredBeats.RemoveAt(i);
                    i--;
                }
        }
    }
}