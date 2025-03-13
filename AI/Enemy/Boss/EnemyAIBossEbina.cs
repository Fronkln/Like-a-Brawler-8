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
            CounterAttacks.Clear();
            CounterAttacks.Add(DBManager.GetSkill("boss_ebina_e_atk_a"));
        }

        public override bool CanHAct()
        {
            return base.CanHAct() && BrawlerBattleManager.ActionBattleTime > 15f;
        }

        public override bool DamageExecValid(IntPtr battleDamageInfo)
        {
            bool baseVal = base.DamageExecValid(battleDamageInfo);

            if (!baseVal)
                return false;

            if (Character.GetMotion().GmtID == (MotionID)17862) //skl_pvk
            {
                EntityHandle<Character> attacker = new BattleDamageInfoSafe(battleDamageInfo).Attacker;

                if (attacker.IsValid())
                {
                    if (Vector3.Distance(attacker.Get().Transform.Position, Character.Transform.Position) <= 2.5f)
                    {
                        BattleTurnManager.ForceCounterCommand(Fighter, attacker.Get().TryGetPlayerFighter(), DBManager.GetSkill("boss_daigo_counter_atk"));
                    }

                    return false;
                }
            }


            return baseVal;
        }
    }
}
