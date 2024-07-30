using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossLandSurfer : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("land_surfer_clash"));
            CounterAttacks.Add(DBManager.GetSkill("land_surfer_tornado_2"));
        }
    }
}
