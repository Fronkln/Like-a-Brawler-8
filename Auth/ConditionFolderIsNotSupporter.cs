using System;
using System.Runtime.InteropServices;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal unsafe static class ConditionFolderIsNotSupporter
    {
        public static bool CheckDisabled(IntPtr disableInfo, IntPtr node)
        {
            if (Mod.IsTurnBased())
                return false;

            uint* handle = (uint*)(disableInfo);
            Character chara = new EntityHandle<Character>(*handle);

            return SupporterManager.GetAI(chara.UID) != null;
        }
    }
}