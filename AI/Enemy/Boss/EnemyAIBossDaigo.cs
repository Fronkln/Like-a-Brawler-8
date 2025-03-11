using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossDaigo : EnemyAIBoss
    {
        public static EnemyAIBossDaigo Instance;
        private float m_hactCooldown = 0;

        public override void Awake()
        {
            base.Awake();

            Instance = this;
            HActList = YazawaCommandManager.LoadYHC("boss/daigo.ehc");
            CounterAttacks.Add(DBManager.GetSkill("boss_daigo_counter_atk"));
        }

        public override void CombatUpdate()
        {
            base.CombatUpdate();
        }

    }
}
