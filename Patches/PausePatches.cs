using DragonEngineLibrary;
using MinHook;
using System.Runtime.InteropServices;
using System;
using System.Runtime.CompilerServices;

namespace LikeABrawler2
{
    internal class PausePatches : BrawlerPatch
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private unsafe delegate bool PauseManagerIsProhibit(IntPtr pauseManager);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private unsafe delegate IntPtr PauseManagerRequestPause1(IntPtr pauseManager, uint pauseID, ulong param, uint focus, uint slot);

        private IntPtr m_pauseProhibitFunc;
        private IntPtr m_pauseReqFunc;
        private IntPtr m_pauseReqFunc2;

        public override void Init()
        {
            base.Init();

            m_pauseReqFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 89 5C 24 10 48 89 6C 24 18 48 89 74 24 20 57 41 54 41 55 41 56 41 57 48 83 EC ? 45 0F B6 F9");
            m_pauseReqFunc2 = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 89 5C 24 10 48 89 6C 24 18 48 89 74 24 20 57 41 54 41 55 41 56 41 57 48 83 EC ? 45 89 CF 4D 89 C4");
            m_pauseProhibitFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 89 5C 24 08 48 89 74 24 18 57 48 83 EC ? 48 8B F9 41 B8 ? ? ? ?");
        }

        protected override void SetActive()
        {
            base.SetActive();

            if (_pauseReq1Trampoline == null)
            {
                _pauseProhibitTrampoline = BrawlerPatches.HookEngine.CreateHook<PauseManagerIsProhibit>(m_pauseProhibitFunc, PauseManager_IsProhibit);
                _pauseReq1Trampoline = BrawlerPatches.HookEngine.CreateHook<PauseManagerRequestPause1>(m_pauseReqFunc, PauseManager_RequestPause1);
                _pauseReq2Trampoline = BrawlerPatches.HookEngine.CreateHook<PauseManagerRequestPause1>(m_pauseReqFunc2, PauseManager_RequestPause2);
            }

            BrawlerPatches.HookEngine.EnableHook(_pauseProhibitTrampoline);
            BrawlerPatches.HookEngine.EnableHook(_pauseReq1Trampoline);
            BrawlerPatches.HookEngine.EnableHook(_pauseReq2Trampoline);
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            if (_pauseReq1Trampoline != null)
            {
                BrawlerPatches.HookEngine.DisableHook(_pauseProhibitTrampoline);
                BrawlerPatches.HookEngine.DisableHook(_pauseReq1Trampoline);
                BrawlerPatches.HookEngine.DisableHook(_pauseReq2Trampoline);
            }
        }


        private static PauseManagerIsProhibit _pauseProhibitTrampoline;
        private static bool PauseManager_IsProhibit(IntPtr mng)
        {
            if (Mod.IsTurnBased() || !BrawlerBattleManager.Battling || BrawlerBattleManager.CurrentPhase != BattleTurnManager.TurnPhase.Action || GameVarManager.GetValueBool(GameVarID.is_pause))
                return _pauseProhibitTrampoline(mng);

            return false;
        }

        private static PauseManagerRequestPause1 _pauseReq1Trampoline;
        private static PauseManagerRequestPause1 _pauseReq2Trampoline;
        private static IntPtr PauseManager_RequestPause1(IntPtr mng, uint pauseID, ulong param, uint focus, uint slot)
        {
           if (BrawlerBattleManager.Battling)
                if(pauseID == 89)
                    pauseID = 0x8;

            return _pauseReq1Trampoline(mng, pauseID, param, focus, slot);        
        }

        private static IntPtr PauseManager_RequestPause2(IntPtr mng, uint pauseID, ulong param, uint focus, uint slot)
        {
            return PauseManager_RequestPause1(mng, pauseID, param, focus, slot);
        }
    }
}
