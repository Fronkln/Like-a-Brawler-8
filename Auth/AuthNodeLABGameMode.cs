using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2.Auth
{
    internal static class AuthNodeLABGameMode
    {
        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, uint unk)
        {
            DragonEngine.Log("Character decision!");

            Player.ID playerID = (Player.ID)Marshal.ReadInt32((IntPtr)(thisObj.ToInt64() + 52));
            BrawlerBattleManager.ChangeCharacter(playerID);

            if (playerID == Player.ID.kiryu)
                BrawlerBattleManager.ChangeToRealtime();
            else
                BrawlerBattleManager.ChangeToTurnBased();
        }
    }
}
