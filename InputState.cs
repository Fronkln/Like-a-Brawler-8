using System;
using System.Runtime.InteropServices;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal static class InputState
    {
        private delegate IntPtr GetInputManager();
        private delegate void InputState_Push(int inputState);


        private static GetInputManager m_getInputMan;
        private static InputState_Push m_pushInputState;

        public static void Init()
        {
            //48 83 EC ? E8 ? ? ? ? 48 8B 80 00 03 00 00
            m_getInputMan = (GetInputManager)Marshal.GetDelegateForFunctionPointer(DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 83 EC ? E8 ? ? ? ? 48 8B 80 00 03 00 00"), typeof(GetInputManager));
           // m_pushInputState = (InputState_Push)Marshal.GetDelegateForFunctionPointer(DragonEngineLibrary.Unsafe.CPP.PatternSearch(" 48 89 5C 24 18 48 89 74 24 20 41 56 48 83 EC ? 8B 71 0C 48 8B D9 8B 49 08 4C 8B F2 48 89 7C 24 38 3B CE 77 ? 8D 41 01 48 89 6C 24 30 D1 ? BD ? ? ? ? 03 C1 0F 45 E8 83 C5 ? 83 E5 ? 8B CD 48 C1 E1 ? E8 ? ? ? ? 48 8B F8 8B 43 0C 85 C0 74 ? 33 D2 85 C0 74 ? 66 0F 1F 44 00 00 48 8D 0C 95 ? ? ? ? 4C 8D 04 39 4D 85 C0 74 ? 48 8B 03 8B 0C 01 41 89 08 FF C2 3B 53 0C 72 ? 48 8B 0B 48 85 C9 74 ? E8 ? ? ? ? 89 6B 08 48 8B 6C 24 30 48 89 3B EB ? 48 8B 3B 48 8D 04 B7 48 8B 7C 24 38 48 85 C0 74 ? 41 8B 0E 89 08 FF 43 0C 8B C6 48 8B 5C 24 40 48 8B 74 24 48 48 83 C4 ? 41 5E C3"), typeof(InputState_Push));
        }

        public unsafe static void Push(int gameState)
        {
            int* val = (int*)m_getInputMan();
            *val = gameState;
        }
    }
}
