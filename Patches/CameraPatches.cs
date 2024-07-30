using System;
using DragonEngineLibrary.Unsafe;

namespace LikeABrawler2
{
    internal class CameraPatches : BrawlerPatch
    {
        private IntPtr m_rpgCamFunc1;

        public override void Init()
        {
            base.Init();

            m_rpgCamFunc1 = CPP.PatternSearch("40 56 57 41 56 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 31 E0 48 89 84 24 80 00 00 00");
        }

        protected override void SetActive()
        {
            base.SetActive();

            CPP.PatchMemory(m_rpgCamFunc1, new byte[] { 0xC3, 0x90, 0x90 });
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            CPP.PatchMemory(m_rpgCamFunc1, new byte[] { 0x40, 0x56, 0x57 });
        }
    }
}
