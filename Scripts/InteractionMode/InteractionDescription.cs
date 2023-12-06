using System.Collections.Generic;
using PokerMode;
using PokerMode.Dialogue;
using UI.Interaction;
using UnityEngine;

namespace InteractionMode
{
    [RequireComponent(typeof(InteractionPoint))]
    public abstract class InteractionDescription : MonoBehaviour
    {
        public virtual DialogueAnchor DialogueAnchor => null;
        public virtual void Initialize(CosmoController cosmoController) { }
        public abstract IEnumerator<float> Implementation(CosmoController cosmoController, InteractionHud hud);
    }
}