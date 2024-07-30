using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossWPY : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("e_boss_wpy_dmg_rev"));
        }

        public override bool TransitMortalAttack()
        {
            m_mortalSkill = DBManager.GetSkill("e_boss_wpy_mortal_attack");
            return true;
        }
    }
}
