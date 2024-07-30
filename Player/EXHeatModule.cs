using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    public static class EXModule
    {
        private static RepeatingTask m_exHeatDecay = new RepeatingTask(
            delegate
            {
                if(!Mod.IsGamePaused && !BrawlerPlayer.GodMode)
                if(BrawlerBattleManager.CurrentPhase == BattleTurnManager.TurnPhase.Action)
                if (BrawlerPlayer.IsExtremeHeat)
                    if(Player.GetHeatNow(BrawlerPlayer.CurrentPlayer) > 0)
                        Player.SetHeatNow(BrawlerPlayer.CurrentPlayer, Player.GetHeatNow(BrawlerPlayer.CurrentPlayer) - 1);
            }, !Mod.IsDemo() ? 0.1f : 0.2f //Make it decay slower in demo for more player experiment
            );

        public static void Update()
        {
            if (!BrawlerPlayer.IsExtremeHeat|| BrawlerBattleManager.IsHAct || BrawlerBattleManager.CurrentPhase != BattleTurnManager.TurnPhase.Action)
            {
                m_exHeatDecay.Paused = true;
                return;
            }

            m_exHeatDecay.Paused = false;

            if (BrawlerPlayer.IsExtremeHeat && !BrawlerBattleManager.IsHAct)
            {
                int heat = Player.GetHeatNow(BrawlerPlayer.CurrentPlayer);

                if (heat <= 0 && !BrawlerBattleManager.IsHAct && !BrawlerFighterInfo.Player.IsSync && !BrawlerFighterInfo.Player.IsAttack)
                {
                    BrawlerPlayer.OnExtremeHeatModeOFF();
                }
            }
        }
    }
}
