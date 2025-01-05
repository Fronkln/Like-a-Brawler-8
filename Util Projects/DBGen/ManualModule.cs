using LibARMP;
using LibARMP.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DBGen
{
    internal static class ManualModule
    {
        public static void Procedure()
        {
            string rootDir = Path.Combine(Program.refPath, "manual");
            string listFile = Path.Combine(rootDir, "list.txt");

            if (!Directory.Exists(rootDir))
                return;

            ARMP manual = Program.GetInputTable("manual");

            if (manual == null)
                return;

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            Console.WriteLine("------|MANUAL GEN|-----");

            List<string> list = File.ReadAllLines(listFile).ToList();

            foreach (string str in Directory.GetFiles(rootDir, "*.manual"))
            {
                string fileName = Path.GetFileNameWithoutExtension(str);

                if (!list.Contains(fileName))
                    list.Add(fileName);
            }

            File.WriteAllLines(listFile, list);

            ARMP uiTexture = Program.GetOutputPUIDTable("ui_texture");

            foreach(string str in list)
            {
                string dir = Path.Combine(rootDir, str);

                ArmpEntry tutEntry = manual.MainTable.AddEntry(new DirectoryInfo(str).Name);
                ArmpTable mainTable = ((ArmpTableMain)manual.MainTable.GetEntry(1).GetValueFromColumn("table")).Copy(false);
                tutEntry.SetValueFromColumn("table", mainTable);

                foreach (string file in new DirectoryInfo(dir).GetFiles("*.txt")
                    .Select(x => x.FullName)
                    .OrderBy(x => uint.Parse(Path.GetFileNameWithoutExtension(x))))
                {
                    string[] split = File.ReadAllLines(file);

                    StringBuilder instructions = new StringBuilder();

                    for (int i = 2; i < split.Length; i++)
                        instructions.AppendLine(split[i]);

                    ArmpEntry instructionEntry = mainTable.AddEntry();
                    instructionEntry.SetValueFromColumn("8", split[0]);
                    instructionEntry.SetValueFromColumn("1", instructions.ToString());
                    instructionEntry.SetValueFromColumn("7", (byte)2);

                    if(uiTexture != null)
                    {
                        try
                        {
                            ArmpEntry texture = uiTexture.MainTable.GetEntry(split[1]);

                            if (texture != null)
                                instructionEntry.SetValueFromColumn("2", (ushort)texture.ID);
                        }
                        catch
                        {
                            
                        }
                    }
                }

                Console.WriteLine("Added manual " + str + " ID: " + tutEntry.ID);

            }

            ArmpFileWriter.WriteARMPToFile(manual, Path.Combine(Program.dbPath, "manual.bin"));

            Console.WriteLine("------|MANUAL GEN COMPLETE|-----");
        }
    }
}
