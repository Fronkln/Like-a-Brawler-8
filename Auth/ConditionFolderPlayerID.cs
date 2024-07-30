using System;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    public unsafe static class ConditionFolderPlayerID
    {
        public static bool Check(IntPtr disableInfo, IntPtr node)
        {
            uint* id = (uint*)(disableInfo.ToInt64() + 36);

            if (BrawlerBattleManager.Battling)
                return !((uint)BrawlerPlayer.CurrentPlayer == *id);
            else
                return !((uint)BrawlerBattleManager.PlayerCharacter.Attributes.player_id == *id);
        }
    }
}
