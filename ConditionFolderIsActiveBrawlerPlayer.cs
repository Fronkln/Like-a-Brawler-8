using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal unsafe static class ConditionFolderIsActiveBrawlerPlayer
    {
        public static bool CheckDisabled(IntPtr disableInfo, IntPtr node)
        {
            if (Mod.IsTurnBased())
                return true;

            uint* handle = (uint*)(disableInfo);
            Character chara = new EntityHandle<Character>(*handle);

            return BrawlerBattleManager.PlayerCharacter.UID == chara.UID;
        }
    }
}
