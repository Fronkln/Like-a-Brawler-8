﻿using System;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal static class AuthNodeLABPlayerAssetUseReduce
    {
        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, uint unk)
        {
            WeaponManager.OnHitWeapon();
        }
    }
}
