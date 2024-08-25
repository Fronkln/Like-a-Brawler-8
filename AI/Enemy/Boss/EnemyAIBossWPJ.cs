using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossWPJ : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("kyoujin_atk"));
        }
    }
}
