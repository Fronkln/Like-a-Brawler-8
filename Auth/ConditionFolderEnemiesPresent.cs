using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal static unsafe class ConditionFolderEnemiesPresent
    {
        //Used in Brawler_finishblow
        public static bool CheckDisabled(IntPtr disableInfo, IntPtr node)
        {
            if (Mod.IsTurnBased())
                return true;

            bool disabled = BrawlerBattleManager.AllEnemies.Length <= 0;

            return disabled;
        }
    }
}
