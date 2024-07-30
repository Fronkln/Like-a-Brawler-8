using System;
using System.Runtime.InteropServices;
using DragonEngineLibrary;
using DragonEngineLibrary.Unsafe;

namespace LikeABrawler2
{
    internal unsafe class AuthPatches : BrawlerPatch
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate void AuthPlayCameraActive(IntPtr authPlay);

        private IntPtr m_cameraActiveFunc;

        public override void Init()
        {
            base.Init();

            m_cameraActiveFunc = CPP.ReadCall(CPP.PatternSearch("E8 ? ? ? ? 8B 8F 44 08 00 00"));
        }

        protected override void SetActive()
        {
            base.SetActive();

            if (_cameraActiveTrampoline == null)
                _cameraActiveTrampoline = BrawlerPatches.HookEngine.CreateHook<AuthPlayCameraActive>(m_cameraActiveFunc, AuthPlay_CameraActive);

            BrawlerPatches.HookEngine.EnableHook(_cameraActiveTrampoline);
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            if(_cameraActiveTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(_cameraActiveTrampoline);
        }

        private static AuthPlayCameraActive _cameraActiveTrampoline;
        private static void AuthPlay_CameraActive(IntPtr authPlay)
        {
            if (HeatActionManager.IsY8BHact)
            {
                int* flags = (int*)(authPlay.ToInt64() + 0x834);
                *flags |= 0x1000;
                *flags &= ~0x2000;
            }

            _cameraActiveTrampoline(authPlay);
        }
    }
}
