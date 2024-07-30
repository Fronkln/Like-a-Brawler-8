using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossYamai2 : EnemyAIBoss
    {
        private bool m_hactDoOnce = false;
        private bool m_awaitingHAct = false;

        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("e_yamai_counter"));
        }

        public override bool TransitMortalAttack()
        {
            m_mortalSkill = DBManager.GetSkill("e_yamai_mortal_attack");
            return true;
        }

        public override EntityHandle<Character> OverrideMarkTarget(EntityHandle<Character> original)
        {
            return BrawlerBattleManager.PlayerCharacter.UID;
        }
    }
}
