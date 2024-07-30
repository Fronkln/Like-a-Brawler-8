using System;
namespace LikeABrawler2
{
    internal class EnemyAIYamaiHostess : BaseEnemyAI
    {
        public override bool CanBeHActed()
        {
            return false;
        }

        public override bool AllowCanGetTurn()
        {
            return LastTurnTime >= 35;
        }
    }
}
