using System;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    public unsafe static class ConditionFolderKiryuStyle
    {
        public static bool Check(IntPtr disableInfo, IntPtr node)
        {
            uint* handle = (uint*)(disableInfo);

            uint* id = (uint*)(disableInfo.ToInt64() + 36);
            uint idVal = *id;
            uint currentStyle = (uint)BrawlerPlayer.CurrentStyle;

            return currentStyle != idVal;
        }
    }
}
