using System;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal static class ScreenEffectManager
    {
        private static bool m_physicalWarning = false;

        public static void Update()
        {
            if (!m_physicalWarning)
            {
                if (ShouldShowPhysicalWarning())
                {
                    ShowPhysicalWarning();
                    m_physicalWarning = true;
                }
            }
            else
            {
                if(!ShouldShowPhysicalWarning())
                {
                    if (EffectEventManager.IsPlayingScreen(69))
                        EffectEventManager.StopScreen(69);

                    m_physicalWarning = false;
                }    
            }
        }

        public static void ShowPhysicalWarning()
        {
            if (!EffectEventManager.IsPlayingScreen(69))
                EffectEventManager.PlayScreen(69, true, true, 0.5f, true);
        }

        public static bool ShouldShowPhysicalWarning()
        {
            return Mod.IsRealtime() && BrawlerBattleManager.Battling && BrawlerBattleManager.ActionBattleTime > 0f && BrawlerBattleManager.PlayerFighter.IsBrawlerCriticalHP() && !BrawlerBattleManager.PlayerFighter.IsDead() && !(BrawlerBattleManager.IsHAct && !HeatActionManager.IsY8BHact);
        }
    }
}
