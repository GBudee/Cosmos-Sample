using System.Collections.Generic;
using System.Linq;
using NavigationMode;
using PokerMode;
using UI.Interaction;
using UnityEngine;
using static GameController;

namespace InteractionMode
{
    [ExecuteBefore(typeof(GameController)), ExecuteBefore(typeof(InteractionHud))]
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private CosmoController _CosmoController;
        [SerializeField] private NavigationController _NavigationController;
        [SerializeField] private List<InteractionPoint> _InteractionPoints;
        
        public NavigationController NavigationController => _NavigationController;
        public InteractionPoint ActivePoint { get; private set; }
        public System.Func<InteractionPoint, InteractionHud> OnActivePointChanged;
        
        void Awake()
        {
            // Find active table in scene
            ActivePoint = _InteractionPoints.FirstOrDefault(x => x.enabled);
        }

        public void Initialize()
        {
            foreach (var point in _InteractionPoints)
            {
                point.Initialize(_CosmoController);
            }
            
            if (ActivePoint != null)
            {
                if (Service.GameController.CurrentMode is GameMode.Interaction) JoinInteraction(ActivePoint);
                else
                {
                    ActivePoint.enabled = false;
                    ActivePoint = null;
                }
            }
        }
        
        public void JoinInteraction(InteractionPoint point)
        {
            point.enabled = true;
            ActivePoint = point;
            var hud = OnActivePointChanged?.Invoke(point);
            Debug.Assert(hud != null, "Interaction Hud not registering with Interaction Controller");
            point.EnterInteractionMode(_CosmoController, hud);
        }
        
        public void LeaveInteraction()
        {
            ActivePoint.enabled = false;
            ActivePoint = null;
            OnActivePointChanged?.Invoke(null);
        }
    }
}