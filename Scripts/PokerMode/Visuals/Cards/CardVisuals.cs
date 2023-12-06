using System;
using System.Collections.Generic;
using DG.Tweening;
using EPOOutline;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;

namespace PokerMode
{
    public class CardVisuals : MonoBehaviour
    {
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        
        [SerializeField] private MeshRenderer _Renderer;
        [SerializeField] private Outlinable _Outline;
        [FormerlySerializedAs("_PlayerColor")] [SerializeField, ColorUsage(true, true)] private Color _HumanColor;
        [FormerlySerializedAs("_EnemyColor")] [SerializeField, ColorUsage(true, true)] private Color _AIColor;
        [SerializeField, ColorUsage(true, true)] private Color _NeutralColor;
        
        public (int rank, Suit suit) State { get; private set; }
        public bool Animating { get; private set; }
        public bool AffectedByCheat { get; set; }

        private enum OutlineState { None, Neutral, Human, AI }
        private List<Material> _materials = new();
        
        public void Initialize(int rank, Suit suit, bool affectedByCheat = false)
        {
            (int rank, Suit suit) state = (rank, suit);
            State = state;
            
            // Get front-side mat
            if (_Renderer == null) return;
            _Renderer.GetMaterials(_materials);
            if (_materials.Count < 2) return;
            
            // Set texture on mat
            var tex = CardTextureLookup.CardTextures[state];
            _materials[1].mainTexture = tex;
            //_materials[1].SetTexture(EmissionMap, tex); // <- Unused currently due to card-mat being unlit

            AffectedByCheat = affectedByCheat;
        }
        
        public IEnumerator<float> DeckToHand_Anim(DeckVisuals deck, HandVisuals hand, int resultHandIndex, int resultHandSize, bool humanHand)
        {
            Animating = true;
            
            // Deck control to hand control
            deck.RemoveCard(this);
            UpdateOutline(humanHand ? OutlineState.Human : OutlineState.AI);
            
            // Calculate target placement in hand
            var (targetPos, targetRot) = hand.CardPlacement(resultHandIndex, resultHandSize);
            
            // Calculate intermediate "deck exit" state
            const float DECK_EXIT_DIST = .15f;
            var deckUp = deck.transform.up;
            var towardPlayer = Vector3.ProjectOnPlane(hand.transform.position - deck.transform.position, deckUp).normalized;
            var deckExitPos = towardPlayer * DECK_EXIT_DIST + transform.position; // Start closer on the table to the player
            var deckExitRot = Quaternion.LookRotation(towardPlayer, -deck.transform.up); // Start with card-forward toward the player
            transform.position = deckExitPos;
            transform.rotation = deckExitRot;
            
            // Calculate intermediate "hand enter" state
            const float HAND_ENTER_DIST = .25f;
            var targetForward = targetRot * Vector3.forward;
            var handEnterPos = targetPos + targetForward * HAND_ENTER_DIST;
            var toHandEnter = handEnterPos - deckExitPos;
            
            // Animate
            const float DURATION = .4f;
            if (humanHand)
            {
                transform.DOMove(hand.transform.position, DURATION).SetEase(Ease.OutQuad) // Horizontal move ONLY
                    .OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                DOTween.Sequence()
                    .Join(transform.DOBlendableMoveBy(Vector3.ProjectOnPlane(toHandEnter, targetForward), DURATION * .6f).SetEase(Ease.OutQuad)) // Horizontal move component (faster)
                    .Join(transform.DOBlendableMoveBy(Vector3.Project(toHandEnter, targetForward), DURATION * .75f)) // Vertical move component
                    .Join(transform.DORotateQuaternion(targetRot, DURATION * .6f).SetEase(Ease.OutQuad))
                    .Append(transform.DOMove(targetPos, DURATION * .25f));
            }
            
            Service.AudioController.Play("DrawCard", deckExitPos);
            yield return Timing.WaitForSeconds(DURATION);
            yield return Timing.WaitForOneFrame;
            hand.AddCard(resultHandIndex, this);

            Animating = false;
        }
        
        public IEnumerator<float> HandToDeck_Anim(HandVisuals hand, DeckVisuals deck, float delay = -1f)
        {
            Animating = true;
            
            if (delay > 0) yield return Timing.WaitForSeconds(delay);
            
            gameObject.SetActive(true);
            
            // Hand control to deck control
            hand.RemoveCard(this);
            deck.AddCard(this);
            
            UpdateOutline(OutlineState.Neutral);
            
            // Calculate target placement in deck
            var (targetPos, targetRot) = deck.CardPlacement(0);
            
            // Calculate actual target "deck enter" state
            const float DECK_ENTER_DIST = .15f;
            var deckUp = deck.transform.up;
            var towardPlayer = Vector3.ProjectOnPlane(hand.transform.position - deck.transform.position, deckUp).normalized;
            var deckEnterPos = towardPlayer * DECK_ENTER_DIST + targetPos; // Closer on the table to the player
            var deckEnterRot = Quaternion.LookRotation(towardPlayer, -deck.transform.up); // Card-forward toward the player
            var toDeckEnter = deckEnterPos - transform.position;
            
            // Animate
            const float DURATION = .3f;
            DOTween.Sequence()
                .Join(transform.DOBlendableMoveBy(Vector3.ProjectOnPlane(toDeckEnter, deckUp), DURATION).SetEase(Ease.InQuad)) // Horizontal move component
                .Join(transform.DOBlendableMoveBy(Vector3.Project(toDeckEnter, deckUp), DURATION * .8f)) // Vertical move component (faster)
                .Join(transform.DORotateQuaternion(deckEnterRot, DURATION * .8f))
                .AppendCallback(() =>
                {
                    transform.position = targetPos;
                    transform.rotation = targetRot;
                });
            
            Service.AudioController.Play("DrawCard", transform.position);
            yield return Timing.WaitForSeconds(DURATION);

            Animating = false;
        }

        public IEnumerator<float> DeckToRiver_Anim(DeckVisuals deck, RiverVisuals river, int riverIndex)
        {
            Animating = true;
            
            deck.RemoveCard(this);
            river.AddCard(riverIndex, this);
            
            // Calculate target placement in river
            var (targetPos, targetRot) = river.CardPlacement(riverIndex);
            
            // Calculate intermediate "deck exit" state
            const float DECK_EXIT_DIST = .08f;
            var deckUp = deck.transform.up;
            var towardPos = Vector3.ProjectOnPlane(targetPos - deck.transform.position, deckUp).normalized;
            var deckExitPos = towardPos * DECK_EXIT_DIST + transform.position; // Start closer on the table to the player
            var deckExitRot = Quaternion.LookRotation(targetRot * Vector3.back, -deck.transform.up); // Start with card face down aligned with river
            transform.position = deckExitPos;
            transform.rotation = deckExitRot;
            
            // Calculate intermediate "river enter" state
            const float RIVER_ENTER_DIST = .2f;
            var riverEnterPos = targetPos + deckUp * RIVER_ENTER_DIST;
            var toRiverEnter = riverEnterPos - deckExitPos;
            
            // Animate
            const float DURATION = .3f;
            DOTween.Sequence()
                .Join(transform.DOBlendableMoveBy(Vector3.ProjectOnPlane(toRiverEnter, deckUp), DURATION * .45f).SetEase(Ease.OutQuad)) // Horizontal move component (faster)
                .Join(transform.DOBlendableMoveBy(Vector3.Project(toRiverEnter, deckUp), DURATION * .75f).SetEase(Ease.OutQuad)) // Vertical move component
                .Join(transform.DOBlendableLocalRotateBy(new Vector3(180, 0, 0), DURATION * .75f))
                .Append(transform.DOMove(targetPos, DURATION * .25f).SetEase(Ease.InQuad));
            
            Service.AudioController.Play("DrawCard", deckExitPos);
            yield return Timing.WaitForSeconds(DURATION);

            Animating = false;
        }
        
        public IEnumerator<float> RiverToDeck_Anim(RiverVisuals river, DeckVisuals deck)
        {
            Animating = true;
            
            river.RemoveCard(this);
            deck.AddCard(this);
            
            // Calculate target placement in deck
            var (targetPos, targetRot) = deck.CardPlacement(0);
            
            // Calculate intermediate "deck enter" state
            const float DECK_ENTER_DIST = .08f;
            var deckUp = deck.transform.up;
            var towardPos = Vector3.ProjectOnPlane(transform.position - deck.transform.position, deckUp).normalized;
            var deckEnterPos = towardPos * DECK_ENTER_DIST + targetPos; // End closer on the table to the player
            var deckExitRot = Quaternion.LookRotation(targetRot * Vector3.back, -deck.transform.up); // End with card flipped
            
            // Calculate intermediate "river exit" state
            const float RIVER_EXIT_DIST = .2f;
            var riverExitPos = transform.position + deckUp * RIVER_EXIT_DIST;
            var toDeckEnterPos = deckEnterPos - riverExitPos;
            
            // Animate
            const float DURATION = .3f;
            DOTween.Sequence()
                .Join(transform.DOMove(riverExitPos, DURATION * .25f).SetEase(Ease.OutQuad))
                .Join(transform.DOBlendableLocalRotateBy(new Vector3(-180, 0, 0), DURATION * .7f))
                .Insert(DURATION * .25f, transform.DOBlendableMoveBy(Vector3.Project(toDeckEnterPos, deckUp), DURATION * .6f).SetEase(Ease.InQuad)) // Vertical move component
                .Insert(DURATION * .65f, transform.DOBlendableMoveBy(Vector3.ProjectOnPlane(toDeckEnterPos, deckUp), DURATION * .35f).SetEase(Ease.InQuad)) // Horizontal move component (faster)
                .AppendCallback(() =>
                {
                    transform.position = targetPos;
                    transform.rotation = targetRot;
                });
            
            Service.AudioController.Play("ReturnToDeck", riverExitPos);
            yield return Timing.WaitForSeconds(DURATION);

            Animating = false;
        }
        
        private void UpdateOutline(OutlineState value)
        {
            // TEMP disabling outlines
            return;
            
            _Outline.enabled = value != OutlineState.None;
            _Outline.OutlineParameters.Color = value switch
            {
                OutlineState.Human => _HumanColor,
                OutlineState.AI => _AIColor,
                _ => _NeutralColor
            };
        }
    }
}