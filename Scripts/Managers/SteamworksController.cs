using UI.Menus;
using UnityEngine;

namespace Managers
{
    public class SteamworksController : MonoBehaviour
    {
        private const int CDL_APPID = 2479950;
        private bool _isInitialized;
        
        public void Awake()
        {
            #if UNITY_EDITOR
            return;
            #endif
            
            try
            {
                /* // Disable restart-into-steam for testing prior to steam approval
#if !UNITY_EDITOR
                bool shouldRestart = Steamworks.SteamClient.RestartAppIfNecessary(CDL_APPID);
                if (shouldRestart)
                {
                    Application.Quit();
                    return;
                }
#endif
                */
            }
            catch
            {
                Debug.LogError("Could not load steam_api.dll. Steamworks functionality disabled.");
            }
            
            try
            {
                Steamworks.SteamClient.Init(CDL_APPID, false);
                Steamworks.Dispatch.OnException = Debug.LogException; // This ensures that any callbacks which fail during the steamworks thread don't do so silently
                _isInitialized = true;
                DontDestroyOnLoad(gameObject);
                
                Debug.Log("<color=lightblue>Steamworks connected.</color> Username: " + Steamworks.SteamClient.Name);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        
        private void Update()
        {
            if (_isInitialized && Steamworks.SteamClient.IsValid)
            {
                Steamworks.SteamClient.RunCallbacks(); // Polls steam prior to NetworkManager polling network events (via ExecuteBefore)
            }
        }
    
        private void OnDisable()
        {
            if (_isInitialized)
            {
                Steamworks.SteamClient.Shutdown();
            }
        }
    }
}