﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossWPR : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("bouncer_atk_a"));
            CounterAttacks.Add(DBManager.GetSkill("hecaton_atk_a"));
        }
    }
}
