using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2.Auth
{
    public static class AuthNodeLABAssetPickup
    {
        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, IntPtr parent)
        {
            WeaponManager.PickupNearestWeapon();
        }
    }
}
