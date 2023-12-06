using DG.Tweening;
using UnityEngine;

namespace PokerMode
{
    public class DealerButton : MonoBehaviour
    {
        public void GoToPlayer(PlayerVisuals target)
        {
            transform.DOMove(target.DealerButtonAnchor.position, .5f);
        }
    }
}