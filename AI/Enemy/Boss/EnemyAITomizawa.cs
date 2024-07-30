using System;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal class EnemyAITomizawa : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();
            CounterAttacks.Add(DBManager.GetSkill("tomizawa_atk_a"));
        }

        public override bool TransitMortalAttack()
        {
            m_mortalSkill = DBManager.GetSkill("e_tomizawa_mortal_attack");
            return true;
        }
    }
}
