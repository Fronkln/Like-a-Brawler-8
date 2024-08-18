using DragonEngineLibrary;
using System;
using System.Runtime.Remoting.Messaging;

namespace LikeABrawler2
{
    internal class EnemyEvasionModule : EnemyModule
    {
        public int BaseEvasionChance { get; private set; } = 15;
        private int m_oldEvasion = 0;

        public float LastEvasionTime = 9999;
        public int RecentlyEvadedAttacks = 0;

        //Bosses get an evasion boost after they recently get up
        private float m_evasionBoostDuration = 0;



        public override void Update()
        {
            base.Update();

            if (LastEvasionTime > 1.8f)
                RecentlyEvadedAttacks = 0;

            float delta = DragonEngine.deltaTime;

            if (m_evasionBoostDuration > 0)
                m_evasionBoostDuration -= delta;

            LastEvasionTime += delta;
        }

        public void SetEvasionChance(int chance)
        {
            m_oldEvasion = chance;
            BaseEvasionChance = chance;
        }

        public void RestoreOldEvasionChance()
        {
            SetEvasionChance(m_oldEvasion);
        }

        public void OnGetUp()
        {
            if (AI.IsBoss())
                m_evasionBoostDuration = 1.5f;
        }

        public void DoEvasion()
        {
            AI.Character.HumanModeManager.ToSway();
            LastEvasionTime = 0;
            RecentlyEvadedAttacks++;
            AI.RecentHitsWithoutDefensiveMove = 0;
            AI.RecentDefensiveAttacks++;
            AI.RecentHitsWithoutAttack++;
        }

        public bool ShouldEvade(BattleDamageInfoSafe inf)
        {
            if (AI.IsMortalAttackOrPreparing())
                return false;

            if (AI.IsBeingJuggled())
                return false;

            if (AI.BrawlerInfo.IsDown || AI.BrawlerInfo.IsGettingUp)
                return false;

            if (AI.BrawlerInfo.IsAttack)
                return false;

            if (AI.BrawlerInfo.IsSync)
                return false;

            if (AI.Fighter.GetStatus().IsSuperArmor() /*|| IsCounterAttacking*/)
                return false;

            if (!inf.Attacker.IsValid())
                return false;

            if (!AI.Character.IsFacingEntity(inf.Attacker))
                return false;

            /*
            if (AI.BlockModule.ShouldBlockAttack(inf))
                return false;
            */

            bool firstEvasion = ShouldEvadeFirstAttack();

            if (firstEvasion)
                return firstEvasion;
            else
                //Make this a proper algorithm later
                return new Random().Next(0, 101) <= (BaseEvasionChance * (m_evasionBoostDuration > 0 ? 2f : 1f));
        }

        //First attack = Hasnt got hit since 2.5 seconds
        public bool ShouldEvadeFirstAttack()
        {
            const float h_firstEvasionChance = 40;

            float chance = h_firstEvasionChance;

            if (!AI.IsBoss())
                chance *= 0.5f;

            if (AI.LastHitTime < 2.5f)
                return false;
            else
                return new Random().Next(0, 101) <= chance;
        }
    }
}
