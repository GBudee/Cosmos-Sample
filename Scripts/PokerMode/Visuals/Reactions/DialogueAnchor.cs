using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace PokerMode.Dialogue
{
    public class DialogueAnchor : MonoBehaviour
    {
        [SerializeField] private Orientation _MyOrientation;
        [SerializeField] private List<Anchor> _Anchors;
        
        public enum Orientation { TopLeft, TopRight, BottomLeft, BottomRight }
        [System.Serializable]
        public struct Anchor
        {
            public Transform Transform;
            public Orientation Orientation;
        }
        
        public Orientation GetOrientation() => _MyOrientation;
        public Transform GetAnchor() => _Anchors.FirstOrDefault(x => x.Orientation == _MyOrientation).Transform;
        public string Text { get; private set; }
        
        public void ShowDialogue(string s, float? duration = null, float? probability = null)
        {
            if (DialogueAlreadyActive(s)) return;
            if (probability.HasValue && Random.Range(0f, 1f) > probability.Value) return;
            
            this.DOKill();
            Text = s;
            RegisterDialogue(s);
            
            if (duration.HasValue) DOVirtual.DelayedCall(duration.Value, HideDialogue, ignoreTimeScale: false).SetTarget(this);
        }
        
        public void HideDialogue()
        {
            this.DOKill();
            Text = null;
            UnregisterDialogue();
        }
        
        // *** DIALOGUE DE-DUPLICATION ***
        private static List<(DialogueAnchor anchor, string dialogue)> AllActiveDialogue = new();
        
        private bool DialogueAlreadyActive(string s) => AllActiveDialogue.Any(x => x.dialogue == s);
        
        private void RegisterDialogue(string s)
        {
            UnregisterDialogue();
            AllActiveDialogue.Add((this, s));
        }
        
        private void UnregisterDialogue()
        {
            for (int i = 0; i < AllActiveDialogue.Count; i++)
                if (AllActiveDialogue[i].anchor == this)
                {
                    AllActiveDialogue.RemoveAt(i);
                    i--;
                }
        }
    }
}