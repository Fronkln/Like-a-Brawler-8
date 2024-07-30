using System;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    public unsafe static class ConditionFolderLABISDemo
    {
        public static bool Check(IntPtr disableInfo, IntPtr node)
        {
            return !Mod.IsDemo();
        }
    }
}
