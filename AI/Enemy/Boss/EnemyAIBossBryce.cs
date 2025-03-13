using DragonEngineLibrary;
using System;

namespace LikeABrawler2
{
    internal class EnemyAIBossBryce : EnemyAIBoss
    {
        private bool m_phaseChanged = false;

        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("bouncer_atk_a"));
            CounterAttacks.Add(DBManager.GetSkill("hecaton_atk_a"));
        }

        public override void CombatUpdate()
        {
            base.CombatUpdate();

            if (!m_phaseChanged)
            {
                if (Asset.GetArmsCategory(Fighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().AssetID) == AssetArmsCategoryID.E)
                {
                    m_phaseChanged = true;
                    OnPhaseChange();
                }
            }
            else
            {
                //Triggers around 10-15% HP, desperate Bryce
                Fighter.GetStatus().SetSuperArmor(true, false);
            }
        }

        private void OnPhaseChange()
        {
            //Sword Bryce
            CounterAttacks.Clear();
            CounterAttacks.Add(DBManager.GetSkill("yakuza_katana_atk_b"));

            DragonEngine.Log("Sword Bryce");
        }
    }
}
