using System;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    public unsafe static class ConditionFolderGamemode
    {
        //0 = turn based, 1 = realtime
        public static bool Check(IntPtr disableInfo, IntPtr node)
        {
            uint* handle = (uint*)(disableInfo);
            uint* id = (uint*)(disableInfo.ToInt64() + 36);

            return !(Mod.Gamemode == *id);
        }
    }
}
