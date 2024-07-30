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
    internal static class TalkSelectModule
    {
        public static void Procedure()
        {
            Console.WriteLine("------|TALK SELECT GEN|-----");

            string rootDir = Path.Combine(Program.refPath);
            string listFile = Path.Combine(rootDir, "talk_select_select", "list.txt");

            ARMP select = Program.GetInputTable("talk_select_select");

            if (select == null)
                return;

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            foreach (string str in File.ReadAllLines(listFile))
            {
                string[] lines = File.ReadAllLines(Path.Combine(rootDir, "talk_select_select", str + ".txt"));
                string[] split = lines[0].Split(' ');

                ArmpEntry entry = select.MainTable.AddEntry(str);
                ArmpTable table = ((ArmpTable)select.MainTable.GetEntry(1).GetValueFromColumn("choice")).CopyTableAsMain();

                entry.SetValueFromColumn("select_type", byte.Parse(split[0]));
                entry.SetValueFromColumn("cancel_type", byte.Parse(split[1]));

                for (int i = 1; i < lines.Length; i++)
                {
                    table.AddEntry().SetValueFromColumn("1", lines[i]);
                }
                entry.SetValueFromColumn("choice", table);
            }

            ArmpFileWriter.WriteARMPToFile(select, Path.Combine(Program.dbPath, "talk_select_select.bin"));
            Console.WriteLine("------|TALK SELECT COMPLETE|-----");
        }
    }
}
