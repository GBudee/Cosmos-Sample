using UnityEngine;
using Utilities;

namespace NavigationMode
{
    public class TruckController : MonoBehaviour
    {
        [SerializeField] private TriggerHelper _VisibleZone;

        public TriggerHelper VisibleZone => _VisibleZone;
    }
}