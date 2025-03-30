using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibARMP;
using LibARMP.IO;
using Newtonsoft.Json;


namespace DBGen
{
    internal static class BattleCommandSetModule
    {
        public static void Procedure()
        {
#if DEBUG
            //libarmp error
            if (Debugger.IsAttached)
                return;
#endif

            string rootDir = Path.Combine("db_gen.puid", "command_set");
            string listFile = Path.Combine(rootDir, "list.txt");

            ARMP battleCommandSet = Program.GetInputTable("battle_command_set");

            if (battleCommandSet == null)
                return;

            List<string> list = File.ReadAllLines(listFile).ToList();

            foreach (string str in Directory.GetFiles(rootDir, "*.dat"))
            {
                string str2 = str.Replace(rootDir + @"\", "");

                if (!list.Contains(str2))
                    list.Add(str2);
            }

            File.WriteAllLines(listFile, list);

            ARMP battleMotionSet = Program.GetOutputDBTable("battle_motion_set");
            ARMP cfcList = Program.GetOutputPUIDTable("battle_command_set");

            foreach(string str in list)
            {
                BattleCommandSetEntry entryDat = JsonConvert.DeserializeObject<BattleCommandSetEntry>(File.ReadAllText(rootDir + @"\" + str));
                ArmpEntry entry = battleCommandSet.MainTable.AddEntry();
                entry.SetValueFromColumn("motion_set", (ushort)battleMotionSet.MainTable.GetEntry(entryDat.MotionSet).ID);
                ArmpEntry subEntry = battleCommandSet.MainTable.SubTable.AddEntry(Path.GetFileNameWithoutExtension(str));
                subEntry.SetValueFromColumn("0", cfcList.MainTable.GetEntry(subEntry.Name).ID);
                subEntry.SetValueFromColumn("2", entry.ID);
            }

            File.WriteAllLines(Path.Combine(Program.dbPath, "battle_command_set.db_index"), Program.CacheARMP(Program.GetOutputPUIDTable("battle_command_set")));
            ArmpFileWriter.WriteARMPToFile(battleCommandSet, Path.Combine(Program.dbPath, "battle_command_set.bin"));
        }
    }
}
