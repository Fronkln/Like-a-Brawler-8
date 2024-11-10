using DragonEngineLibrary;
using DragonEngineLibrary.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class HumanModePatches : BrawlerPatch
    {
        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool HumanModeManagerDamageExecValid(IntPtr humanModeManager, IntPtr battleDamageInfo);
        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool HumanModeManageTransitDamage(IntPtr humanModeManager, IntPtr battleDamageInfo);

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool HumanModeManagerIsInputSway(IntPtr humanModeManager);

        private IntPtr m_humanModeDamageValidFunc;
        private IntPtr m_humanModeIsInputSwayFunc;

        public override void Init()
        {
            base.Init();

            m_humanModeDamageValidFunc = CPP.PatternSearch("40 56 57 41 56 41 57 48 83 EC ? 48 89 CF");
            m_humanModeIsInputSwayFunc = CPP.PatternSearch("48 89 5C 24 08 57 48 83 EC ? 0F B6 FA 48 8B D9 E8 ? ? ? ? 84 C0 74 ?");
        }

        protected override void SetActive()
        {
            base.SetActive();

            if (m_damageExecValidTrampoline == null)
                m_damageExecValidTrampoline = BrawlerPatches.HookEngine.CreateHook<HumanModeManagerDamageExecValid>(m_humanModeDamageValidFunc, HumanModeManager_DamageExecValid);

            if (m_isInputSwayTrampoline == null)
                 m_isInputSwayTrampoline = BrawlerPatches.HookEngine.CreateHook<HumanModeManagerIsInputSway>(m_humanModeIsInputSwayFunc, HumanModeManager_IsInputSway);

            BrawlerPatches.HookEngine.EnableHook(m_damageExecValidTrampoline);
            BrawlerPatches.HookEngine.EnableHook(m_isInputSwayTrampoline);
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            if (m_damageExecValidTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_damageExecValidTrampoline);

            if (m_isInputSwayTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_isInputSwayTrampoline);
        }

        private static HumanModeManagerIsInputSway m_isInputSwayTrampoline;
        private unsafe static bool HumanModeManager_IsInputSway(IntPtr humanModeManager)
        {
            if (Mod.IsTurnBased())
                return m_isInputSwayTrampoline(humanModeManager);

            HumanModeManager manager = new HumanModeManager() { Pointer = humanModeManager };
            Character human = manager.Human;


            if (manager.Human.UID == BrawlerBattleManager.PlayerCharacter.UID)
            {
                //Dont restart sway procedure when we are on our manual CFC defined sway
                if (manager.GetCommandName().StartsWith("Sway"))
                    return false;

                return m_isInputSwayTrampoline(humanModeManager);
            }
            else
                return m_isInputSwayTrampoline(humanModeManager);
        }

        private static HumanModeManagerDamageExecValid m_damageExecValidTrampoline;
        private unsafe static bool HumanModeManager_DamageExecValid(IntPtr humanModeManager, IntPtr battleDamageInfo)
        {
            if(Mod.IsTurnBased())
                return m_damageExecValidTrampoline(humanModeManager, battleDamageInfo);

            HumanModeManager manager = new HumanModeManager() { Pointer = humanModeManager };
            Character human = manager.Human;

            if(human.Attributes.player_id == BrawlerPlayer.CurrentPlayer)
            {
                if (!BrawlerPlayer.TransitDamage(new BattleDamageInfoSafe(battleDamageInfo)))
                    return false;
                else
                {
                    BrawlerPlayer.OnGetHit(new BattleDamageInfoSafe(battleDamageInfo));
                    return m_damageExecValidTrampoline(humanModeManager, battleDamageInfo);
                }
            }

            BaseEnemyAI enemy = EnemyManager.GetAI(human.UID);
            BaseSupporterAI supporter = SupporterManager.GetAI(human.UID);

            if(supporter != null)
            {
                int* dmg1 = (int*)(battleDamageInfo.ToInt64() + 0x120);
                int* dmg2 = (int*)(battleDamageInfo.ToInt64() + 0x120);

                long hp = supporter.Fighter.GetStatus().CurrentHP;

                if (supporter.Fighter.GetStatus().CurrentHP <= 1 || *dmg1 >= hp || *dmg2 >= hp)
                {
                    *dmg1 = 0;
                    *dmg2 = 0;
                }
            }

            if(enemy == null)
                return m_damageExecValidTrampoline(humanModeManager, battleDamageInfo);

            enemy.PreTakeDamage(battleDamageInfo);

            bool exec = enemy.DamageExecValid(battleDamageInfo);

            if (!exec)
                return false;
            else
            {
                
                bool result = m_damageExecValidTrampoline(humanModeManager, battleDamageInfo);
                //TODO: Not make this poopshit

                if (!HeatActionManager.IsHAct())
                {
                    BrawlerPlayer.OnHitEnemy(EnemyManager.GetAI(human.UID).Fighter, new BattleDamageInfoSafe(battleDamageInfo));
                    enemy.OnTakeDamage(new BattleDamageInfoSafe(battleDamageInfo));
                }

                return result;
            }
        }
    }
}
