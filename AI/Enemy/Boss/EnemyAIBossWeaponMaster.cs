using System;

namespace LikeABrawler2
{
    internal class EnemyAIBossWeaponMaster : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("wp_master_atk_a"));
        }
    }
}
