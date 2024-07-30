using System;
using System.Collections.Generic;
using DragonEngineLibrary;

namespace LikeABrawler2
{

    //2 bytes - Ichiban and Kiryu Revelation Queue
    //
    public unsafe static class BrawlerSaveData
    {
        private static IntPtr GetDataPtr()
        {
            IntPtr addr = SaveData.GetItem(35);
            ulong* brawlerFlags = (ulong*)(addr.ToInt64() + 0x8);

            return (IntPtr)brawlerFlags;
        }


        //If higher than zero, means we are gonna show all revelations starting from
        //The lowest revelation we haven't seen yet
        //Example, player is level 4, jumps to level 99
        //Save byte is set to level 4, revelations starting from level 4 will be shown.
        public static void SetRevelationQueue(int playerLevel, bool kiryu)
        {
            IntPtr data = GetDataPtr();

            if (playerLevel < 0)
                playerLevel = 0;
            if (playerLevel > 99)
                playerLevel = 99;

            byte* start = (byte*)data;

            if (kiryu)
                start++;

            *start = (byte)playerLevel;
        }

        public static uint GetRevelationQueue(bool kiryu)
        {
            IntPtr data = GetDataPtr();

            byte* start = (byte*)data;

            if (kiryu)
                start++;

            return (uint)*start;
        }
    }
}