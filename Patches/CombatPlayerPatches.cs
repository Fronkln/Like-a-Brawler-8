using System;
using System.Runtime.InteropServices;
using DragonEngineLibrary;
using MinHook;

namespace LikeABrawler2
{
    internal unsafe class CombatPlayerPatches : BrawlerPatch
    {
        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool HumanModeManagerIsInputSway(IntPtr humanModeManager);

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool HumanModeManagerIsInputGuard(IntPtr humanModeManager);

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool HumanModeManagerIsInputPickup(IntPtr humanModeManager);

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool HumanModeManagerIsInputKamae(IntPtr humanModeManager);

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool CalcDamageGuardReduceIsValidEvent(IntPtr calcArgs, long** fighterPtrPtr);

        private delegate long TrustParamManagerGetTrustLevel(Player.ID playerID);


        private IntPtr m_inputIsGuardFunc;
        private IntPtr m_inputIsPickupFunc;
        private IntPtr m_inputIsKamaeFunc;

        private IntPtr m_swayRestorationAddr;
        private IntPtr m_freeMovementAddr;

        private IntPtr m_guardProcedure1;
        private IntPtr m_guardProcedure2;
        private IntPtr m_guardSModeFunc;
        private IntPtr m_guardOrigFunc;
        private IntPtr m_guardProcedure3;

        private IntPtr m_canDamSyncJump;

        private IntPtr m_kizunaFollowupFunc;

        public override void Init()
        {
            m_inputIsGuardFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 83 EC ? 48 8B 81 C8 03 00 00 80 B8 A3 11 00 00 ?");
            m_inputIsPickupFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 83 EC ? 48 83 C1 ? E8 ? ? ? ? 0F B6 80 B4 00 00 00");
            m_inputIsKamaeFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 89 5C 24 08 57 48 83 EC ? 48 8B 81 C8 03 00 00 48 89 CB");

            m_swayRestorationAddr = DragonEngineLibrary.Unsafe.CPP.PatternSearch("80 B8 96 05 00 00 ? 74 ? 48 8B 03");
            m_freeMovementAddr = DragonEngineLibrary.Unsafe.CPP.PatternSearch("88 98 A3 11 00 00");

            m_guardProcedure1 = DragonEngineLibrary.Unsafe.CPP.PatternSearch("84 C0 0F 84 ? ? ? ? E8 ? ? ? ? 44 38 B0 74 21 00 00");
            m_guardProcedure2 = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 89 5C 24 08 57 48 83 EC ? 48 8B 4A 08");
            m_guardSModeFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 8B 01 48 8B 80 10 4E 00 00 48 8B 40 10");
            m_guardProcedure3 = DragonEngineLibrary.Unsafe.CPP.PatternSearch("E8 ? ? ? ? 48 8B 88 00 21 00 00 48 85 C9 74 ? 83 B9 C8 00 00 00 ?");

            m_guardOrigFunc = DragonEngineLibrary.Unsafe.CPP.ReadCall(m_guardProcedure3);

            m_canDamSyncJump = DragonEngineLibrary.Unsafe.CPP.PatternSearch("75 ? B0 ? 48 8B 5C 24 30 48 83 C4 ? 5F C3 48 8B 5C 24 30 32 C0 48 83 C4 ? 5F C3");

            m_kizunaFollowupFunc = DragonEngineLibrary.Unsafe.CPP.ReadCall(Mod.FindPatternAssert("E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 48 85 FF 0F 84 ? ? ? ? 48 8D 4D 97"));
        }


        //TODO CRUCIAL: CACHE ADDRESSES IN INITIALIZE AND ADD INACTVIE
        protected unsafe override void SetActive()
        {
            if (m_isInputGuardTrampoline == null)
            {
                m_isInputGuardTrampoline = BrawlerPatches.HookEngine.CreateHook<HumanModeManagerIsInputGuard>(m_inputIsGuardFunc, HumanModeManager_IsInputGuard);
                m_isInputPickupTrampoline = BrawlerPatches.HookEngine.CreateHook<HumanModeManagerIsInputPickup>(m_inputIsPickupFunc, HumanModeManager_IsInputPickup);
                m_inputIsKamaeTrampoline = BrawlerPatches.HookEngine.CreateHook<HumanModeManagerIsInputKamae>(m_inputIsKamaeFunc, HumanModeManager_IsInputKamae);
                m_calcDamageGuardValidEventTrampoline = BrawlerPatches.HookEngine.CreateHook<CalcDamageGuardReduceIsValidEvent>(m_guardProcedure2, CalcDamageGuardReduce_ValidEvent);
            }

            BrawlerPatches.HookEngine.EnableHook(m_isInputGuardTrampoline);
            BrawlerPatches.HookEngine.EnableHook(m_isInputPickupTrampoline);
            BrawlerPatches.HookEngine.EnableHook(m_inputIsKamaeTrampoline);
            BrawlerPatches.HookEngine.EnableHook(m_calcDamageGuardValidEventTrampoline);

            //COMBAT: Prevent the game from blocking us from swaying (you can't tell me what to do!)
            DragonEngineLibrary.Unsafe.CPP.NopMemory(m_swayRestorationAddr, 7);

            //COMBAT: Prevent game from automatically setting "free movement mode" to false.
            DragonEngineLibrary.Unsafe.CPP.NopMemory(m_freeMovementAddr, 6);

            //TODO: MOVE COMBAT GUARDING PATCHES TO COMBATPATCHES AND MAKE THAT INHERIT FROM PATCH
            //COMBAT GUARDING: Guard Procedure - Remove Is Enemy Check
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_guardProcedure1, new byte[] { 0x90, 0x90, 0xE9, 0xA5, 0x0, 0x0, 0x0, 0x90 });

            //COMBAT GUARDING: GuardReduce Valid Event - True (Allows us to guard)
            //DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_guardProcedure2, new byte[] { 0xB8, 0x01, 0x0, 0x0, 0x0, 0xC3 });

            //COMBAT GUARDING: Check if we are guarding based on humanmode
            //call Fighter::sMode
            DragonEngineLibrary.Unsafe.CPP.WriteCall(m_guardProcedure3, m_guardSModeFunc); 
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_guardProcedure3 + 5, 
                new byte[] 
                { 
                    0x48, 0x8B, 0xC8,  //mov rcx, rax
                    0x48, 0x8B, 0x10,  //mov rdx, [rax] 
                    0xFF, 0x92, 0x78, 0x01, 0x0, 0x0, //call qword ptr [rdx+0x178] (HumanMode::IsGuard)
                    0x3C, 0x01, // cmp al, 01
                    0x90, 0x90, 0x90, 0x90, 0x90 //nop (x5)
                });

            DragonEngineLibrary.Unsafe.CPP.NopMemory(m_canDamSyncJump, 2);

            //COMBAT: Disable party member followups which may break brawler!
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_kizunaFollowupFunc, new byte[] { 0x31, 0xC0, 0xC3 });
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            if(m_isInputGuardTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_isInputGuardTrampoline);

            if (m_isInputPickupTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_isInputPickupTrampoline);

            if (m_inputIsKamaeTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_inputIsKamaeTrampoline);

            if (m_calcDamageGuardValidEventTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_calcDamageGuardValidEventTrampoline);

            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_swayRestorationAddr, new byte[] { 0x80, 0xB8, 0x96, 0x05, 0x0, 0x0, 0x0 });
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_freeMovementAddr, new byte[] { 0x88, 0x98, 0xA3, 0x11, 0x0, 0x0 });

            DragonEngineLibrary.Unsafe.CPP.WriteCall(m_guardProcedure3, m_guardOrigFunc);
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_guardProcedure3 + 5,
                new byte[]
                {
                    0x48, 0x8B, 0x88, 0x0, 0x21, 0x0, 0x0,
                    0x48, 0x85, 0xC9,
                    0x74, 0x13,
                    0x83, 0xB9, 0xC8, 0x0, 0x0, 0x0, 0x03
                });

            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_canDamSyncJump, new byte[] { 0x75, 0xD });
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_kizunaFollowupFunc, new byte[] { 0x48, 0x8B, 0xC4 });
        }


        private static HumanModeManagerIsInputSway m_isInputSwayTrampoline = null;
        public static bool HumanModeManager_IsInputSway(IntPtr humanModeMngPtr)
        {
            if (BrawlerBattleManager.CurrentPhase != BattleTurnManager.TurnPhase.Action || BrawlerBattleManager.PlayerCharacter == null)
                return m_isInputSwayTrampoline(humanModeMngPtr);

            HumanModeManager manager = new HumanModeManager() { Pointer = humanModeMngPtr };

            if (manager.Human.Pointer != BrawlerBattleManager.PlayerCharacter.Pointer)
                return m_isInputSwayTrampoline(humanModeMngPtr);
            else
                return BrawlerPlayer.IsInputSway(manager);
        }

        private static HumanModeManagerIsInputGuard m_isInputGuardTrampoline = null;
        public static bool HumanModeManager_IsInputGuard(IntPtr humanModeMngPtr)
        {
            if(BrawlerBattleManager.CurrentPhase != BattleTurnManager.TurnPhase.Action || BrawlerBattleManager.PlayerCharacter == null)
                return m_isInputGuardTrampoline(humanModeMngPtr);

            HumanModeManager manager = new HumanModeManager() { Pointer = humanModeMngPtr };

            if (manager.Human.Pointer != BrawlerBattleManager.PlayerCharacter.Pointer)
            {
                BaseAI ai = manager.Human.TryGetAI();
                if(ai == null)
                    return m_isInputGuardTrampoline(humanModeMngPtr);

                bool res = ai.ShouldGuard();

                if (res)
                    return true;
                else
                    return m_isInputGuardTrampoline(humanModeMngPtr);
            }
            else
                return BrawlerPlayer.IsInputGuard(manager);
        }

        private static HumanModeManagerIsInputKamae m_inputIsKamaeTrampoline = null;
        public static bool HumanModeManager_IsInputKamae(IntPtr humanModeMngPtr)
        {
            return m_inputIsKamaeTrampoline(humanModeMngPtr);
        }

        private static HumanModeManagerIsInputPickup m_isInputPickupTrampoline = null;
        public static bool HumanModeManager_IsInputPickup(IntPtr humanModeMngPtr)
        {
            if (BrawlerBattleManager.CurrentPhase != BattleTurnManager.TurnPhase.Action || BrawlerBattleManager.PlayerCharacter == null)
                return m_isInputPickupTrampoline(humanModeMngPtr);

            return true;

            HumanModeManager manager = new HumanModeManager() { Pointer = humanModeMngPtr };

            if (manager.Human.Pointer != BrawlerBattleManager.PlayerCharacter.Pointer)
                return m_isInputGuardTrampoline(humanModeMngPtr);
            else
                return BrawlerPlayer.IsInputGuard(manager);
        }

        private static CalcDamageGuardReduceIsValidEvent m_calcDamageGuardValidEventTrampoline = null;
        public static bool CalcDamageGuardReduce_ValidEvent(IntPtr calcDmg, long** fighterPtrPtr)
        {
            if (fighterPtrPtr == null)
                return m_calcDamageGuardValidEventTrampoline(calcDmg, fighterPtrPtr);

            long* fighterAddr = fighterPtrPtr[1];

            if (fighterAddr == null)
                return m_calcDamageGuardValidEventTrampoline(calcDmg, fighterPtrPtr);

            Fighter fighter = new Fighter((IntPtr)(fighterAddr));

            if (fighter == BrawlerBattleManager.PlayerFighter)
                return true;

            BaseEnemyAI ai = EnemyManager.GetAI(fighter);

            if(ai == null)
                return m_calcDamageGuardValidEventTrampoline(calcDmg, fighterPtrPtr);

            if (ai.IsMortalAttackOrPreparing())
                return false;


            return true;
        }
    }
}
