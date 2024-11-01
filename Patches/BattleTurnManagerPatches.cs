using System;
using System.Runtime.InteropServices;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal unsafe class BattleTurnManagerPatches : BrawlerPatch
    {
        private IntPtr m_changePhaseFunc;
        private IntPtr m_changeStepFunc;
        private IntPtr m_btlTurnAfterDamageFunc;
        private IntPtr m_btlTurnFighterDeadFunc;
        private IntPtr m_btlTurnExecActionFunc;
        private IntPtr m_setupTurnFighterFunc;

        private delegate void BattleTurnManager_ChangePhase(IntPtr battleTurnManager, BattleTurnManager.TurnPhase phase);
        private delegate void BattleTurnManager_ChangeActionStep(IntPtr battleTurnManager, BattleTurnManager.ActionStep step);
        private delegate void BattleTurnManagerDmgNotify(IntPtr mng, IntPtr inf);
        private delegate void BattleTurnManagerExecPhase(IntPtr mng);
        private delegate void FighterSetupTurnBattleFighter(IntPtr fighter);

        public override void Init()
        {
            base.Init();

            m_changePhaseFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 89 5C 24 18 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 E0 E9 FF FF");
            m_changeStepFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 8B C4 48 89 58 18 55 56 57 41 54 41 55 41 56 41 57 48 8D A8 E8 F5 FF FF");
            m_btlTurnAfterDamageFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("40 56 41 56 48 83 EC ? 4C 8B 02");
            m_btlTurnExecActionFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("4D 85 ED 0F 84 ? ? ? ? 48 8D 8E BC 05 00 00") - 207;
            m_setupTurnFighterFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 89 5C 24 10 48 89 74 24 18 55 57 41 54 41 56 41 57 48 8D 6C 24 E0");
            m_btlTurnFighterDeadFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("40 53 41 56 48 83 EC ? 49 89 CE");

            //We want these two to be active always
            m_changeTurnPhase = BrawlerPatches.HookEngine.CreateHook<BattleTurnManager_ChangePhase>(m_changePhaseFunc, BattleTurnManager_ChangeTurnPhase);
            m_changeTurnStep = BrawlerPatches.HookEngine.CreateHook<BattleTurnManager_ChangeActionStep>(m_changeStepFunc, BattleTurnManager_ChangeTurnActionStep);

            //04.09.2024 i dont think we do actually
            //BrawlerPatches.HookEngine.EnableHook(m_changeTurnPhase);
            //BrawlerPatches.HookEngine.EnableHook(m_changeTurnStep);
        }

        protected override void SetActive()
        {
            base.SetActive();

            if (_btlTurnManagerDmgNotifyTrampoline == null)
            {
                _btlTurnManagerDmgNotifyTrampoline = BrawlerPatches.HookEngine.CreateHook<BattleTurnManagerDmgNotify>(m_btlTurnAfterDamageFunc, BattleTurnManager_OnAfterDamage);
                _btlTurnManagerFighterDeadNotifyTrampoline = BrawlerPatches.HookEngine.CreateHook<BattleTurnManagerDmgNotify>(m_btlTurnFighterDeadFunc, BattleTurnManager_OnFighterDead);
                _btlTurnManagerExecPhaseActionTrampoline = BrawlerPatches.HookEngine.CreateHook<BattleTurnManagerExecPhase>(m_btlTurnExecActionFunc, BattleTurnManager_ExecPhaseAction);
                m_setupTurnBattleFighter = BrawlerPatches.HookEngine.CreateHook<FighterSetupTurnBattleFighter>(m_setupTurnFighterFunc, SetupTurnBattleFighter);
            }

            BrawlerPatches.HookEngine.EnableHook(m_setupTurnBattleFighter);
            BrawlerPatches.HookEngine.EnableHook(_btlTurnManagerDmgNotifyTrampoline);
            BrawlerPatches.HookEngine.EnableHook(_btlTurnManagerFighterDeadNotifyTrampoline);
            BrawlerPatches.HookEngine.EnableHook(_btlTurnManagerExecPhaseActionTrampoline);
            BrawlerPatches.HookEngine.EnableHook(m_changeTurnPhase);
            BrawlerPatches.HookEngine.EnableHook(m_changeTurnStep);
        }

        protected override void SetInactive()
        {
            base.SetInactive();


            if (_btlTurnManagerDmgNotifyTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(_btlTurnManagerDmgNotifyTrampoline);

            if (_btlTurnManagerExecPhaseActionTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(_btlTurnManagerExecPhaseActionTrampoline);

            if (m_setupTurnBattleFighter != null)
                BrawlerPatches.HookEngine.DisableHook(m_setupTurnBattleFighter);

            if (m_changeTurnStep != null)
                BrawlerPatches.HookEngine.DisableHook(m_changeTurnPhase);
            if (m_changeTurnStep != null)
                BrawlerPatches.HookEngine.DisableHook(m_changeTurnStep);
        }

        private static BattleTurnManager_ChangePhase m_changeTurnPhase;
        private static void BattleTurnManager_ChangeTurnPhase(IntPtr battleTurnManager, BattleTurnManager.TurnPhase phase)
        {
            DragonEngine.Log(phase);

            if (Mod.IsTurnBased())
            {
                m_changeTurnPhase.Invoke(battleTurnManager, phase);
                return;
            }

            if (phase == BattleTurnManager.TurnPhase.Event && HeatActionManager.IsY8BHact)
            {
                //this will immediately lead back to Action and bring turn based UI visible
                //new DETaskTime(0.01f, delegate { BattleTurnManager.ChangePhase(BattleTurnManager.TurnPhase.Start); });
                return;
            }
            m_changeTurnPhase.Invoke(battleTurnManager, phase);

            //Reduce cleanup phase time and not wait the full 5 seconds.
            if (phase == BattleTurnManager.TurnPhase.Cleanup)
            {
                if (BrawlerBattleManager.BattleEndedInY8BHAct)
                {
                    float* val = (float*)(BattleTurnManager.Pointer().ToInt64() + 0x560);
                    *val = 1.5f;
                }
            }
        }

        private static BattleTurnManager_ChangeActionStep m_changeTurnStep;
        private static void BattleTurnManager_ChangeTurnActionStep(IntPtr battleTurnManager, BattleTurnManager.ActionStep step)
        {
            if (Mod.IsTurnBased())
            {
                m_changeTurnStep.Invoke(battleTurnManager, step);
                return;
            }

            /* Uncomment: Ichiban cannot move or lockon
             * Check if battleturmanager/character inputinfo updates when this is true or something, or see what happens when you prevent it from being written to.
            if (BrawlerBattleManager.CurrentPhase == BattleTurnManager.TurnPhase.Action && (step == BattleTurnManager.ActionStep.Ready || step == BattleTurnManager.ActionStep.ActionStart || step == BattleTurnManager.ActionStep.TriggeredEvent))
                return;
            */

            if (step == BattleTurnManager.ActionStep.SelectTarget && SupporterManager.SkipDoubleTurn)
            {
                SupporterManager.SkipDoubleTurn = false;
                step = BattleTurnManager.ActionStep.TriggeredEvent;
            }

            m_changeTurnStep.Invoke(battleTurnManager, step);
        }

        private static BattleTurnManagerDmgNotify _btlTurnManagerDmgNotifyTrampoline;
        private static void BattleTurnManager_OnAfterDamage(IntPtr mng, IntPtr inf)
        {
            FighterID id = Marshal.PtrToStructure<FighterID>(inf + 0x8);

            if (id.Handle == BrawlerBattleManager.PlayerCharacter.UID)
            {
                if (!IniSettings.ShowPlayerDamage || !BrawlerPlayer.AllowDamage(new BattleDamageInfo()))
                    return;
            }
            else
                if (!IniSettings.ShowEnemyDamage)
                return;

            _btlTurnManagerDmgNotifyTrampoline(mng, inf);
        }

        private static BattleTurnManagerDmgNotify _btlTurnManagerFighterDeadNotifyTrampoline;
        private static void BattleTurnManager_OnFighterDead(IntPtr mng, IntPtr inf)
        {
            _btlTurnManagerFighterDeadNotifyTrampoline(mng, inf);
            BrawlerBattleManager.NotifyFighterDeath(inf);
        }


        private static BattleTurnManagerExecPhase _btlTurnManagerExecPhaseActionTrampoline;
        private static void BattleTurnManager_ExecPhaseAction(IntPtr mng)
        {
            //Return, and it wont update turn based combat including turns, important for special events like
            //mortal attacks

            if (EnemyManager.IsAnyoneMortalAttacking() || MortalReversalManager.Procedure || RevelationManager.RevelationProcedure)
                return;

            _btlTurnManagerExecPhaseActionTrampoline(mng);
        }

        private static FighterSetupTurnBattleFighter m_setupTurnBattleFighter = null;
        private static void SetupTurnBattleFighter(IntPtr fighterPtr)
        {
            if (Mod.IsTurnBased())
            {
                m_setupTurnBattleFighter(fighterPtr);
                return;
            }

            Fighter fighter = new Fighter(fighterPtr);

            if (fighter.IsPlayer())
                return;

            if (fighter.IsPartyMember() && SupporterManager.ConvertPartyMemberToSupporter)
                return;

            m_setupTurnBattleFighter(fighterPtr);
        }
    }
}
