using System;
using System.Collections.Generic;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal static class AuraManager
    {
        public static Dictionary<RPGJobID, AuraDefinition> AuraDefs = new Dictionary<RPGJobID, AuraDefinition>();

        static AuraDefinition LastAura = new AuraDefinition(EffectEventCharaID.invalid, EffectEventCharaID.invalid);

        private static bool m_auraPlaying = false;
        private static bool m_hactFlag = false;


        static AuraManager()
        {
            AuraDefs = new Dictionary<RPGJobID, AuraDefinition>()
            {
                [RPGJobID.invalid] = new AuraDefinition(EffectEventCharaID.OgrefHeatAuraKr01, EffectEventCharaID.invalid),
                [RPGJobID.kasuga_braver] = new AuraDefinition(EffectEventCharaID.boss_mabuchi_lp, EffectEventCharaID.invalid),
                [RPGJobID.kiryu_01] = new AuraDefinition((EffectEventCharaID)2764, EffectEventCharaID.invalid),
                [RPGJobID.man_05] = new AuraDefinition((EffectEventCharaID)2766, EffectEventCharaID.invalid),
                [RPGJobID.man_07] = new AuraDefinition((EffectEventCharaID)2765, EffectEventCharaID.invalid),
                [RPGJobID.man_footballer] = new AuraDefinition((EffectEventCharaID)2767, EffectEventCharaID.invalid),
                [RPGJobID.man_western] = new AuraDefinition((EffectEventCharaID)2768, EffectEventCharaID.invalid),
            };

            HeatActionManager.OnHActEndEvent += HactEnd;
        }

        public static void Update()
        {
            RPGJobID playerJob = Player.GetCurrentJob(BrawlerPlayer.CurrentPlayer);
            AuraDefinition aura;

            if (!AuraDefs.ContainsKey(playerJob))
                aura = AuraDefs[RPGJobID.invalid];
            else
                aura = AuraDefs[playerJob];

            if (LastAura != aura)
                OnAuraChanged();

            LastAura = aura;

            if (BrawlerBattleManager.IsHAct)
            {
                if (m_auraPlaying)
                    m_hactFlag = true;

                m_auraPlaying = false;
            }
            else
            {
                LastAura = aura;

                if (ShouldShowHeatAura())
                {
                    if (!m_auraPlaying)
                        StartAura();
                }
                else
                    StopAura();
            }
        }


        private static void HactEnd()
        {
            if(m_hactFlag)
                StartAura();

            m_hactFlag = false;
        }

        private static void Reset()
        {
            m_hactFlag = false;
        }

        private static bool ShouldShowHeatAura()
        {
            if (BrawlerBattleManager.CurrentPhase >= BattleTurnManager.TurnPhase.BattleResult)
                return false;

            if (!BrawlerPlayer.IsExtremeHeat)
                return false;

            return true;
        }

        private static void OnAuraChanged()
        {
            StopAura();
        }

        private static void StopAura()
        {
            BrawlerBattleManager.PlayerCharacter.Components.EffectEvent.Get().StopEvent(LastAura.Loop, false);
            m_auraPlaying = false;
        }

        private static void StartAura()
        {
            BrawlerBattleManager.PlayerCharacter.Components.EffectEvent.Get().PlayEventOverride(LastAura.Loop);
            m_auraPlaying = true;
        }
    }
}
