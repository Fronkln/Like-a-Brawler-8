using System;
using DragonEngineLibrary;
using System.Runtime.InteropServices;

namespace LikeABrawler2.Auth
{
    public static class AuthNodeLABGamemodeDecision
    {
        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, IntPtr parent)
        {
            DragonEngine.Log("Gamemode decision!");

            int gamemode = Marshal.ReadInt32((IntPtr)(thisObj.ToInt64() + 52));

            if(gamemode == 0)
                BrawlerBattleManager.ChangeToTurnBased();
            else
                BrawlerBattleManager.ChangeToRealtime();

            IniSettings.Write();
        }
    }
}
