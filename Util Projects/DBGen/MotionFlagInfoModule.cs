using LibARMP;
using LibARMP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DBGen
{
    public static class MotionFlagInfoModule
    {
        private struct MotionEntry
        {
            public uint ID;
            public string Name;
        }

        public static void Procedure()
        {

            //libarmp error
            if (Debugger.IsAttached)
                return;

            Console.WriteLine("------|MOTION FLAG INFO GEN|-----");

            string genDir = "motion/gen/flag/";
            string genFilePath = genDir + "_gen.info";

            ARMP motionFlagBin = Program.GetInputTable("motion_flag_info");

            if (motionFlagBin == null)
                return;

            if (!File.Exists(genFilePath))
                File.Create(genFilePath).Close();

            string[] motionList = Directory.GetFiles(genDir, "*.txt");

            ARMP gmtTable = Program.GetOutputPUIDTable("motion_gmt");

            List<MotionEntry> gmtEntries = new List<MotionEntry>();

            string[] genFile = File.ReadAllLines("db_gen.puid/motion_gmt.txt");

            foreach(string str in genFile)
            {
                if (!File.Exists(genDir + str + ".txt"))
                    continue;

                ArmpEntry puidEntry = gmtTable.MainTable.GetEntryProper(str);

                if (puidEntry == null)
                    continue;

                MotionEntry entry = new MotionEntry() { ID = puidEntry.ID, Name = str };
                gmtEntries.Add(entry);
            }

            gmtEntries = gmtEntries.OrderBy(x => x.ID).ToList();

            foreach(MotionEntry entry in gmtEntries)
            {
                string flagFile = "motion/gen/flag/" + entry.Name + ".txt";
                MotionFlagInfo flags = JsonConvert.DeserializeObject<MotionFlagInfo>(File.ReadAllText(flagFile));

                ArmpEntry newEntryMain = motionFlagBin.MainTable.AddEntry();
                ArmpEntry newEntrySub = null;


                if(motionFlagBin.MainTable.SubTable != null)
                    newEntrySub = motionFlagBin.MainTable.SubTable.AddEntry(entry.Name);

                TrySet(newEntryMain, "is_common_pack", flags.is_common_pack);
                TrySet(newEntryMain, "is_loop", flags.is_loop);
                TrySet(newEntryMain, "base_gmt", flags.base_gmt);
                TrySet(newEntryMain, "start_frame", flags.start_frame);
                TrySet(newEntryMain, "is_counter_attack", flags.is_counter_attack);
                TrySet(newEntryMain, "action_joint_tick", flags.action_joint_tick);
                TrySet(newEntryMain, "is_charge_attack", flags.is_charge_attack);
                TrySet(newEntryMain, "is_nage_attack", flags.is_nage_attack);
                TrySet(newEntryMain, "is_finish_hold_attack", flags.is_finish_hold_attack);
                TrySet(newEntryMain, "is_heavy_attack", flags.is_heavy_attack);
                TrySet(newEntryMain, "is_light_attack", flags.is_light_attack);
                TrySet(newEntryMain, "is_damage_reversal", flags.is_damage_reversal);
                TrySet(newEntryMain,    "is_ma_break", flags.is_ma_break);
                TrySet(newEntryMain, "is_reverse_attack", flags.is_reverse_attack);
                TrySet(newEntryMain, "is_enable_rev_sabaki", flags.is_enable_rev_sabaki);
                TrySet(newEntryMain, "is_sway_attack", flags.is_sway_attack);
                TrySet(newEntryMain, "is_ex_attack", flags.is_ex_attack);
                TrySet(newEntryMain,    "is_revenge", flags.is_revenge);
                TrySet(newEntryMain, "is_run_attack", flags.is_run_attack);
                TrySet(newEntryMain,"is_low_attack", flags.is_low_attack);
                TrySet(newEntryMain, "is_combo_finish", flags.is_combo_finish);

                if (newEntrySub != null)
                {
                    newEntrySub.SetValueFromColumn("0", (uint)entry.ID);
                    newEntrySub.SetValueFromColumn("2", (uint)newEntryMain.ID);
                }

                Console.WriteLine("Added motion flags for " + entry.Name);
            }

            ArmpFileWriter.WriteARMPToFile(motionFlagBin, Path.Combine(Program.dbPath, "motion_flag_info.bin"));

            Console.WriteLine("------|MOTION FLAG INFO GEN COMPLETE|-----");
        }

        private static void TrySet(ArmpEntry entry, string column, object value)
        {
            try
            {
                entry.SetValueFromColumn(column, value);
            }
            catch
            {

            }
        }
    }
}
