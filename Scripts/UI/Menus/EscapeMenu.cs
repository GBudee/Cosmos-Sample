using System;
using NavigationMode;
using UI.Cheats;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

namespace UI.Menus
{
    [ExecuteBefore(typeof(NavigationController))]
    public class EscapeMenu : MonoBehaviour
    {
        [SerializeField] private LoadingScreen prefab_LoadingScreen;
        [SerializeField] private NavigationController _NavigationController;
        [SerializeField] private GameObject _Container;
        [SerializeField] private CheatDeckMenu _CheatDeckMenu;
        [Header("Child Menus")]
        [SerializeField] private SettingsMenu _SettingsMenu;
        [Header("Buttons")]
        [SerializeField] private HudButton _Resume;
        [SerializeField] private HudButton _Settings;
        [SerializeField] private HudButton _Exit;
        
        public static bool Paused => _pausedFromMenu || _pausedFromOverlay;
        private static void UpdateTimeScale() => Time.timeScale = Paused ? 0f : 1f;
        private static bool _pausedFromMenu;
        private static bool _pausedFromOverlay;
        
        private System.Action OnEscape;
        
        private void Awake()
        {
            OnEscape = Show;
            if (Steamworks.SteamClient.IsValid) Steamworks.SteamFriends.OnGameOverlayActivated += OnSteamOverlay;
        }
        
        private void OnDestroy()
        {
            if (Steamworks.SteamClient.IsValid) Steamworks.SteamFriends.OnGameOverlayActivated -= OnSteamOverlay;
        }
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) OnEscape?.Invoke();
            if (Paused || Service.GameController.CurrentMode is GameMode.Cinematic or GameMode.Poker || _CheatDeckMenu.DisableHotkey) return;
            if (!_NavigationController.ShowCheatDeckMenu && Input.GetKeyDown(KeyCode.Tab)) _NavigationController.ShowObjectiveMenu = !_NavigationController.ShowObjectiveMenu;
            if (!_NavigationController.ShowObjectiveMenu && Input.GetKeyDown(KeyCode.R)) _NavigationController.ShowCheatDeckMenu = !_NavigationController.ShowCheatDeckMenu;
        }
        
        public void LoadScene(string sceneName)
        {
            _pausedFromMenu = true;
            OnEscape = null;
            Instantiate(prefab_LoadingScreen).LoadScene(sceneName);
            
            SceneManager.sceneUnloaded += SceneUnloaded;
            void SceneUnloaded(Scene scene)
            {
                _pausedFromMenu = false;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }
        
        private void OnSteamOverlay(bool value)
        {
            _pausedFromOverlay = value;
            UpdateTimeScale();
        }
        
        private void Show()
        {
            // Set up inputs
            _Resume.SetListener(() => Hide());
            _Settings.SetListener(() =>
            {
                Hide(toGameplay: false);
                _SettingsMenu.Show(onExit: Show);
                OnEscape = () =>
                {
                    _SettingsMenu.Hide();
                    Show();
                };
            });
            _Exit.SetListener(() =>
            {
                if (_NavigationController.enabled) Service.GameController.SaveGame();
                LoadScene("MainMenu");
            });
            OnEscape = () => Hide();
            
            // Disable navigation focus
            _pausedFromMenu = true;
            UpdateTimeScale();
            
            // Show menu visuals
            _Container.gameObject.SetActive(true);
        }
        
        private void Hide(bool toGameplay = true)
        {
            // Set up inputs
            OnEscape = toGameplay ? Show : null;
            
            // Enable navigation focus
            if (toGameplay)
            {
                _pausedFromMenu = false;
                UpdateTimeScale();
            }
            
            // Hide menu visuals
            _Container.gameObject.SetActive(false);
        }
    }
}
