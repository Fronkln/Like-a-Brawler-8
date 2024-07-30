using System;
using System.Runtime.InteropServices;


namespace LikeABrawler2
{
    internal class HActPatches : BrawlerPatch
    {
        private IntPtr m_patchAddr1;
        private IntPtr m_patchAddr2;
        private IntPtr m_patchAddr3;
        private IntPtr m_patchAddr4;

        private IntPtr m_requestHactFunc;

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool HactManagerRequestHact(IntPtr hman, IntPtr inf);

        public override void Init()
        {
            base.Init();

            m_patchAddr1 = DragonEngineLibrary.Unsafe.CPP.PatternSearch("0F 84 ? ? ? ? C5 E9 EF D2 C5 FA 11 54 24 30 C4 E3 79 17 54 24 34 ? C4 E3 79 17 54 24 38 ? C4 E3 79 17 54 24 3C ? B8 ? ? ? ?");
            m_patchAddr2 = m_patchAddr1 + 0x7C;
            m_patchAddr3 = DragonEngineLibrary.Unsafe.CPP.PatternSearch("0F 84 ? ? ? ? 8D 46 FF 48 8D 04 40 48 C1 E0 ? 48 8D 9F 60 02 00 00");
            m_patchAddr4 = m_patchAddr3 - 7;

            m_requestHactFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 89 5C 24 08 57 48 83 EC ? 48 8B D9 48 8B FA 48 8B 0D ? ? ? ? E8 ? ? ? ? 84 C0 75 ?");
        }

        protected override void SetActive()
        {
            //COMBAT (RANGE): Disable filtering for cec_hact
            DragonEngineLibrary.Unsafe.CPP.NopMemory(m_patchAddr1, 6);
            DragonEngineLibrary.Unsafe.CPP.NopMemory(m_patchAddr2, 6);
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_patchAddr3, new byte[] { 0xE9, 0xA8, 0x0, 0x0, 0x0, 0x90 });
            DragonEngineLibrary.Unsafe.CPP.NopMemory(m_patchAddr4, 7);

            if (m_requestTrampoline == null)
                m_requestTrampoline = BrawlerPatches.HookEngine.CreateHook<HactManagerRequestHact>(m_requestHactFunc, HActManager_RequestHAct);

            BrawlerPatches.HookEngine.EnableHook(m_requestTrampoline);
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_patchAddr1, new byte[] {0x0F, 0x84, 0x89, 0x0, 0x0, 0x0});
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_patchAddr2, new byte[] { 0x0F, 0x84, 0x13, 0x1, 0x0, 0x0 });
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_patchAddr3, new byte[] { 0x0F, 0x84, 0x13, 0x1, 0x0, 0x0 });
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_patchAddr4, new byte[] { 0x48, 0x8B, 0x05, 0x32, 0xE7, 0xAF, 0x2 });

            if (m_requestTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_requestTrampoline);
        }

        HactManagerRequestHact m_requestTrampoline;
        private bool HActManager_RequestHAct(IntPtr mng, IntPtr inf)
        {
            bool result = m_requestTrampoline(mng, inf);

            if (result)
                HeatActionManager.OnRequestHAct();

            return result;
        }
    }
}
