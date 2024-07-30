using LikeABrawler2;
using System;

namespace LikeABrawler2
{
    public static class ConditionFolderDragonBoost
    {
        public static bool Check(IntPtr disableInfo, IntPtr node)
        {
            return !BrawlerPlayer.IsExtremeHeat;
        }
    }
}