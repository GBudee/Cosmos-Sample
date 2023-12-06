using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public static class Saving
    {
        private const int OLDEST_VALID_VERSION = 8;
        private const int VERSION = 8;
        private static string Path => Application.persistentDataPath + "/Autosave.sav";
        private static string DemoPath => Application.persistentDataPath + "/Autosave_Demo.sav";
        private static string TempPath => Application.persistentDataPath + "/Autosave_Temp.sav";
        private static string BackupPath => Application.persistentDataPath 
                                            + $"/Autosave_Backup_{DateTime.Now:yy-MM-dd_HH-mm}.sav";
        
        public delegate void SaveAction(int version, BinaryWriter writer, bool changingScenes);
        public delegate void LoadAction(int version, BinaryReader reader);
        
        public static bool Exists() => File.Exists(Path);
        public static bool DemoSaveExists() => File.Exists(DemoPath);
        public static void Delete() => File.Delete(Path);
        
        private static Tween _backupTimer;
        
        public static void Save(bool changingScenes, params SaveAction[] saveActions)
        {
            try
            {
                // Create save
                using var stream = new FileStream(TempPath, FileMode.OpenOrCreate);
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(VERSION);
                    writer.Write(SceneManager.GetActiveScene().name);
                    foreach (var saveAction in saveActions)
                        saveAction(VERSION, writer, changingScenes);
                }
                
                // Copy save to master path, backing up existing save if appropriate
                if (_backupTimer == null && File.Exists(Path))
                {
                    var backupPath = BackupPath;
                    if (!File.Exists(backupPath))
                        File.Copy(Path, backupPath);
                    _backupTimer = DOVirtual.DelayedCall(10 * 60, () => _backupTimer = null, ignoreTimeScale: false); // 10 minutes between backups
                }
                File.Delete(Path);
                File.Copy(TempPath, Path);
                File.Delete(TempPath);
            }
            catch (Exception e)
            {
                ErrorDisplay.ShowError($"Saving failed. Progress since last autosave is not preserved.\n\nError: {e.Message + e.StackTrace}");
                throw;
            }
        }
        
        public static string GetSceneName()
        {
            if (!File.Exists(Path)) return "BuzzGazz";
            using var stream = File.Open(Path, FileMode.Open);
            using var reader = new BinaryReader(stream);
            
            var version = reader.ReadInt32();
            var scene = reader.ReadString();
            return scene;
        }
        
        public static bool Load(params LoadAction[] loadActions)
        {
            if (!File.Exists(Path)) return false;
            
            try
            {
                using var stream = File.Open(Path, FileMode.Open);
                using var reader = new BinaryReader(stream);
                
                var version = reader.ReadInt32();
                if (version < OLDEST_VALID_VERSION) return false;
                var scene = reader.ReadString();
                foreach (var loadAction in loadActions)
                    loadAction(version, reader);
                return true;
            }
            catch (Exception e)
            {
                ErrorDisplay.ShowError($"Failed to load Autosave.sav\n\nConsider using one of the backup save files in {Application.persistentDataPath}\n\nError: {e.Message + e.StackTrace}");
                throw;
            }
        }
    }
}