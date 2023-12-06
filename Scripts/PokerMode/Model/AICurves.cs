using UnityEngine;

namespace PokerMode
{
    [CreateAssetMenu(fileName = "AICurves", menuName = "ScriptableObjects/AICurves", order = 0)]
    public class AICurves : ScriptableObject
    {
        public AnimationCurve CallRate_Per_RoR;
        public AnimationCurve RaiseRate_Per_Winrate;
        public AnimationCurve LowestRaise_Per_Winrate;
        public AnimationCurve HighestRaise_Per_Winrate;
        
        private static AICurves _instance;
        public static AICurves Instance
        {
            get
            {
                _instance ??= Resources.Load<AICurves>("AICurves");
                return _instance;
            }
        }
    }
}