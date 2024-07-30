using System;
using MinHook;
using DragonEngineLibrary;
using System.Runtime.InteropServices;

namespace LikeABrawler2
{
    internal unsafe class CFCPatches : BrawlerPatch
    {
        private IntPtr m_LeverConditionFunc;

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool ItemBuffCheck(IntPtr handler, IntPtr checkFighterResult, IntPtr fighter);

        public override void Init()
        {
            base.Init();

            m_LeverConditionFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("40 53 48 83 EC ? 49 8B C8 48 8B DA E8 ? ? ? ? 84 C0");
        }

        protected override void SetActive()
        {
            base.SetActive();

            m_leverTrampoline = BrawlerPatches.HookEngine.CreateHook<ItemBuffCheck>(m_LeverConditionFunc, Check_AnalogDeadzone);
            BrawlerPatches.HookEngine.EnableHook(m_leverTrampoline);
        }

        private static ItemBuffCheck m_leverTrampoline;
        private static bool Check_AnalogDeadzone(IntPtr handler, IntPtr checkFighterResult, IntPtr fighter)
        {
            byte* paramsPtr = *(byte**)(checkFighterResult.ToInt64() + 0x8);
            byte level = *paramsPtr;
            byte op = *(paramsPtr + 1);

            uint fLevel = Player.GetLevel(BrawlerPlayer.CurrentPlayer);

            switch(op)
            {
                default:
                    return true;
                case 0:
                    return fLevel == level;
                case 1:
                    return fLevel >= level;
                case 2:
                    return fLevel <= level;
            }

            return m_leverTrampoline(handler, checkFighterResult, fighter);
        }
    }
}
