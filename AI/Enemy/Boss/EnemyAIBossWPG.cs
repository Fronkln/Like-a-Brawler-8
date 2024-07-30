using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossWPG : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("slugger_atk_a"));
        }

        public override bool TransitMortalAttack()
        {
            m_mortalSkill = DBManager.GetSkill("e_boss_wpg_mortal_attack");
            return true;
        }
    }
}
