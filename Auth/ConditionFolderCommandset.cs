using System;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    public unsafe static class ConditionFolderCommandset
    {
        public static bool Check(IntPtr disableInfo, IntPtr node)
        {
            uint* handle = (uint*)(disableInfo);
            uint* id = (uint*)(disableInfo.ToInt64() + 36);
            Character chara = new EntityHandle<Character>(*handle);

            return (uint)chara.Attributes.command_set_id != *id;
        }
    }
}
