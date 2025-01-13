    using System;
using System.Collections.Generic;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    public static class HeatModule
    {
        //Play sounds/effects once player can do heat actions
        private static int m_HeatActionCount; //calculated amount of heat actions the player can do as of now
        private static bool m_canDoHeatDoOnce = true;

        public static void Update()
        {
            if (!BrawlerBattleManager.Battling)
            {
                m_canDoHeatDoOnce = true;
                return;
            }

            int curHeat = Player.GetHeatNow(Player.ID.kasuga);
            int heatMax = Player.GetHeatMax(Player.ID.kasuga);

            int numHeatActions = 0;

            if (heatMax < (int)HeatActionManager.GetHActCost())
            {
                if (curHeat == heatMax)
                    numHeatActions = 1;
            }
            else
                numHeatActions = curHeat / (int)HeatActionManager.GetHActCost();

            if (numHeatActions > m_HeatActionCount)
                OnCanDoHeat();

            m_HeatActionCount = numHeatActions;
        }

        private static void OnCanDoHeat()
        {
            if (BrawlerPlayer.IsExtremeHeat)
                return;

            SoundManager.PlayCue(DBManager.GetSoundCuesheet("battle_common"), 15, 0);

            if(BrawlerPlayer.IsKasuga())
                BrawlerBattleManager.PlayerCharacter.Components.EffectEvent.Get().PlayEventOverride((EffectEventCharaID)2769);
            else
                BrawlerBattleManager.PlayerCharacter.Components.EffectEvent.Get().PlayEventOverride(EffectEventCharaID.OgrefHeatAuraKr02);
        }
    }
}
