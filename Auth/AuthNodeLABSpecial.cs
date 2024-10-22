using DragonEngineLibrary;
using System;
using System.Runtime.InteropServices;

namespace LikeABrawler2.Auth
{
    internal static class AuthNodeLABSpecial
    {
        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, IntPtr parent)
        {
            int type = Marshal.ReadInt32((IntPtr)(thisObj.ToInt64() + 48));

            switch(type)
            {
                case 0:
                    BrawlerSpecial.TransitDragonFear();
                    break;
                case 1:
                    BrawlerSpecial.TransitSoloFight(); //ebina fight
                    break;
                case 2:
                    int idx = Marshal.ReadInt32((IntPtr)(thisObj.ToInt64() + 52));
                    BrawlerSpecial.RemoveNakamaUI(idx); //ebina fight
                    break;
            }
        }
    }
}
