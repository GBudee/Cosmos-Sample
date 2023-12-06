using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Pool;
using PokerMode;
using UnityEngine;

namespace UI
{
    public class PlayerDisplayMgr : MonoBehaviour
    {
        [SerializeField] private PlayerDisplay _HumanDisplay;
        [SerializeField] private PlayerDisplay prefab_PlayerDisplay;
        
        private List<PlayerDisplay> _AIDisplays = new();
        
        public void Initialize(IEnumerable<PlayerVisuals> targets)
        {
            // Clear
            foreach (var aiDisplay in _AIDisplays) LeanPool.Despawn(aiDisplay);
            _AIDisplays.Clear();
            
            // Populate
            var aiIndex = 0;
            int aiCount = targets.Count() - 1;
            foreach (var player in targets)
            {
                if (player.IsHuman) _HumanDisplay.Initialize(player);
                else
                {
                    var playerDisplay = LeanPool.Spawn(prefab_PlayerDisplay, transform);
                    playerDisplay.Initialize(player, aiIndex);
                    (playerDisplay.transform as RectTransform).anchoredPosition = new Vector2((aiIndex - (aiCount / 2f) + .5f) * MathG.Remap(3, 6, 360f, 300f, aiCount), 0);
                    playerDisplay.transform.localScale = Vector3.one * MathG.Remap(3, 6, 1f, .8f, aiCount);
                    _AIDisplays.Add(playerDisplay);
                    aiIndex++;
                }
            }
        }
    }
}