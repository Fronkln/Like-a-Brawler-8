using DragonEngineLibrary;
using System;
using System.Runtime.InteropServices;

namespace LikeABrawler2
{
    internal class ParticlePatches : BrawlerPatch
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private unsafe delegate IntPtr ParticleManagerPlay(IntPtr manager, IntPtr result, uint particleID, IntPtr mtx, uint type);

        private IntPtr m_particleFunc;

        public override void Init()
        {
            m_particleFunc = DragonEngineLibrary.Unsafe.CPP.ReadCall(DragonEngineLibrary.Unsafe.CPP.PatternSearch("E8 ? ? ? ? 49 81 C6 ? ? ? ? 4C 3B F0 0F 84 ? ? ? ?"));
            _ptcManPlayTrampoline =  BrawlerPatches.HookEngine.CreateHook<ParticleManagerPlay>(m_particleFunc, ParticleManager_Play);
        }

        protected override void SetActive()
        {
            base.SetActive();

            if(_ptcManPlayTrampoline != null)
                BrawlerPatches.HookEngine.EnableHook(_ptcManPlayTrampoline);
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            if (_ptcManPlayTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(_ptcManPlayTrampoline);
        }

        private static ParticleManagerPlay _ptcManPlayTrampoline;
        private static unsafe IntPtr ParticleManager_Play(IntPtr manager, IntPtr result, uint particleID, IntPtr mtx, uint type)
        {
            //RPG effects we dont want in a realtime setting
            //These pibs include the movement area, the yellow indicator above players head etc...
            if (particleID == 0x31EC || particleID == 0x3898 || particleID == 0x31EB)
                particleID = 0;

            if (particleID == 12399)
                particleID = 23136; //HYa0001 -> BHYa0001 (smaller version of pib)

            return _ptcManPlayTrampoline(manager, result, particleID, mtx, type);
        }
    }
}
