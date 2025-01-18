using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class BrawlerSpecial
    {  
        public static bool TransitDragonFear()
        {
            if (BrawlerBattleManager.IsEncounter)
                return false;

            if (BrawlerBattleManager.ActionBattleTime > 4.5f)
                return false;

            Random rnd = new Random();

            foreach(Fighter fighter in BrawlerBattleManager.AllEnemies)
            {

                BaseEnemyAI ai = EnemyManager.GetAI(fighter.Character.UID);
                if (ai == null)
                    continue;

                ai.ApplyFear(rnd.Next(4.5f, 8f));
            }

            BrawlerBattleManager.SkipTurn();

            return true;
        }

        public static void TransitSoloFight()
        {
            BrawlerBattleManager.SoloBattleOnce = true;
            NakamaManager.Change(1, Player.ID.invalid);
            NakamaManager.Change(2, Player.ID.invalid);
            NakamaManager.Change(3, Player.ID.invalid);

            //dont use on encounter battles because this will permanently alter the combat UI until its respawned
            BrawlerUIManager.DoSoloFight();
        }

        public static void RemoveNakamaUI(int idx)
        {
            var gauge = BrawlerUIManager.GaugeRoot.GetChild(idx);
            gauge.Release();
        }
    }
}
