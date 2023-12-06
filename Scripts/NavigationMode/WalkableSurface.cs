using UnityEngine;

namespace NavigationMode
{
    public class WalkableSurface : MonoBehaviour
    {
        [SerializeField] private string _FootstepSound = "Footstep";
        [SerializeField] private int _SoundCount = 3;
        
        public string FootstepSound => _FootstepSound;
        public int SoundCount => _SoundCount;
    }
}