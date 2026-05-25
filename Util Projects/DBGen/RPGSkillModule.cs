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
        private static bool m_added = false;

        private static Dictionary<RPGSkillEntry, ArmpEntry> processedEntries = new Dictionary<RPGSkillEntry, ArmpEntry>();
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
                    skillEntry = rpgSkill.GetMainTable().AddEntry(str);
                else
                    skillEntry = rpgSkill.GetMainTable().GetEntry(entry.OverrideName);

                m_added = true;

                bool replaceMode = !string.IsNullOrEmpty(entry.OverrideName);

                if (!string.IsNullOrEmpty(entry.Motion))
                    skillEntry.SetValueFromColumn("motion_id", (ushort)motionGmt.GetMainTable().GetEntry(entry.Motion).ID);
                else
                    skillEntry.SetValueFromColumn("motion_id", (ushort)0);

                if (!string.IsNullOrEmpty(entry.CommandSet))
                {
                    var commandsetEntry = cfcList.GetMainTable().Indexer.GetEntry(entry.CommandSet);
                    skillEntry.SetValueFromColumn("command_set", Convert.ToInt16(commandsetEntry.GetValueFromColumn("0")));
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

                    skillEntry.SetValueFromColumn("eff_range", entry.EffRange);
                    skillEntry.SetValueFromColumn("eff_target_lot_type", entry.EffTargetLotType);
                    skillEntry.SetValueFromColumn("sort_id", entry.SortID);
                    skillEntry.SetValueFromColumn("rest_time", entry.RestTime);
                    skillEntry.SetValueFromColumn("is_sync", entry.IsSync);
                    skillEntry.SetValueFromColumn("base_atk_ratio", entry.BaseAttackRatio);
                    skillEntry.SetValueFromColumn("sp_atk_ratio", entry.SPAttackRatio);
                }
                else
                {
                  //  if(entry.BaseAttackRatio > 0)
                        skillEntry.SetValueFromColumn("base_atk_ratio", entry.BaseAttackRatio);
                }


                if (entry.BootDistMin >= 0)
                    skillEntry.SetValueFromColumn("boot_dist_min", entry.BootDistMin);

                if (entry.UseFixWep > 0)
                    skillEntry.SetValueFromColumn("use_fix_wep", entry.UseFixWep);

                if (entry.UseFixWepL > 0)
                    skillEntry.SetValueFromColumn("use_fix_wep_l", entry.UseFixWepL);

                if(entry.PlayEffectSelf > 0)
                    skillEntry.SetValueFromColumn("play_effect_self", entry.PlayEffectSelf);

                if (entry.IsMoveCut.HasValue)
                    skillEntry.SetValueFromColumn("is_move_cut", entry.IsMoveCut);

                processedEntries.Add(entry, skillEntry);

                if(!replaceMode)
                    Console.WriteLine("Added " + entry.Name);
                else
                    Console.WriteLine("Replaced " + skillEntry.Name);
            }


            foreach(var kv in processedEntries)
            {
                if (!string.IsNullOrEmpty(kv.Key.DownBranchSkill))
                {
#warning TODO: just move this table getting to start of skill gen i was too lazy to do it now
                    var baseBranchSkill = rpgSkill.GetMainTable().GetEntry("kasuga_job01_nml_atk");

                    if (baseBranchSkill != null)
                    {
                        var newBranchSkill = baseBranchSkill.GetValueFromColumn<ArmpTable>("branch_act_tbl").Copy();
                        newBranchSkill.DeleteEntry(3);
                        newBranchSkill.DeleteEntry(2);
                        newBranchSkill.DeleteEntry(1);
                        newBranchSkill.SetValue(0, "1", (ushort)rpgSkill.GetMainTable().GetEntry(kv.Key.DownBranchSkill).ID);
         
                        kv.Value.SetValueFromColumn("branch_act_tbl", newBranchSkill);
                    }
                }
            }

            File.WriteAllLines(Path.Combine(Program.dbPath, "rpg_skill.db_index"), Program.CacheARMP(rpgSkill));
            
            
            if(m_added)
                ArmpFileWriter.WriteARMPToFile(rpgSkill, Path.Combine(Program.dbPath, "rpg_skill.bin"));

            Console.WriteLine("------|RPG SKILL GEN COMPLETE|-----");
        }
    }
}
