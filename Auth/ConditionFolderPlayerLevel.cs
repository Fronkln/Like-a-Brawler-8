using System;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal unsafe static class ConditionFolderPlayerLevel
    {
        public static bool Check(IntPtr disableInfo, IntPtr node)
        {
            uint* level = (uint*)(disableInfo.ToInt64() + 36);
            return !(Player.GetLevel(Mod.MainPlayer.PlayerID) >= *level);
        }
    }
}
