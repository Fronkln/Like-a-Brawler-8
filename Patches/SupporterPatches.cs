using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal class SupporterPatches : BrawlerPatch
    {
        private IntPtr m_createSupporterUIFunc;

        public override void Init()
        {
            base.Init();

            m_createSupporterUIFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("40 56 57 41 55 41 56 48 83 EC ? 48 8B 81 70 12 00 00");
        }

        protected override void SetActive()
        {
            base.SetActive();

            //Disable UI Creation for supporters
            //TODO: Only disable this for party members converted to supporter and not story ones
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_createSupporterUIFunc, 0xC3, 0x90, 0x90);
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_createSupporterUIFunc, 0x40, 0x56, 0x57);
        }
    }
}
