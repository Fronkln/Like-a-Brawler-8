namespace LikeABrawler2
{
    internal class EnemyAIBossSumo : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("big_sumou_atk"));
        }
    }
}
