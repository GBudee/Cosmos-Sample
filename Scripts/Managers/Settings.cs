using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Managers
{
    public class Settings
    {
        /*
         NOTE: WHEN ADDING SETTINGS, USE THE FOLLOWING CHECKLIST:
          1. Add a setting field []
          2. Add a Load to the default constructor []
          3. Add a Clone to the clone constructor []
          4. Add an equality check to Equals() []
          5. Add a Save and the actual setting functionality to Apply() []
         */
        
        // Enumerable states
        public enum DisplayMode { Windowed, BorderlessFullscreen, Fullscreen }
        public static IEnumerable<DisplayMode> ValidDisplayModes => Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();
        public static IEnumerable<(int width, int height)> ValidResolutions => Screen.resolutions.Select(x => (x.width, x.height)).Distinct();
        
        // Display settings
        public DisplayMode CurrentDisplayMode;
        public (int width, int height) Resolution;
        public bool VSync;
        
        // Gameplay settings
        public int FOV;
        public float MouseSensitivity;
        public bool ShowTooltips;
        
        // Audio settings
        public int MasterVolume;
        public int SFXVolume;
        public int MusicVolume;
        
        // Settings.Current singleton accessor
        public static Settings Current => _currentSettings ??= new Settings();
        private static Settings _currentSettings;
        
        // Default constructor loads current settings state from Unity & PlayerPrefs
        private Settings()
        {
            CurrentDisplayMode = Screen.fullScreenMode switch
            {
                FullScreenMode.Windowed => DisplayMode.Windowed,
                FullScreenMode.FullScreenWindow => DisplayMode.BorderlessFullscreen,
                FullScreenMode.ExclusiveFullScreen => DisplayMode.Fullscreen,
                _ => DisplayMode.BorderlessFullscreen
            };
            Resolution = (Screen.width, Screen.height);
            VSync = QualitySettings.vSyncCount > 0;
            FOV = PlayerPrefs.GetInt("FOV", defaultValue: 60);
            MouseSensitivity = PlayerPrefs.GetFloat("MOUSE_SENSITIVITY", 1f);
            ShowTooltips = PlayerPrefs.GetInt("SHOW_TOOLTIPS", 1) > 0;
            MasterVolume = PlayerPrefs.GetInt("MASTER_VOLUME", defaultValue: 100);
            SFXVolume = PlayerPrefs.GetInt("SFX_VOLUME", defaultValue: 100);
            MusicVolume = PlayerPrefs.GetInt("MUSIC_VOLUME", defaultValue: 100);
        }
        
        // Clone constructor allows creating temporary settings instances for menu to make non-destructive data changes
        public Settings(Settings toClone)
        {
            CurrentDisplayMode = toClone.CurrentDisplayMode;
            Resolution = toClone.Resolution;
            VSync = toClone.VSync;
            FOV = toClone.FOV;
            MouseSensitivity = toClone.MouseSensitivity;
            ShowTooltips = toClone.ShowTooltips;
            MasterVolume = toClone.MasterVolume;
            SFXVolume = toClone.SFXVolume;
            MusicVolume = toClone.MusicVolume;
        }
        
        public bool Equals(Settings other)
        {
            return CurrentDisplayMode == other.CurrentDisplayMode 
                   && Resolution.Equals(other.Resolution) 
                   && VSync == other.VSync 
                   && FOV == other.FOV
                   && MouseSensitivity == other.MouseSensitivity
                   && ShowTooltips == other.ShowTooltips
                   && MasterVolume == other.MasterVolume 
                   && SFXVolume == other.SFXVolume 
                   && MusicVolume == other.MusicVolume;
        }
        
        // Apply effectuates the intended setting state and makes it the current one
        public static void Apply(Settings toApply)
        {
            // Display settings
            FullScreenMode fullScreenMode = toApply.CurrentDisplayMode switch
            {
                DisplayMode.Windowed => FullScreenMode.Windowed,
                DisplayMode.BorderlessFullscreen => FullScreenMode.FullScreenWindow,
                DisplayMode.Fullscreen => FullScreenMode.ExclusiveFullScreen
            };
            Screen.SetResolution(toApply.Resolution.width, toApply.Resolution.height, fullScreenMode);
            QualitySettings.vSyncCount = toApply.VSync ? 1 : 0;
            
            // Gameplay settings
            PlayerPrefs.SetInt("FOV", toApply.FOV);
            PlayerPrefs.SetFloat("MOUSE_SENSITIVITY", toApply.MouseSensitivity);
            PlayerPrefs.SetInt("SHOW_TOOLTIPS", toApply.ShowTooltips ? 1 : 0);
            
            // Audio settings
            SetVolume(toApply.MasterVolume, "MasterVolume", "MASTER_VOLUME");
            SetVolume(toApply.SFXVolume, "SFXVolume", "SFX_VOLUME");
            SetVolume(toApply.MusicVolume, "MusicVolume", "MUSIC_VOLUME");
            void SetVolume(int value, string mixerLabel, string prefsLabel)
            {
                // Logarithmic volume conversion per https://johnleonardfrench.com/the-right-way-to-make-a-volume-slider-in-unity-using-logarithmic-conversion/
                Service.AudioController.SetVolume(mixerLabel, value);
                PlayerPrefs.SetInt(prefsLabel, value);
            }
            
            PlayerPrefs.Save();
            
            // Update instance data
            _currentSettings = toApply;
        }
    }
}