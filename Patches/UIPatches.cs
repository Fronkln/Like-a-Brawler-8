using DragonEngineLibrary;
using System;


namespace LikeABrawler2
{
    internal unsafe class UIPatches : BrawlerPatch
    {
        private IntPtr m_createMinimapFunc;
        private IntPtr m_showDamageNumberFunc;

        private delegate IntPtr CreateMinimap(IntPtr obj, IntPtr val2, IntPtr val3);
        private delegate IntPtr ShowDamageNumber(IntPtr obj, IntPtr val2, uint val3, IntPtr fighterPtr, byte val4, IntPtr val5);

        public override void Init()
        {
            base.Init();

            m_createMinimapFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 89 4C 24 08 55 53 56 57 41 54 41 55 41 56 41 57 48 8D 6C 24 E1 48 81 EC ? ? ? ? 48 8B F9 41 8B 00 89 45 6F");
            m_showDamageNumberFunc = DragonEngineLibrary.Unsafe.CPP.ReadCall(DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 8B 84 24 E8 00 00 00 4C 8D 4C 24 30") + 87);
            SetActive();
        }


        protected override void SetActive()
        {
            base.SetActive();

            if (m_createMinimapTrampoline == null)
                m_createMinimapTrampoline = BrawlerPatches.HookEngine.CreateHook<CreateMinimap>(m_createMinimapFunc, CreateMinimapDetour);

            if (m_showDamageNumberTrampoline == null)
                m_showDamageNumberTrampoline = BrawlerPatches.HookEngine.CreateHook<ShowDamageNumber>(m_showDamageNumberFunc, Show_Damage_Number);

            BrawlerPatches.HookEngine.EnableHook(m_createMinimapTrampoline);

            if (!IniSettings.ShowEnemyDamage && IniSettings.IsPlayerRealtime())
            {
                BrawlerPatches.HookEngine.EnableHook(m_showDamageNumberTrampoline);
            }

        }

        protected override void SetInactive()
        {
            base.SetInactive();

            if (m_createMinimapTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_createMinimapTrampoline);

            if (m_showDamageNumberTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_showDamageNumberTrampoline);
        }

        private static CreateMinimap m_createMinimapTrampoline = null;
        private static IntPtr CreateMinimapDetour(IntPtr obj, IntPtr val2, IntPtr val3)
        {
            IntPtr result = m_createMinimapTrampoline.Invoke(obj, val2, val3);

            ulong* handle = (ulong*)(obj.ToInt64() + 192);
            BrawlerUIManager.Minimap = new UIHandleBase(*handle);

            return result;
        }

        private static ShowDamageNumber m_showDamageNumberTrampoline = null;
        private unsafe static IntPtr Show_Damage_Number(IntPtr obj, IntPtr val2, uint val3, IntPtr fighterPtr, byte val4, IntPtr val5)
        {
            return IntPtr.Zero;
        }
    }
}
