using System;

namespace LikeABrawler2
{
    internal static class ConditionFolderNotHActOrWaiting
    {
        public static bool CheckDisabled(IntPtr disableInfo, IntPtr node)
        {
            return BrawlerBattleManager.IsHActOrWaiting;
        }
    }
}
