namespace LikeABrawler2
{
    internal class EnemyAIBossKuwaki : EnemyAIBossWPE
    {
        //Else the hact sequence may break
        public override bool CanBeHActed()
        {
            if (!Fighter.IsHPBelowRatio(0.5f))
                return false;

            return base.CanBeHActed();
        }

        public override bool TransitMortalAttack()
        {
            if (!Fighter.IsHPBelowRatio(0.35f))
                return false;

            return base.TransitMortalAttack();
        }
    }
}
