using System;
using DragonEngineLibrary;
using ElvisCommand;

namespace LikeABrawler2
{
    internal static class AuthNodeButtonMash
    {
        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, IntPtr parent)
        {
            if (HeatActionManager.PerformingHAct == null)
                return;

            if (BattleManager.PadInfo.CheckCommand(BattleButtonID.light, 0, 10, 0))
            {
                if(HeatActionManager.PerformingHAct.Map.ContainsKey(HeatActionActorType.Enemy1))
                {
                    var enemy = HeatActionManager.PerformingHAct.Map[HeatActionActorType.Enemy1];
                    var enemyStatus = enemy.GetStatus();

                    long dmg = (long)(BrawlerBattleManager.PlayerFighter.GetStatus().AttackPower * 0.1f);
                    long newHealth = enemyStatus.CurrentHP - dmg;

                    enemy.GetStatus().SetHPCurrent(newHealth);

                    if (newHealth <= 0)
                        enemy.Character.ToDead();
                }
            }
        }
    }
}
