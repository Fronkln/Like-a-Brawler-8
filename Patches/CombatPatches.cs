using System;
using System.Net.Security;
using System.Runtime.InteropServices;
using DragonEngineLibrary;
using DragonEngineLibrary.Unsafe;
using MinHook;

namespace LikeABrawler2
{
    internal unsafe class CombatPatches : BrawlerPatch
    {
        private IntPtr m_inputStateAssignment;
        private IntPtr m_fighterTransformFunc;
        private IntPtr m_fighterTransformEffectFunc;
        private IntPtr m_canDamSyncJump;
        private IntPtr m_canDamSyncFunc;
        private IntPtr m_setMarkFighterFunc;
        private IntPtr m_autoModeFunc;
        private IntPtr m_invisFighterJmp1;
        private IntPtr m_invisFighterJmp2;
        private IntPtr m_invisFighterJmp3;
        private IntPtr m_yorokeFunc;
        private IntPtr m_ecbattleStatusCalculateHpFunc;
        private IntPtr m_fighterDisableRunFunc;
        private IntPtr m_liveActionNodeFunc;
        private IntPtr m_someFighterFuncAddr;

        private IntPtr m_transitKiryuGuardFunc;
        private HijackedFunction m_transitKiryuGuardHijack;
        private NopPatch m_transitOrigFuncNopPatch;

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool FighterManagerRequestTransform(IntPtr fman, uint* character);


        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool FighterCanDamSync(IntPtr fighter, IntPtr armorInf);

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool FighterDisableRun(IntPtr fighter);

        private delegate void TransitHijackedKiryuGuard();

        private delegate IntPtr FighterManagerPlayTransformEffect(IntPtr fighter);

        private delegate void ECBattleStatusSetMarkFighter(IntPtr status, uint* characterUID);

        private delegate bool TurnCommandDecideManagerHandleAutoMode(IntPtr thisPtr, IntPtr selectCommandInfo, long** fighterPtrPtr);


        private delegate void LiveActionNodePlayFirst(IntPtr thisPtr);

        private delegate void ECBattleStatusInitializeHP(IntPtr status);


        private delegate void SomeFighterFunctionDel(IntPtr thisPtr, long someValue);


        public override void Init()
        {
            base.Init();

            m_inputStateAssignment = CPP.PatternSearch("83 F8 ? 73 ? 8B 4C 87 20") + 9;
            m_fighterTransformFunc = CPP.PatternSearch("48 89 5C 24 10 48 89 74 24 18 57 48 83 EC ? 48 89 D6 48 89 CF 48 8D 99 F0 BF 00 00");
            m_fighterTransformEffectFunc = CPP.PatternSearch("40 53 48 83 EC ? 8B 81 58 05 00 00");
            m_fighterDisableRunFunc = CPP.ReadCall(CPP.PatternSearch("E8 ? ? ? ? 84 C0 45 0F B6 F6 44 0F 45 F3"));
            m_canDamSyncJump = CPP.PatternSearch("75 ? B0 ? 48 8B 5C 24 30 48 83 C4 ? 5F C3 48 8B 5C 24 30 32 C0 48 83 C4 ? 5F C3");
            m_canDamSyncFunc = CPP.ReadCall(CPP.PatternSearch("E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 48 8B CF E8 ? ? ? ? 48 8B 80 B8 0C 00 00"));
            m_setMarkFighterFunc = CPP.ReadCall(CPP.PatternSearch("E8 ? ? ? ? 48 8B 0D ? ? ? ? 48 8B 01 45 33 C0"));
           // m_autoModeValue = DragonEngineLibrary.Unsafe.CPP.PatternSearch("41 8B 40 08 49 8B F8 48 8B DA");
            m_autoModeFunc = CPP.ReadCall(CPP.PatternSearch("E8 ? ? ? ? 48 8B 0F 84 C0 75 ?"));
            m_invisFighterJmp1 = CPP.PatternSearch("F6 87 68 01 00 00 ? 0F 85 ? ? ? ?");
            m_invisFighterJmp2 = CPP.PatternSearch("74 ? C5 78 2F D1 76 ? C5 FA 10 05 ? ? ? ?");
            m_invisFighterJmp3 = CPP.PatternSearch("76 ? C5 78 2F 47 10");
            m_yorokeFunc = CPP.ReadCall(CPP.PatternSearch("E8 ? ? ? ? 48 8B 8E E8 07 00 00 8B 59 08"));
            m_battleTransformOnTrampoline = BrawlerPatches.HookEngine.CreateHook<FighterManagerPlayTransformEffect>(m_fighterTransformEffectFunc, ECRender_BattleTransformOn);
            m_ecbattleStatusCalculateHpFunc = CPP.PatternSearch("48 89 4C 24 08 55 53 56 57 41 54 41 55 41 57");
            m_liveActionNodeFunc = CPP.PatternSearch("48 89 5C 24 18 56 48 83 EC ? 8B 51 3C");
            m_liveActionPlayFunc = BrawlerPatches.HookEngine.CreateHook<LiveActionNodePlayFirst>(CPP.PatternSearch("48 89 5C 24 18 56 48 83 EC ? 8B 51 3C"), LiveActionPlayFirst);
            //m_liveActionPlayFunc2 = BrawlerPatches.HookEngine.CreateHook<LiveActionNodePlayFirst>(CPP.PatternSearch("40 56 57 48 83 EC ? 48 8B F1 8B FA"), LiveActionPlay);

            m_transitKiryuGuardFunc = CPP.PatternSearch("48 8B C4 48 89 58 10 48 89 70 18 48 89 78 20 55 41 54 41 55 41 56 41 57 48 8D A8 18 FF FF FF 48 81 EC ? ? ? ? C5 F8 29 70 C8 C5 F8 29 78 B8 C5 78 29 40 A8 C5 78 29 48 98 48 8B F1");
            
            IntPtr origCall = CPP.PatternSearch("E8 ? ? ? ? 48 8B D3 48 8B C8 E8 ? ? ? ? 48 8B 06");
            m_transitOrigFuncNopPatch = new NopPatch(origCall+5);
            m_transitKiryuGuardHijack = new HijackedFunction(origCall, m_transitKiryuGuardFunc);

            if (m_ecbattleStatusInitializeHpTrampoline == null)
            {
                m_ecbattleStatusInitializeHpTrampoline = BrawlerPatches.HookEngine.CreateHook<ECBattleStatusInitializeHP>(m_ecbattleStatusCalculateHpFunc, ECBattleStatus_InitializeHP);
                BrawlerPatches.HookEngine.EnableHook(m_ecbattleStatusInitializeHpTrampoline);
            }

            m_someFighterFuncAddr = CPP.PatternSearch("40 53 48 83 EC ? 48 8B D9 48 8B CA E8 ? ? ? ? 84 C0");
        }

        protected override void SetActive()
        {
            base.SetActive();

            if (m_battleTransformOnTrampoline == null)
                m_battleTransformOnTrampoline = BrawlerPatches.HookEngine.CreateHook<FighterManagerPlayTransformEffect>(m_fighterTransformEffectFunc, ECRender_BattleTransformOn);

            if (m_canDamSyncTrampoline == null)
                m_canDamSyncTrampoline = BrawlerPatches.HookEngine.CreateHook<FighterCanDamSync>(m_canDamSyncFunc, Fighter_CanDamSync);

            if (m_ecBattleStatusSetMarkFighterTrampoline == null)
                m_ecBattleStatusSetMarkFighterTrampoline = BrawlerPatches.HookEngine.CreateHook<ECBattleStatusSetMarkFighter>(m_setMarkFighterFunc, ECBattleStatus_SetMarkFighter);

            if (m_handleAutoModeTrampoline == null)
                m_handleAutoModeTrampoline = BrawlerPatches.HookEngine.CreateHook<TurnCommandDecideManagerHandleAutoMode>(m_autoModeFunc, TurnCommandDecideManager_HandleAutoMode);

            if(m_transitKiryuCounterTrampoline == null)
                m_transitKiryuCounterTrampoline = BrawlerPatches.HookEngine.CreateHook<TransitHijackedKiryuGuard>(m_transitKiryuGuardFunc, TransitKiryuCounter);

            if (m_fighterDisableRunTrampoline == null)
                m_fighterDisableRunTrampoline = BrawlerPatches.HookEngine.CreateHook<FighterDisableRun>(m_fighterDisableRunFunc, Fighter_DisableRun);

            if(m_someFighterFunc == null)
                m_someFighterFunc =  BrawlerPatches.HookEngine.CreateHook<SomeFighterFunctionDel>(m_someFighterFuncAddr, SomeFighterFunction);

            BrawlerPatches.HookEngine.EnableHook(m_battleTransformOnTrampoline);
            BrawlerPatches.HookEngine.EnableHook(m_canDamSyncTrampoline);
            BrawlerPatches.HookEngine.EnableHook(m_ecBattleStatusSetMarkFighterTrampoline);
            BrawlerPatches.HookEngine.EnableHook(m_handleAutoModeTrampoline);
            BrawlerPatches.HookEngine.EnableHook(m_transitKiryuCounterTrampoline);
            BrawlerPatches.HookEngine.EnableHook(m_fighterDisableRunTrampoline);
            BrawlerPatches.HookEngine.EnableHook(m_liveActionPlayFunc);
            BrawlerPatches.HookEngine.EnableHook(m_someFighterFunc);
            //BrawlerPatches.HookEngine.EnableHook(m_liveActionPlayFunc2);

            CPP.PatchMemory(m_invisFighterJmp1+6, 0x99);
            CPP.PatchMemory(m_invisFighterJmp2, 0xEB);
            CPP.PatchMemory(m_invisFighterJmp3, 0xEB);

            //Yoroke Procedure: Disable (Dont push nearby enemies around on my turn start if i am not targeting them.)
            CPP.PatchMemory(m_yorokeFunc, 0xC3, 0x90, 0x90);

            //COMBAT: Force AutoBattle mode on 3 for party members.
            //DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_autoModeValue, new byte[] { 0xB8, 0x03, 0x0, 0x0, 0x0 });

            //COMBAT (Kiryu): Change the way we transit Kiryu counter by hijacking to function
            m_transitKiryuGuardHijack.Enable();
            m_transitOrigFuncNopPatch.Enable(0x94);
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            if (m_battleTransformOnTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_battleTransformOnTrampoline);

            if (m_ecBattleStatusSetMarkFighterTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_ecBattleStatusSetMarkFighterTrampoline);

            if (m_handleAutoModeTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_handleAutoModeTrampoline);

            if (m_fighterDisableRunTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_fighterDisableRunTrampoline);

            if(m_liveActionPlayFunc != null)
                BrawlerPatches.HookEngine.DisableHook(m_liveActionPlayFunc);

            if (m_liveActionPlayFunc2 != null)
                BrawlerPatches.HookEngine.DisableHook(m_liveActionPlayFunc2);

            if (m_someFighterFunc != null)
                BrawlerPatches.HookEngine.DisableHook(m_someFighterFunc);

            CPP.PatchMemory(m_invisFighterJmp1+6, 0x20);
            CPP.PatchMemory(m_invisFighterJmp2, 0x74);
            CPP.PatchMemory(m_invisFighterJmp3, 0x77);

            CPP.PatchMemory(m_yorokeFunc, 0x48, 0x8B, 0xC4);

            //COMBAT: Force AutoBattle mode on 3 for party members.
            //DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_autoModeValue, new byte[] { 0x41, 0x8B, 0x40, 0x8, 0X49 });
            EnableAssignment();

            if (m_transitKiryuGuardHijack != null)
            {
                BrawlerPatches.HookEngine.DisableHook(m_transitKiryuCounterTrampoline);
                m_transitKiryuGuardHijack.Disable();
                m_transitOrigFuncNopPatch.Disable();
            }
        }

        public void OnCombatStart()
        {
            DisableAssignment();
        }

        public void OnCombatEnd()
        {
            EnableAssignment();
        }


        public void EnableAssignment()
        {
            CPP.PatchMemory(m_inputStateAssignment, new byte[] { 0x89, 0x0F });
        }

        public void DisableAssignment()
        {
            CPP.NopMemory(m_inputStateAssignment, 2);
        }


        private static ECBattleStatusInitializeHP m_ecbattleStatusInitializeHpTrampoline;

        private unsafe static void ECBattleStatus_InitializeHP(IntPtr status)
        {
            m_ecbattleStatusInitializeHpTrampoline(status);

            if (!Mod.IsRealtime())
                return;

            ECBattleStatus statusobj = new ECBattleStatus() { Pointer = status };
            Character owner = statusobj.Owner;

            long val = statusobj.CurrentHP;
            uint uid = statusobj.Owner.UID;

            IntPtr addr = owner.Pointer;

            Fighter fighter = new Fighter((IntPtr)(&addr));

            if (!owner.IsPartyMember() && owner.UID != BrawlerBattleManager.PlayerCharacter.UID)
            {
                //Dont rebalance for higher difficulties
                if (NativeFuncs.BattleDifficultyFunc() <= 1)
                {

                    uint soldierID = (uint)owner.Attributes.soldier_data_id;
                    EnemyRebalanceEntry rebalancedDat = DBManager.GetSoldierRebalance(soldierID);

                    if (rebalancedDat == null)
                        return;

                    statusobj.SetHPMax(rebalancedDat.Health);
                    statusobj.SetHPCurrent(rebalancedDat.Health);
                    statusobj.AttackPower = rebalancedDat.Attack;
                    statusobj.DefensePower = rebalancedDat.Defense;
                }
            }
        }

        //Originally cbattle_manager constructor, it only gets called once on game start, so we can easily make this function our own!
        private static TransitHijackedKiryuGuard m_transitKiryuCounterTrampoline;
        private static void TransitKiryuCounter()
        {
            if (BrawlerPlayer.CurrentStyle != PlayerStyle.Default && BrawlerPlayer.CurrentStyle != PlayerStyle.Dragon)
                return;

            if (BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.IsValid())
                return;

            BattleTurnManager.ForceCounterCommand(BrawlerBattleManager.PlayerFighter, BrawlerBattleManager.AllEnemiesNearest[0], (RPGSkillID)1386);
        }


        private static FighterManagerPlayTransformEffect m_battleTransformOnTrampoline = null;
        private static unsafe IntPtr ECRender_BattleTransformOn(IntPtr rendererPtr)
        {
            if (Mod.IsTurnBased())
                return m_battleTransformOnTrampoline(rendererPtr);

            ECRenderCharacter charaRender = new ECRenderCharacter() { Pointer = rendererPtr };

            Character ownerCharacter = new Character() { Pointer = charaRender.Owner.Pointer };

            bool isPlayer = ownerCharacter.UID == BrawlerBattleManager.PlayerCharacter.UID;
            bool isParty = ownerCharacter.IsPartyMember();

            if (isPlayer || isParty)
            {
                if (ownerCharacter.UID == BrawlerBattleManager.PlayerCharacter.UID)
                {
                    if (!BrawlerBattleManager.AllowPlayerTransformThisFight && !BrawlerPlayer.IsOtherPlayer())
                        return IntPtr.Zero;
                }
                else
                {
                    if (ownerCharacter.IsPartyMember())
                        if (!BrawlerBattleManager.AllowAllyTransformThisFight)
                            return IntPtr.Zero;
                }

                if (BrawlerBattleManager.CurrentPhase != BattleTurnManager.TurnPhase.Action)
                    return IntPtr.Zero;
            }

            return m_battleTransformOnTrampoline(rendererPtr);
        }

        private static FighterCanDamSync m_canDamSyncTrampoline = null;
        private static bool Fighter_CanDamSync(IntPtr fighterPtr, IntPtr superArmorInfoPtr)
        {
            Fighter figher = new Fighter(fighterPtr);

            if (MortalReversalManager.CounterFlag)
                return true;

            return m_canDamSyncTrampoline.Invoke(fighterPtr, superArmorInfoPtr);
        }

        private static ECBattleStatusSetMarkFighter m_ecBattleStatusSetMarkFighterTrampoline = null;
        private static void ECBattleStatus_SetMarkFighter(IntPtr statusPtr, uint* characterHandle)
        {
            ECBattleStatus status = new ECBattleStatus() { Pointer = statusPtr };
            BaseEnemyAI ai = EnemyManager.GetAI(status.Owner.UID);

            if (ai != null)
            {
                Character targetCharacter = new EntityHandle<Character>(*characterHandle);

                //Force AI to target kasuga instead if they are targeting a fellow party member!
                //...unless we actually want them to do this! Like in the second Yamai fight who wants to attack Kiryu!
                //...because if he doesnt, he only taunts kasuga
                //*characterHandle = ai.OverrideMarkTarget(new EntityHandle<Character>(*characterHandle)).UID;

                *characterHandle = BrawlerBattleManager.PlayerCharacter.UID;
            }

            m_ecBattleStatusSetMarkFighterTrampoline(statusPtr, characterHandle);
        }

        private static TurnCommandDecideManagerHandleAutoMode m_handleAutoModeTrampoline = null;
        private static bool TurnCommandDecideManager_HandleAutoMode(IntPtr thisPtr, IntPtr selectCommandInfo, long** fighterPtrPtr)
        {
            Fighter fighter = new Fighter((IntPtr)(*fighterPtrPtr));

            if(fighter.IsAnyPartyMember())
            {
                //TODO MAYBE: Let party member choose their automode
                if (!fighter.IsMainPlayer())
                {
                    int* autoMode = (int*)fighterPtrPtr + 2;
                    *autoMode = SupporterPartyMember.DecideAutoModeStrategy(fighter); //1;
                }
            }

            return m_handleAutoModeTrampoline(thisPtr, selectCommandInfo, fighterPtrPtr);

        }

        private static FighterDisableRun m_fighterDisableRunTrampoline = null;
        private static bool Fighter_DisableRun(IntPtr fighterPtr)
        {
            Fighter fighter = new Fighter(fighterPtr);

            if (fighter != BrawlerBattleManager.PlayerFighter)
                return m_fighterDisableRunTrampoline(fighterPtr);

            //Let EX Heat Ichiban, Beast and OEDOD to run with weapons
            if (BrawlerPlayer.CurrentJob == RPGJobID.kasuga_freeter && BrawlerPlayer.IsExtremeHeat)
                return false;

            if (BrawlerPlayer.CurrentStyle == PlayerStyle.Beast || BrawlerPlayer.CurrentStyle == PlayerStyle.Dragon)
                return false;

            return m_fighterDisableRunTrampoline(fighterPtr);
        }

        LiveActionNodePlayFirst m_liveActionPlayFunc;
        private unsafe void LiveActionPlayFirst(IntPtr thisPtr)
        {
            AuthNode node = new AuthNode() { Pointer = thisPtr };
            Character character = node.GetSelf().Get().GetCharacter();

            if (character != BrawlerBattleManager.PlayerCharacter)
                return;

            m_liveActionPlayFunc(thisPtr);
        }

        LiveActionNodePlayFirst m_liveActionPlayFunc2;
        private unsafe void LiveActionPlay(IntPtr thisPtr)
        {
            AuthNode node = new AuthNode() { Pointer = thisPtr };
            Character character = node.GetSelf().Get().GetCharacter();

            if (character != BrawlerBattleManager.PlayerCharacter)
                return;

            m_liveActionPlayFunc2(thisPtr);
        }

        SomeFighterFunctionDel m_someFighterFunc;
        private unsafe void SomeFighterFunction(IntPtr thisPtr, long someValue)
        {
            if (thisPtr == IntPtr.Zero || someValue == 0)
                return;

            m_someFighterFunc.Invoke(thisPtr, someValue);
        }
    }
}
