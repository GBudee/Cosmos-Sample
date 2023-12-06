using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using MEC;
using NavigationMode;
using Sentry;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace UI.Menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private LoadingScreen prefab_LoadingScreen;
        [SerializeField] private Image _ForegroundScrim;
        [SerializeField] private CanvasGroup _CanvasGroup;
        [Header("Child Menus")]
        [SerializeField] private SettingsMenu _SettingsMenu;
        [SerializeField] private ConfirmationMenu _ConfirmationMenu;
        [SerializeField] private GameCreditsMenu _CreditsMenu;
        [Header("Buttons")]
        [SerializeField] private HudButton _Continue;
        [SerializeField] private HudButton _NewGame;
        [SerializeField] private HudButton _Settings;
        [SerializeField] private HudButton _Credits;
        [SerializeField] private HudButton _Quit;
        
        private System.Action _onEscape;
        private Tween _fader;
        
        private void Awake()
        {
            // Init game settings
            Settings.Apply(Settings.Current);
            if (Steamworks.SteamClient.IsValid) Steamworks.SteamFriends.OnGameOverlayActivated += OnSteamOverlay;
            _fader = _CanvasGroup.DOFade(1f, .5f).From(0f)
                .SetAutoKill(false).Pause();
            _fader.Complete();
        }
        
        private void OnDestroy()
        {
            if (Steamworks.SteamClient.IsValid) Steamworks.SteamFriends.OnGameOverlayActivated -= OnSteamOverlay;
        }
        
        private void OnSteamOverlay(bool value)
        {
            Time.timeScale = value ? 0f : 1f;
        }
        
        private void Start()
        {
            Show();
            _ForegroundScrim.color = Color.black;
            Service.AudioController.SetRadioSpatialization(false);
            Timing.RunCoroutine(ForegroundFade());
            IEnumerator<float> ForegroundFade()
            {
                yield return Timing.WaitForOneFrame; // Allow the game to fully load before starting the fade
                yield return Timing.WaitForOneFrame;
                _ForegroundScrim.DOFade(0f, 1f);
            }
        }
        
        void Update()
        {
            // Escape to navigate submenus
            if (Input.GetKeyDown(KeyCode.Escape)) _onEscape?.Invoke();
        }
        
        private void Show()
        {
            bool saveExists = Saving.Exists();
            bool demoSaveExists = Saving.DemoSaveExists();
            
            // Set up inputs
            _Continue.interactable = saveExists || demoSaveExists;
            _Continue.SetListener(() =>
            {
                if (saveExists) LoadGame(Saving.GetSceneName());
                else _ConfirmationMenu.Show(label: "This save is associated with a pre-release game version, and is no longer compatible.");
            });
            if (saveExists)
            {
                _NewGame.SetListener(() => _ConfirmationMenu.Show(label: "Are you sure? This will delete your existing save."
                    , confirm: () =>
                    {
                        Saving.Delete();
                        LoadGame("BuzzGazz");
                    }));
            }
            else
            {
                _NewGame.SetListener(() => LoadGame("BuzzGazz"));
            }
            _Settings.SetListener(() =>
            {
                _SettingsMenu.Show(onExit: Show);
                _onEscape = () =>
                {
                    _SettingsMenu.Hide();
                    Show();
                };
            });
            _Credits.SetListener(() =>
            {
                _fader.PlayBackwards();
                _CreditsMenu.Show(onHide: () => _fader.PlayForward());
            });
            _Quit.SetListener(Application.Quit);
            
            void LoadGame(string scene)
            {
                GameController.LoadingFromMenu = true;
                Instantiate(prefab_LoadingScreen).LoadScene(scene);
            }
            
            // Escape not used when no submenus active
            _onEscape = null;
        }
    }
}
