using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal unsafe static class ConditionFolderNotPocketWeapon
    {
        public static bool Check(IntPtr disableInfo, IntPtr node)
        {
            uint* handle = (uint*)(disableInfo);
            uint* id = (uint*)(disableInfo.ToInt64() + 36);
            Character chara = new EntityHandle<Character>(*handle);

            return BrawlerFighterInfo.Player.RightWeapon.IsFromPocket();
        }
    }
}
