using LibARMP;
using LibARMP.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBGen
{
    internal static class InputGameStateModule
    {
        public static void Procedure()
        {
            string rootDir = Path.Combine(Program.refPath);
            string dirs = Path.Combine(rootDir, "input_game_state");


            ARMP inputGameState = Program.GetInputTable("input_game_state");

            if (!Directory.Exists(dirs))
                return;

            if (inputGameState == null)
                return;

            ARMP inputActionList = Program.GetOutputDBTable("input_action");

            if (inputActionList == null)
                return;

            Console.WriteLine("------|INPUT GAME STATE GEN|-----");

            foreach (DirectoryInfo inf in new DirectoryInfo(dirs).GetDirectories())
            {
                string[] split = inf.Name.Split('-');
                string profileName = split[0];
                string name = split[1];

                ArmpEntry entry = inputGameState.MainTable.GetAllEntries().FirstOrDefault(x => (string)x.GetValueFromColumn("profile_name") == profileName);

                if (entry == null)
                    continue;

                Console.WriteLine("Overriding inputs for profile "  + profileName + "(" + name + ")");

                entry.SetValueFromColumn("name", name);


                ArmpTable bindingsTable = (ArmpTable)entry.GetValueFromColumn("bindings");
                bindingsTable.GetAllEntries().Clear();

                //parse buttons
                foreach(string inputFile in inf.GetFiles("*.txt")
                    .Select(x => x.FullName)
                    .OrderBy(x => uint.Parse(Path.GetFileNameWithoutExtension(x))))
                {
                    string dat = File.ReadAllText(inputFile);
                    InputGameStateEntry input = JsonConvert.DeserializeObject<InputGameStateEntry>(dat);

                    ArmpEntry inputActionEntry = inputActionList.MainTable.GetEntry(input.InputActionName);
                    ArmpEntry inputEntry = bindingsTable.AddEntry();
                    inputEntry.SetValueFromColumn("1", (ushort)inputActionEntry.ID);
                    inputEntry.SetValueFromColumn("2", input.Button1);
                    inputEntry.SetValueFromColumn("3", input.Button2);
                }
            }


            ArmpFileWriter.WriteARMPToFile(inputGameState, Path.Combine(Program.dbPath, "input_game_state.bin"));
            Console.WriteLine("------|INPUT GAME STATE GEN COMPLETE|-----");
        }
    }
}
