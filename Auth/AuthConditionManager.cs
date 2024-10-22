using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal static class AuthConditionManager
    {
        private delegate bool ConditionDeleg(IntPtr dat, IntPtr node);
        private static List<ConditionDeleg> _condDelegates = new List<ConditionDeleg>();

        //TODO: EXTENSIONS/EX AUTH CONDITION WAS BROKEN FOR DEVILLEON! NOT GOOD!
        [DllImport("mods/EX Auth Condition/EX Auth Condition.asi", EntryPoint = "EX_AUTH_COND_REGISTER_CONDITION", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool _RegisterPrivate(uint id, IntPtr func);

        public static void Init()
        {
            Register(133700001, ConditionFolderPlayerJob.CheckDisabled);
            Register(2, ConditionFolderDragonBoost.Check);
            Register(133700002, ConditionFolderCommandset.Check);
            Register(133700003, ConditionFolderGamemode.Check);
            Register(133700004, ConditionFolderPlayerID.Check);
            Register(133700005, ConditionFolderLABISDemo.Check);
            Register(133700006, ConditionFolderLABBattleOverByY8BHact.Check);
            Register(133700007, ConditionFolderIsNotSupporter.CheckDisabled);
            Register(133700008, ConditionFolderKiryuStyle.Check);
            Register(133700009, ConditionFolderIsActiveBrawlerPlayer.CheckDisabled);
            Register(133700010, ConditionFolderNotHActOrWaiting.CheckDisabled);
        }


        public static bool Register(uint id, Func<IntPtr, IntPtr, bool> checkFunc)
        {
            ConditionDeleg del = new ConditionDeleg(checkFunc);
            _condDelegates.Add(del);

            return _RegisterPrivate(id, Marshal.GetFunctionPointerForDelegate(del));
        }
    }
}