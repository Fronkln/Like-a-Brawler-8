using HActLib;
using LibARMP;
using LibARMP.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBGen
{
    public static class RPGSkillModule
    {
        public static void Procedure()
        {
            Console.WriteLine("------|RPG SKILL GEN|-----");

            string rootDir = Path.Combine(Program.refPath, "rpg_skill");
            string listFile = Path.Combine(rootDir, "list.txt");

            ARMP rpgSkill = Program.GetInputTable("rpg_skill");

            if (rpgSkill == null)
                return;

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            ARMP motionGmt = Program.GetOutputPUIDTable("motion_gmt");
            ARMP cfcList = Program.GetOutputDBTable("battle_command_set");

            List<string> list = File.ReadAllLines(listFile).ToList();

            foreach (string str in Directory.GetFiles(rootDir, "*.dat"))
            {
                string fileName = Path.GetFileNameWithoutExtension(str);

                if (!list.Contains(fileName))
                    list.Add(fileName);
            }

            File.WriteAllLines(listFile, list);

            foreach (string str in list)
            {
                RPGSkillEntry entry = JsonConvert.DeserializeObject<RPGSkillEntry>(File.ReadAllText(Path.Combine(rootDir, str + ".dat")));
                ArmpEntry skillEntry = null;

                if (string.IsNullOrEmpty(entry.OverrideName))
                    skillEntry = rpgSkill.MainTable.AddEntry(str);
                else
                    skillEntry = rpgSkill.MainTable.GetEntry(entry.OverrideName);

                bool replaceMode = !string.IsNullOrEmpty(entry.OverrideName);

                if (!string.IsNullOrEmpty(entry.Motion))
                    skillEntry.SetValueFromColumn("motion_id", (ushort)motionGmt.MainTable.GetEntry(entry.Motion).ID);
                else
                    skillEntry.SetValueFromColumn("motion_id", (ushort)0);

                if (!string.IsNullOrEmpty(entry.CommandSet))
                {
                    skillEntry.SetValueFromColumn("command_set", Convert.ToByte(cfcList.MainTable.SubTable.GetEntry(entry.CommandSet).GetValueFromColumn("0")));
                    skillEntry.SetValueFromColumn("command_name", entry.CommandName);
                }
                else
                {
                    skillEntry.SetValueFromColumn("command_set", (byte)0);
                    skillEntry.SetValueFromColumn("command_name", null);
                }

                if (!replaceMode)
                {
                    skillEntry.SetValueFromColumn("name", entry.Name);
                    skillEntry.SetValueFromColumn("category", (byte)entry.Category);
                    skillEntry.SetValueFromColumn("use_cond", entry.UseCond);
                    skillEntry.SetValueFromColumn("attribute", entry.Attribute);
                    skillEntry.SetValueFromColumn("boot_dist", entry.BootDist);
                    skillEntry.SetValueFromColumn("boot_dist_min", entry.BootDistMin);
                    skillEntry.SetValueFromColumn("eff_range", entry.EffRange);
                    skillEntry.SetValueFromColumn("eff_target_lot_type", entry.EffTargetLotType);
                    skillEntry.SetValueFromColumn("sort_id", entry.SortID);
                    skillEntry.SetValueFromColumn("rest_time", entry.RestTime);
                    skillEntry.SetValueFromColumn("is_sync", entry.IsSync);
                }

                if(!replaceMode)
                    Console.WriteLine("Added " + entry.Name);
                else
                    Console.WriteLine("Replaced " + skillEntry.Name);
            }

            File.WriteAllLines(Path.Combine(Program.dbPath, "rpg_skill.db_index"), Program.CacheARMP(rpgSkill));
            ArmpFileWriter.WriteARMPToFile(rpgSkill, Path.Combine(Program.dbPath, "rpg_skill.bin"));

            Console.WriteLine("------|RPG SKILL GEN COMPLETE|-----");
        }
    }
}
