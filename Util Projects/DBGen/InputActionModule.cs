using LibARMP;
using LibARMP.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBGen
{
    internal static class InputActionModule
    {
        public static void Procedure()
        {
            string rootDir = Path.Combine(Program.refPath);
            string listFile = Path.Combine(rootDir, "input_action.txt");

            ARMP inputAction = Program.GetInputTable("input_action");

            if (inputAction == null)
                return;

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            Console.WriteLine("------|INPUT ACTION GEN|-----");

            foreach(string str in File.ReadAllLines(listFile))
            {
                string[] split = str.Split('|');

                ArmpEntry entry = inputAction.MainTable.AddEntry(split[0]);
                entry.SetValueFromColumn("name", split[1]);

                Console.WriteLine("Added input action " + split[0] + ", ID: " + entry.ID);
            }

            ArmpFileWriter.WriteARMPToFile(inputAction, Path.Combine(Program.dbPath, "input_action.bin"));
            Console.WriteLine("------|INPUT ACTION GEN COMPLETE|-----");
        }
    }
}
