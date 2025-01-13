using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBoss : BaseEnemyAI
    {
        private float m_punishCooldown = 0;

        protected float PunishChance = BOSS_PUNISH_FAR_PLAYER_BASE_CHANCE;
        protected float PunishDistance = BOSS_PUNISH_FAR_PLAYER_BASE_DIST;

        private const float BOSS_PUNISH_FAR_PLAYER_BASE_DIST = 3.5f;
        private const int BOSS_PUNISH_FAR_PLAYER_BASE_CHANCE = 45;
        private const float BOSS_PUNISH_FAR_PLAYER_COOLDOWN = 7f;

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

            if (m_punishCooldown >= 0)
                m_punishCooldown -= DragonEngine.deltaTime;
        }

        protected override void OnTakeDamageEvent(BattleDamageInfoSafe dmg)
        {
            Character attacker = dmg.Attacker;
            bool isBackAttack = false;

            if (attacker.IsValid())
                isBackAttack = Vector3.Distance(attacker.Transform.Position, Character.Transform.Position) <= 3f && !Character.IsFacingEntity(attacker, 0.1f);

            if (RecentHitsWithoutAttack > 6 && !isBackAttack)
                if (!m_hasAntiSpamArmor && m_antiSpamArmorCooldown <= 0)
                {
                    m_antiSpamArmorCooldown = 10f;
                    ToggleAntiSpamArmor(true);

                    DragonEngine.Log("HYPERARMOR TIME!");
                }
        }


        protected override void OnSway()
        {
            base.OnSway();
            m_swayAttack = new Random().Next(0, 101) <= SwayAttackChance && Character.IsFacingEntity(BrawlerBattleManager.PlayerCharacter) && DistToPlayer <= 4f;
        }

        protected override void OnPlayerStartAttackingEvent()
        {
            base.OnPlayerStartAttackingEvent();

            if(Character.IsFacingEntity(BrawlerBattleManager.PlayerCharacter) && BrawlerBattleManager.PlayerCharacter.IsFacingEntity(Character))
            {
                float dist = Vector3.Distance(Character.Transform.Position, BrawlerBattleManager.PlayerCharacter.Transform.Position);

                if (dist >= 3.5f)
                {
                    bool shouldPunish = new Random().Next(0, 101) <= PunishChance;

                    if(shouldPunish)
                    {
                        m_punishCooldown = BOSS_PUNISH_FAR_PLAYER_COOLDOWN;
                        OnPunishFarPlayer();
                    }
                }
            }
        }

        protected virtual void OnPunishFarPlayer()
        {
           
        }
    }
}
