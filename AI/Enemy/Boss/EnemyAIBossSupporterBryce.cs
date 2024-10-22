using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal class EnemyAIBossSupporterBryce : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();

            EnemyManager.ForcedAttacker = this;
        }

        public override void Update()
        {
            base.Update();

            if (LastTurnTime >= 15)
                EnemyManager.ForcedAttacker = this;
        }
    }
}
