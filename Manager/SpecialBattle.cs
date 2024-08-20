using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    public static class SpecialBattle
    {
        private static bool m_adachiSwapped = false;


        public static void OnBattleEnd()
        {
            m_adachiSwapped = false;
        }

        /// <summary>
        /// elvis_lng04_btl11_0030 theater split fight. Adachi becomes protag momentarily
        /// </summary>
        public unsafe static void SplitFight()
        {
            if (m_adachiSwapped)
                return;
    
            int adachiIdx = NakamaManager.FindIndex(Player.ID.adachi);
            BrawlerBattleManager.MakeNakamaMain((uint)adachiIdx);

            m_adachiSwapped = true;


            int tomizawaIdx = NakamaManager.FindIndex(Player.ID.tomizawa);

            if (tomizawaIdx > 0)
            {
                Fighter tomizawaFighter = FighterManager.GetFighter((uint)tomizawaIdx);
                BaseAI tomizawaAI = tomizawaFighter.TryGetAI();

                if(tomizawaAI is BaseSupporterAI)
                    (tomizawaAI as BaseSupporterAI).TakeTurn();
            }

            DragonEngine.Log("Adachi Files");
        }

        public static void Update()
        {
            switch (BrawlerBattleManager.BattleConfigID)
            {
                case 189:
                    if (m_adachiSwapped)
                    {
                        if (BrawlerPlayer.IsOtherPlayer())
                        {
                            if (BrawlerBattleManager.CurrentPhase == BattleTurnManager.TurnPhase.Event)
                                if (!HeatActionManager.IsY8BHact)
                                    BrawlerBattleManager.MakeNakamaMain(0); // back to kasuga
                        }
                    }
                    break;
            }
        }
    }
}
