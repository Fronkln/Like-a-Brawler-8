using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DragonEngineLibrary;
using LikeABrawler2.Auth;

namespace LikeABrawler2
{
    internal unsafe static class AuthCustomNodeManager
    {
        private delegate void PlayDeleg(IntPtr thisObj, uint tick, IntPtr mtx, IntPtr parent);
        private static List<PlayDeleg> _playDelegates = new List<PlayDeleg>();

        [DllImport("mods/EX Auth/EXAuth.asi", EntryPoint = "InitializeASI", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool _Init();

        [DllImport("mods/EX Auth/EXAuth.asi", EntryPoint = "RegisterNewNode", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool _RegisterNewNode(uint id);

        [DllImport("mods/EX Auth/EXAuth.asi", EntryPoint = "RegisterPlayFunc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool _RegisterPlayFunc(uint id, IntPtr func);

        [DllImport("mods/EX Auth/EXAuth.asi", EntryPoint = "RegisterPlayFirstFunc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool _RegisterPlayFirstFunc(uint id, IntPtr func);

        [DllImport("mods/EX Auth/EXAuth.asi", EntryPoint = "RegisterPlayLastFunc", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool _RegisterPlayLastFunc(uint id, IntPtr func);

        public static bool RegisterNewNode(uint id)
        {
            return _RegisterNewNode(id);
        }

        public static bool RegisterPlayFunc(uint id, Action<IntPtr, uint, IntPtr, IntPtr> func)
        {
            PlayDeleg del = new PlayDeleg(func);
            _playDelegates.Add(del);

            return _RegisterPlayFunc(id, Marshal.GetFunctionPointerForDelegate(del));
        }

        public static bool RegisterPlayFirstFunc(uint id, Action<IntPtr, uint, IntPtr, IntPtr> func)
        {
            PlayDeleg del = new PlayDeleg(func);
            _playDelegates.Add(del);

            return _RegisterPlayFirstFunc(id, Marshal.GetFunctionPointerForDelegate(del));
        }

        public static bool RegisterPlayLastFunc(uint id, Action<IntPtr, uint, IntPtr, IntPtr> func)
        {
            PlayDeleg del = new PlayDeleg(func);
            _playDelegates.Add(del);

            return _RegisterPlayFirstFunc(id, Marshal.GetFunctionPointerForDelegate(del));
        }

        public static void Init()
        {
            _Init();

            RegisterNewNode(70010);
            //RegisterPlayFirstFunc(70010, AuthNodeLABCharacterDecision.Play);
            RegisterPlayFunc(70010, AuthNodeLABGamemodeDecision.Play);
            RegisterNewNode(70011);
            RegisterPlayFunc(70011, AuthNodeTransitRpgSkill.Play);
            RegisterNewNode(70012);
            RegisterPlayFunc(70012, AuthNodeLABPlayerAssetUseReduce.Play);
            RegisterNewNode(70013);
            RegisterPlayFunc(70013, AuthNodeLABAssetPickup.Play);
            RegisterNewNode(70014);
            RegisterPlayFirstFunc(70014, AuthNodeRobWeapon.Play);
            RegisterNewNode(70015);
            RegisterPlayFunc(70015, AuthNodeTransitHAct.Play);
            RegisterNewNode(70016);
            RegisterPlayFunc(70016, AuthNodeButtonMash.Play);
            RegisterNewNode(70017);
            RegisterPlayFunc(70017, AuthNodeLABSpecial.Play);
            RegisterNewNode(70018);
            RegisterPlayFunc(70018, AuthNodeTransitRange.Play);
            //RegisterPlayLastFunc(70010, AuthNodeLABCharacterDecision.Play);
        }
    }
}
