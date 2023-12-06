using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace PokerMode
{
    public class BidDisplay : MonoBehaviour
    {
        [SerializeField] private PhysicalLever _Lever;
        [FormerlySerializedAs("_CreditDisplay")] [SerializeField] private CreditVisuals _CreditVisuals;
        
        private int _bid;
        
        void Awake()
        {
            _Lever.OnValueChange += UpdateBid;
        }
        
        private void UpdateBid(float value)
        {
            const int LOWEST_BID = 5;
            const int HIGHEST_BID = 200;
            
            _bid = Mathf.RoundToInt(Mathf.Lerp(LOWEST_BID, HIGHEST_BID, value));
            _CreditVisuals.Show(_bid);
        }
    }
}