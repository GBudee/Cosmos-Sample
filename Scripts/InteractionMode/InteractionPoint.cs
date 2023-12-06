using System;
using System.Collections.Generic;
using Cinemachine;
using MEC;
using PokerMode;
using PokerMode.Dialogue;
using UI.Interaction;
using UnityEngine;

namespace InteractionMode
{
    public class InteractionPoint : MonoBehaviour
    {
        [SerializeField] private InteractionDescription _Description;
        [Header("Optional")]
        [SerializeField] private CinemachineVirtualCamera _VirtualCamera;

        public DialogueAnchor DialogueAnchor => _Description.DialogueAnchor;
        public bool HasCustomCamera => _VirtualCamera != null;
        
        void OnEnable()
        {
            if (_VirtualCamera != null)
                _VirtualCamera.enabled = true;
        }
        
        void OnDisable()
        {
            _Description.DialogueAnchor?.HideDialogue();
            Timing.KillCoroutines(gameObject);
            if (_VirtualCamera != null)
                _VirtualCamera.enabled = false;
        }
        
#if UNITY_EDITOR
        void OnValidate()
        {
            if (_VirtualCamera != null)
                _VirtualCamera.enabled = enabled;
        }
#endif
        
        public void Initialize(CosmoController cosmoController) => _Description.Initialize(cosmoController);
        
        public void EnterInteractionMode(CosmoController cosmoController, InteractionHud hud)
        {
            Timing.RunCoroutine(_Description.Implementation(cosmoController, hud), gameObject);
        }
    }
}