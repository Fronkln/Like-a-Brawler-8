using System;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal static class AuthNodeLABPlayerAssetUseReduce
    {
        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, IntPtr parent)
        {
            //07.01.2025: Obsoleted, now handled without nodes on BaseEnemyAI.OnTakeDAmage
            //WeaponManager.OnHitWeapon();
        }
    }
}
