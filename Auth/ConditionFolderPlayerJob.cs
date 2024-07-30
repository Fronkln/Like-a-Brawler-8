using System;
using System.Runtime.InteropServices;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal unsafe static class ConditionFolderPlayerJob
    {
        public static bool CheckDisabled(IntPtr disableInfo, IntPtr node)
        {
            uint* id = (uint*)(disableInfo.ToInt64() + 36);

            RPGJobID playerJob;

            if(BrawlerPlayer.IsExtremeHeat)
                playerJob = Player.GetCurrentJob(BrawlerPlayer.CurrentPlayer);
            else
            {
                if (BrawlerPlayer.IsKiryu())
                    playerJob = RPGJobID.kiryu_01;
                else
                    playerJob = RPGJobID.kasuga_freeter;
            }

            if ((uint)playerJob != *id)
                return true;

            return false;
        }
    }
}