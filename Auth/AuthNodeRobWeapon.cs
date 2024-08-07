using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    //Battle Special version didnt seem like it worked? azk_gun
    internal unsafe static class AuthNodeRobWeapon
    {
        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, IntPtr parent)
        {

            AssetID wep = HeatActionManager.PerformingHAct.Map[ElvisCommand.HeatActionActorType.Enemy1].GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().AssetID;
            BrawlerBattleManager.PlayerFighter.Equip(wep, AttachmentCombinationID.right_weapon, 0, 0);

        }
    }
}
