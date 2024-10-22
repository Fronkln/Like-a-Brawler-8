using System;

namespace LikeABrawler2
{
    internal class EnemyAIBossBryce : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("bouncer_atk_a"));
            CounterAttacks.Add(DBManager.GetSkill("hecaton_atk_a"));
        }
    }
}
