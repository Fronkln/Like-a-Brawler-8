using System;
using DragonEngineLibrary.Unsafe;

namespace LikeABrawler2
{
    internal class CameraPatches : BrawlerPatch
    {
        private IntPtr m_rpgCamFunc1;
        private IntPtr m_rpgCamFunc2;
        private IntPtr m_rpgCamFunc3;

        public override void Init()
        {
            base.Init();

            m_rpgCamFunc1 = CPP.PatternSearch("40 56 57 41 56 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 31 E0 48 89 84 24 80 00 00 00");
            m_rpgCamFunc2 = CPP.ReadCall(CPP.PatternSearch("E8 ? ? ? ? E9 ? ? ? ? 48 8B 8E E8 07 00 00"));

            //ccamera function only used by rpg camera
            m_rpgCamFunc3 = CPP.ReadCall(CPP.PatternSearch("E8 ? ? ? ? EB ? 33 D2 48 8B CF E8 ? ? ? ? 41 B1 ?"));
        }

        protected override void SetActive()
        {
            base.SetActive();

            CPP.PatchMemory(m_rpgCamFunc1, new byte[] { 0xC3, 0x90, 0x90 });
            CPP.PatchMemory(m_rpgCamFunc2, new byte[] { 0xC3, 0x90, 0x90 });
            CPP.PatchMemory(m_rpgCamFunc3, new byte[] { 0xC3, 0x90, 0x90, 0x90 });
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            CPP.PatchMemory(m_rpgCamFunc1, new byte[] { 0x40, 0x56, 0x57 });
            CPP.PatchMemory(m_rpgCamFunc2, new byte[] { 0x48, 0x8B, 0xC4 });
            CPP.PatchMemory(m_rpgCamFunc3, new byte[] { 0xC5, 0xF8, 0x10, 0x02 });
        }
    }
}
