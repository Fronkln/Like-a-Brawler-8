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
    internal class EnemyAIBossYamai1 : EnemyAIBoss
    {
        private bool m_hactDoOnce = false;
        private bool m_awaitingHAct = false;

        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("e_yamai_counter"));
        }

        public override void Update()
        {
            base.Update();

            if (m_awaitingHAct)
                if (BrawlerBattleManager.IsHAct)
                {
                    m_hactDoOnce = true;
                    m_awaitingHAct = false;

                    HeatActionManager.OnHActEndEvent += OnHActEnd;
                }
        }

        public override void CombatUpdate()
        {
            base.CombatUpdate();

            if (!m_hactDoOnce)
                if (Fighter.IsHPBelowRatio(0.099f))
                {
                    //Player needs to have his turn for the action sequence to play
                    BrawlerBattleManager.ForceGivePlayerTurn();
                    BrawlerBattleManager.SkipTurn();
                    m_awaitingHAct = true;

                    return;
                }
        }

        private void OnHActEnd()
        {
            new DETaskTime(0.3f,
            delegate
            {
                BrawlerBattleManager.SkipTurn();
            });

            HeatActionManager.OnHActEndEvent -= OnHActEnd;
        }

        public override bool TransitMortalAttack()
        {
            m_mortalSkill = DBManager.GetSkill("e_yamai_mortal_attack");
            return true;
        }
    }
}
