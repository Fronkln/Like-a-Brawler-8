using System;
using System.Globalization;
using System.IO;

namespace LikeABrawler2
{
    internal static class IniSettings
    {
        public static bool ShowPlayerDamage = true;
        public static bool ShowEnemyDamage = false;
        public static int IsIchibanRealtime = 1;
        public static int IsKiryuRealtime = 1;
        public static float PartyMemberSkillMPReqRatio = 0.5f;
        public static int PartyMemberSkillChance = 20;
        public static float PartyMemberSkillTime = 30;

        public static bool AllowResurgenceMusic = false;

        public static string IniPath()
        {
            return Path.Combine(Mod.ModPath, "mod_settings.ini");
        }

        public static void Read()
        {
            Ini ini = new Ini(IniPath());
            ShowPlayerDamage = ini.GetValue("ShowPlayerDamage", "Display") == "1";
            ShowEnemyDamage = ini.GetValue("ShowEnemyDamage", "Display") == "1";
            IsIchibanRealtime = int.Parse(ini.GetValue("IchibanRealtime", "Gameplay", "1"));
            IsKiryuRealtime = int.Parse(ini.GetValue("KiryuRealtime", "Gameplay", "1"));
            PartyMemberSkillMPReqRatio = float.Parse(ini.GetValue("PartyMemberSkillMPRequirementRatio", "Party", "0.5"), CultureInfo.InvariantCulture);
            PartyMemberSkillChance = int.Parse(ini.GetValue("PartyMemberSkillChance", "Party", "20"));
            PartyMemberSkillTime = float.Parse(ini.GetValue("PartyMemberSkillTime", "Party", "30"), CultureInfo.InvariantCulture);
            AllowResurgenceMusic = ini.GetValue("PlayDragonResurgenceMusic", "Gameplay", "1") == "1";
            BrawlerUIManager.UseClassicGauge = ini.GetValue("UseClassicGauge", "Gameplay", "0") == "1";
        }

        public static void Write()
        {
            Ini ini = new Ini(IniPath());
            ini.WriteValue("ShowPlayerDamage", "Display", Convert.ToByte(ShowPlayerDamage).ToString());
            ini.WriteValue("ShowEnemyDamage", "Display", Convert.ToByte(ShowEnemyDamage).ToString());
            ini.WriteValue("IchibanRealtime", "Gameplay", IsIchibanRealtime.ToString());
            ini.WriteValue("KiryuRealtime", "Gameplay", IsKiryuRealtime.ToString());
            ini.WriteValue("AllowResurgenceMusic", "Gameplay", Convert.ToByte(AllowResurgenceMusic).ToString());
            ini.WriteValue("UseClassicGauge", "Gameplay", Convert.ToByte(BrawlerUIManager.UseClassicGauge).ToString());
            ini.WriteValue("PartyMemberSkillMPRequirementRatio", "Party", PartyMemberSkillMPReqRatio.ToString(CultureInfo.InvariantCulture));
            ini.WriteValue("PartyMemberSkillChance", "Party", PartyMemberSkillChance.ToString());
            ini.WriteValue("PartyMemberSkillTime", "Party", PartyMemberSkillTime.ToString(CultureInfo.InvariantCulture));
            ini.Save();
        }

        public static bool IsPlayerRealtime()
        {
            if (BrawlerPlayer.IsKasuga())
                return IsIchibanRealtime == 1;
            else if (BrawlerPlayer.IsKiryu())
                return IsKiryuRealtime == 1;

            else return IsIchibanRealtime == 1;
        }
    }
}
