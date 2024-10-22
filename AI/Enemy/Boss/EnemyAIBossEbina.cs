using System;
using DragonEngineLibrary;
using ElvisCommand;

namespace LikeABrawler2
{
    internal class EnemyAIBossEbina : EnemyAIBoss
    {
        private bool m_swappedWeaponOnce = false;

        public override void Awake()
        {
            base.Awake();

            HActList = YazawaCommandManager.LoadYHC("boss/ebina.ehc");
            CounterAttacks.Add(DBManager.GetSkill("boss_ebina_atk_a"));
        }

        public override void CombatUpdate()
        {
            base.CombatUpdate();

            if(!m_swappedWeaponOnce)
                if(BrawlerInfo.RightWeapon.IsValid())
                {
                    m_swappedWeaponOnce = true;
                    OnChangeToSword();
                }
        }

        private void OnChangeToSword()
        {
            HActList = null;

            CounterAttacks.Clear();
            CounterAttacks.Add(DBManager.GetSkill("e_ebina_e_counter"));
        }

        public override bool CanHAct()
        {
            return base.CanHAct() && BrawlerBattleManager.ActionBattleTime > 15f;
        }
    }
}
