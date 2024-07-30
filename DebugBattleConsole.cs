using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    public static class DebugBattleConsole
    {
        public static void Update()
        {
#if !DEBUG
        return;
#endif
            if (!BrawlerBattleManager.Battling)
                return;

            return;

            Console.WriteLine("Current Phase: " + BattleTurnManager.CurrentPhase);
            Console.WriteLine("Current ActionStep: " + BattleTurnManager.CurrentActionStep);
            Console.WriteLine("Current Action Type: " + BattleTurnManager.CurrentActionType);

            Console.WriteLine("");
        }
    }
}
