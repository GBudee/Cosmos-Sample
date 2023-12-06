using System;
using System.IO;
using Cinematics;
using InteractionMode;
using Managers;
using NavigationMode;
using PokerMode;
using UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameController : MonoBehaviour
{
    public static bool LoadingFromMenu = false;

    [SerializeField] private CinematicController _CinematicController;
    [SerializeField] private PokerController _PokerController;
    [SerializeField] private NavigationController _NavigationController;
    [SerializeField] private InteractionController _InteractionController;
    [SerializeField] private SimpleTutorial _SimpleTutorial;
    [SerializeField] private bool _DEBUG_ForceNewSave;
    [SerializeField] private bool _DEBUG_CustomStartingCash;
    [SerializeField] private bool _DEBUG_StackDeck;
    
    public bool DEBUG_CustomStartingCash => _DEBUG_CustomStartingCash;
    public bool DEBUG_StackDeck => _DEBUG_StackDeck;
    
    public enum GameMode { Navigation, Cinematic, Poker, Interaction }
    public GameMode CurrentMode => _currentMode;
    private GameMode _currentMode;
    
    void Awake()
    {
        Settings.Apply(Settings.Current); // Init settings
#if UNITY_EDITOR
        if (_DEBUG_ForceNewSave && Saving.Exists()) Saving.Delete();
#endif
        bool loadedSave = Saving.Load(_PokerController.Load, _NavigationController.Load, _SimpleTutorial.Load);
        
        if (_CinematicController != null && (_CinematicController.enabled || LoadingFromMenu && !loadedSave)) _currentMode = GameMode.Cinematic;
        else if (_NavigationController.enabled || LoadingFromMenu && loadedSave) _currentMode = GameMode.Navigation;
        else if (_PokerController.ActiveTable != null) _currentMode = GameMode.Poker;
        else if (_InteractionController.ActivePoint != null) _currentMode = GameMode.Interaction;
        
        if (_CinematicController != null) _CinematicController.enabled = _currentMode == GameMode.Cinematic;
        _NavigationController.enabled = _currentMode == GameMode.Navigation;
        if (LoadingFromMenu) _NavigationController.ShowObjectiveMenu = true;
        
        _PokerController.Initialize(loadedSave);
        _InteractionController.Initialize();
        if (_CinematicController != null) _CinematicController.Initialize();
        UpdateShadowProximity();
    }
    
    private void OnDisable()
    {
        UniversalRenderPipelineAsset urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        urp.shadowDistance = 180;
    }
    
    public void SaveGame(bool changingScenes = false)
    {
        Saving.Save(changingScenes, _PokerController.Save, _NavigationController.Save, _SimpleTutorial.Save);
    }
    
    public void SetMode(GameMode newMode, TableController table = null, InteractionPoint interaction = null)
    {
        if (_currentMode == newMode) return;
        _currentMode = newMode;
        
        if (_CinematicController != null) _CinematicController.enabled = newMode == GameMode.Cinematic;
        _NavigationController.enabled = newMode == GameMode.Navigation;
        if (newMode == GameMode.Poker) _PokerController.JoinTable(table);
        else if (_PokerController.ActiveTable != null) _PokerController.LeaveTable();
        if (newMode == GameMode.Interaction) _InteractionController.JoinInteraction(interaction);
        else if (_InteractionController.ActivePoint != null) _InteractionController.LeaveInteraction();
        
        UpdateShadowProximity();
    }
    
    private void UpdateShadowProximity()
    {
        UniversalRenderPipelineAsset urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        urp.shadowDistance = (_currentMode is GameMode.Navigation or GameMode.Interaction || SceneManager.GetActiveScene().name != "BuzzGazz") ? 180 : 30;
    }
}
