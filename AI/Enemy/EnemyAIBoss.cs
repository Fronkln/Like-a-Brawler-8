using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBoss : BaseEnemyAI
    {
        public override void Awake()
        {
            base.Awake();

            EvasionModule.SetEvasionChance(20);
        }

        public override bool IsBoss()
        {
            return true;
        }

        public override void CombatUpdate()
        {
            base.CombatUpdate();

            if (!BrawlerBattleManager.IsHActOrWaiting)
                if (m_numMortalAttacks <= 0)
                    if (Fighter.IsHPBelowRatio(0.51f))
                        TransitMortalAttack();
        }

        protected override void OnTakeDamageEvent(BattleDamageInfoSafe dmg)
        {
            base.OnTakeDamageEvent(dmg);

            if (RecentHitsWithoutAttack > 6)
                if (!m_hasAntiSpamArmor && m_antiSpamArmorCooldown <= 0)
                {
                    m_antiSpamArmorCooldown = 10f;
                    ToggleAntiSpamArmor(true);

                    DragonEngine.Log("HYPERARMOR TIME!");
                }
        }
    }
}
