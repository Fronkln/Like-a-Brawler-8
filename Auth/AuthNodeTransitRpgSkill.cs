using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2.Auth
{
    internal class AuthNodeTransitRpgSkill
    {
        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, IntPtr parent)
        {
            IntPtr skillNamePtr = (IntPtr)(thisObj.ToInt64() + 48);
            string hactName = Marshal.PtrToStringAnsi(skillNamePtr);

            BattleTurnManager.ForceCounterCommand(BrawlerBattleManager.PlayerFighter, BrawlerBattleManager.AllEnemiesNearest[0], DBManager.GetSkill("e_yamai_mortal_attack_2"));
            DragonEngine.Log("transit");
        }
    }
}
