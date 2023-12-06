

using Steamworks;

namespace Managers
{
    public static class Achievements
    {
        public static void Initialize()
        {
            if (!SteamClient.IsValid) return;
            
            //Steamworks.SteamServerStats.RequestUserStatsAsync(SteamClient.)
        }
        
        public static void Trigger(string label)
        {
            if (!SteamClient.IsValid) return;
            
            //Steamworks.SteamServerStats.RequestUserStatsAsync(SteamClient.)
        }
    }
}