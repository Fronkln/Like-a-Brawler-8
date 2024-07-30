using System;
using System.IO;

namespace LikeABrawler2
{
    internal static class IniSettings
    {
        public static bool ShowPlayerDamage = true;
        public static bool ShowEnemyDamage = false;
        public static int IsIchibanRealtime = 1;
        public static int IsKiryuRealtime = 1;

        public static string IniPath()
        {
            return Path.Combine(Mod.ModPath, "mod_settings.ini");
        }

        public static void Read()
        {
            Ini ini = new Ini(IniPath());
            ShowPlayerDamage = ini.GetValue("ShowPlayerDamage", "Display") == "1";
            ShowEnemyDamage = ini.GetValue("ShowEnemyDamage", "Display") == "1";
            IsIchibanRealtime = int.Parse(ini.GetValue("IchibanRealtime", "Gameplay"));
            IsKiryuRealtime = int.Parse(ini.GetValue("KiryuRealtime", "Gameplay"));
        }

        public static void Write()
        {
            Ini ini = new Ini(IniPath());
            ini.WriteValue("ShowPlayerDamage", "Display", Convert.ToByte(ShowPlayerDamage).ToString());
            ini.WriteValue("ShowEnemyDamage", "Display", Convert.ToByte(ShowEnemyDamage).ToString());
            ini.WriteValue("IchibanRealtime", "Gameplay", IsIchibanRealtime.ToString());
            ini.WriteValue("KiryuRealtime", "Gameplay", IsKiryuRealtime.ToString());
            ini.Save();
        }
    }
}
