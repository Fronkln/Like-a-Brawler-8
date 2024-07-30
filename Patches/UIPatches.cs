using DragonEngineLibrary;
using System;


namespace LikeABrawler2
{
    internal unsafe class UIPatches : BrawlerPatch
    {
        private IntPtr m_createMinimapFunc;

        private delegate IntPtr CreateMinimap(IntPtr obj, IntPtr val2, IntPtr val3);

        public override void Init()
        {
            base.Init();

            m_createMinimapFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 89 4C 24 08 55 53 56 57 41 54 41 55 41 56 41 57 48 8D 6C 24 E1 48 81 EC ? ? ? ? 48 8B F9 41 8B 00 89 45 6F");
            SetActive();
        }


        protected override void SetActive()
        {
            base.SetActive();

            if (m_createMinimapTrampoline == null)
                m_createMinimapTrampoline = BrawlerPatches.HookEngine.CreateHook<CreateMinimap>(m_createMinimapFunc, CreateMinimapDetour);

            BrawlerPatches.HookEngine.EnableHook(m_createMinimapTrampoline);

        }

        private static CreateMinimap m_createMinimapTrampoline = null;
        private static IntPtr CreateMinimapDetour(IntPtr obj, IntPtr val2, IntPtr val3)
        {
            IntPtr result = m_createMinimapTrampoline.Invoke(obj, val2, val3);

            ulong* handle = (ulong*)(obj.ToInt64() + 192);
            BrawlerUIManager.Minimap = new UIHandleBase(*handle);

            return result;
        }
    }
}
