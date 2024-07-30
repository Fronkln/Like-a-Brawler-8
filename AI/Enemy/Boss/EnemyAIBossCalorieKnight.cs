using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal class EnemyAIBossCalorieKnight : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("calorie_knight_atk"));
        }

        public override bool TransitMortalAttack()
        {
            m_mortalSkill = DBManager.GetSkill("e_boss_calorie_knight_mortal_attack");
            return true;
        }
    }
}
