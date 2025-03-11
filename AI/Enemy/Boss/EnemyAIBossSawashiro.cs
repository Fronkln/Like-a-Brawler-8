using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal class EnemyAIBossSawashiro : EnemyAIBoss
    {
        private AssetArmsCategoryID m_leftArmsCategory = 0;
        private AssetArmsCategoryID m_RightArmsCategory = 0;

        public int Mode = 0;

        public override void CombatUpdate()
        {
            base.CombatUpdate();

            AssetArmsCategoryID leftArmsCategory = Asset.GetArmsCategory(Fighter.GetWeapon(AttachmentCombinationID.left_weapon).Unit.Get().AssetID);
            AssetArmsCategoryID rightArmsCategory = Asset.GetArmsCategory(Fighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().AssetID);

            if (m_leftArmsCategory != leftArmsCategory || m_RightArmsCategory != rightArmsCategory)
            {
                m_leftArmsCategory = leftArmsCategory;
                m_RightArmsCategory = rightArmsCategory;
                OnChangeWeapon();
            }
        }

        public void OnChangeWeapon()
        {
            CounterAttacks.Clear();

            //BRT (cane)
            if (m_RightArmsCategory == AssetArmsCategoryID.A)
            {
                CounterAttacks.Add(DBManager.GetSkill("boss_sawashiro_brt_atk_a"));
                Mode = 1;
            }

            //B (dual knives)
            if (m_RightArmsCategory == AssetArmsCategoryID.B)
            {
                CounterAttacks.Add(DBManager.GetSkill("boss_sawashiro_x_atk_b"));
                Mode = 2;
            }

            //E (katana)
            if (m_RightArmsCategory == AssetArmsCategoryID.E)
            {
                CounterAttacks.Add(DBManager.GetSkill("boss_sawashiro_e_atk_a"));
                Mode = 3;
            }

            //Unarmed
            if(m_leftArmsCategory == AssetArmsCategoryID.invalid && m_RightArmsCategory == AssetArmsCategoryID.invalid)
            {
                CounterAttacks.Add(DBManager.GetSkill("boss_sawashiro_atk_a"));
                Mode = 4;
            }
        }

        public override bool TransitMortalAttack()
        {
            switch(Mode)
            {
                case 1:
                    m_mortalSkill = DBManager.GetSkill("e_sawashiro_brt_mortal_attack");
                    break;
                case 2:
                    m_mortalSkill = DBManager.GetSkill("e_sawashiro_x_mortal_attack");
                    break;
                case 3:
                    m_mortalSkill = DBManager.GetSkill("e_sawashiro_e_mortal_attack");
                    break;
            }

            return true;
        }
    }
}
