using System.Collections.Generic;
using System.Linq;
using Managers;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Menus
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private HudHoverableMgr _HudHoverableMgr;
        [Header("Child Menus")]
        [SerializeField] private ConfirmationMenu _ConfirmationMenu;
        [Header("Labels")]
        [SerializeField] private TMP_Text _DisplayModeLabel;
        [SerializeField] private TMP_Text _ResolutionLabel;
        [SerializeField] private TMP_Text _VSyncLabel;
        [SerializeField] private TMP_Text _FOVLabel;
        [SerializeField] private TMP_Text _MouseSensitivityLabel;
        [SerializeField] private TMP_Text _ShowTooltipsLabel;
        [SerializeField] private TMP_Text _MasterVolumeLabel;
        [SerializeField] private TMP_Text _SFXVolumeLabel;
        [SerializeField] private TMP_Text _MusicVolumeLabel;
        [Header("Buttons")] 
        [SerializeField] private HudDropdown _DisplayMode;
        [SerializeField] private HudDropdown _Resolution;
        [SerializeField] private HudToggle _VSync;
        [SerializeField] private HudSlider _FOV;
        [SerializeField] private HudSlider _MouseSensitivity;
        [SerializeField] private HudToggle _ShowTooltips;
        [SerializeField] private HudSlider _MasterVolume;
        [SerializeField] private HudSlider _SFXVolume;
        [SerializeField] private HudSlider _MusicVolume;
        [SerializeField] private HudButton _Confirm;
        [SerializeField] private HudButton _Cancel;
        
        public void Show(System.Action onExit)
        {
            Settings currentSettings = Settings.Current;
            Settings intendedSettings = new Settings(currentSettings);
            
            // Reset labels
            ResetLabels(_DisplayModeLabel, _ResolutionLabel, _VSyncLabel, _FOVLabel, _MouseSensitivityLabel, _MasterVolumeLabel, _SFXVolumeLabel, _MusicVolumeLabel);
            
            // Settings buttons
            _DisplayMode.SetEntries
            (
                entries: Settings.ValidDisplayModes.Select(value => (value, (System.Action)(() =>
                {
                    intendedSettings.CurrentDisplayMode = value;
                    UpdateLabel(_DisplayModeLabel, value != currentSettings.CurrentDisplayMode);
                }))),
                current: currentSettings.CurrentDisplayMode
            );
            
            _Resolution.SetEntries
            (
                entries: Settings.ValidResolutions.Select(value => (value, (System.Action)(() =>
                {
                    intendedSettings.Resolution = value;
                    UpdateLabel(_ResolutionLabel, value != currentSettings.Resolution);
                }))), 
                current: currentSettings.Resolution
            );
            
            _VSync.SetListener(value =>
            {
                intendedSettings.VSync = value;
                UpdateLabel(_VSyncLabel, value != currentSettings.VSync);
            }, initialValue: currentSettings.VSync);
            
            _FOV.SetListener(value =>
            {
                intendedSettings.FOV = value;
                UpdateLabel(_FOVLabel, value != currentSettings.FOV);
            }, initialValue: currentSettings.FOV);
            
            _MouseSensitivity.SetListener(value =>
            {
                intendedSettings.MouseSensitivity = value;
                UpdateLabel(_MouseSensitivityLabel, value != currentSettings.MouseSensitivity);
            }, initialValue: currentSettings.MouseSensitivity);
            
            _ShowTooltips.SetListener(value =>
            {
                intendedSettings.ShowTooltips = value;
                UpdateLabel(_ShowTooltipsLabel, value != currentSettings.ShowTooltips);
            }, initialValue: currentSettings.ShowTooltips);
            
            _MasterVolume.SetListener(value =>
            {
                intendedSettings.MasterVolume = value;
                Service.AudioController.SetVolume("MasterVolume", value);
                UpdateLabel(_MasterVolumeLabel, value != currentSettings.MasterVolume);
            }, initialValue: currentSettings.MasterVolume);
            
            _SFXVolume.SetListener(value =>
            {
                intendedSettings.SFXVolume = value;
                Service.AudioController.SetVolume("SFXVolume", value);
                UpdateLabel(_SFXVolumeLabel, value != currentSettings.SFXVolume);
            }, initialValue: currentSettings.SFXVolume);
            
            _MusicVolume.SetListener(value =>
            {
                intendedSettings.MusicVolume = value;
                Service.AudioController.SetVolume("MusicVolume", value);
                UpdateLabel(_MusicVolumeLabel, value != currentSettings.MusicVolume);
            }, initialValue: currentSettings.MusicVolume);
            
            // Confirm/Cancel
            _Cancel.SetListener(Exit);
            _Confirm.SetListener(() =>
            {
                if (!intendedSettings.Equals(currentSettings))
                {
                    // Apply changed settings
                    var priorSettings = currentSettings;
                    Settings.Apply(intendedSettings);
                    
                    // Show confirmation/reversion dialogue for display changes
                    if (priorSettings.CurrentDisplayMode != intendedSettings.CurrentDisplayMode 
                        || priorSettings.Resolution != intendedSettings.Resolution)
                    {
                        _HudHoverableMgr.AllowHighlight(false);
                        _ConfirmationMenu.Show(label: "Keep Display Changes?", confirm: Exit, cancel: () =>
                        {
                            Settings.Apply(priorSettings);
                            Show(onExit);
                        }, autoCancelTimer: 10f);
                    }
                    else Exit();
                }
                else Exit();
            });
            
            void Exit()
            {
                // Restore volume if appropriate
                currentSettings = Settings.Current;
                if (currentSettings.MasterVolume != intendedSettings.MasterVolume) Service.AudioController.SetVolume("MasterVolume", currentSettings.MasterVolume);
                if (currentSettings.SFXVolume != intendedSettings.SFXVolume) Service.AudioController.SetVolume("SFXVolume", currentSettings.SFXVolume);
                if (currentSettings.MusicVolume != intendedSettings.MusicVolume) Service.AudioController.SetVolume("MusicVolume", currentSettings.MusicVolume);
                
                // Leave settings menu
                Hide();
                onExit?.Invoke();
            }

            _HudHoverableMgr.AllowHighlight(true);
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void ResetLabels(params TMP_Text[] labels)
        {
            foreach (var label in labels) 
                label.text = label.text.Replace("*", "");
        }
        
        private void UpdateLabel(TMP_Text label, bool isModified)
        {
            label.text = label.text.Replace("*", "") + (isModified ? "*" : "");
        }
    }
}