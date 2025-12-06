using LibARMP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibARMP.IO;
using System.Diagnostics;

namespace DBGen
{
    internal static class SoundCuesheetModule
    {
        private static string genDir = "sound_gen";

        public static ARMP Result;
        
        private static string[] ReadGenOutputFile()
        {
            string genListPath = Path.Combine(genDir, "gen_list_output.txt");
            return File.ReadAllLines(genListPath);
        }

        private static void ProcessGenOutputFile()
        {
            DirectoryInfo genDirInf = new DirectoryInfo(genDir);

            string genListPath = Path.Combine(genDir, "gen_list_output.txt");

            if (!File.Exists(genListPath))
                File.Create(genListPath).Close();

            string[] curDat = ReadGenOutputFile();
            List<string> output = new List<string>();


            File.AppendAllLines(genListPath, output);
        }

        public static void Procedure()
        {
            //libarmp error
            if (Debugger.IsAttached)
                return;

            Console.WriteLine("------|CUESHEET GEN|-----");

            DirectoryInfo genDirInf = new DirectoryInfo(genDir);

            ARMP soundCuesheetInfo = Program.GetInputTable("sound_cuesheet_info");

            if (soundCuesheetInfo == null)
            {
                Console.WriteLine("Sound cuesheet info does not exist in input folder, aborting...");
                return;
            }

            
            DirectoryInfo[] localizedSoundDirs = new DirectoryInfo(Directory.GetCurrentDirectory()).GetDirectories()
                .Where(x => x.Name.StartsWith("sound"))
                .Where(x => x.Name.Contains("_"))
                .Where(x => x.Name.Length <= 8)
                .OrderBy(x => x.Name)
                .ToArray();

            string[] localizedSoundDirPrefixes = localizedSoundDirs
                .Select(x => x.Name.Replace("sound", ""))
                .OrderBy(x => x.Length)
                .ToArray();

            FileInfo[][] localizedSoundDirFiles = localizedSoundDirs
                .Select(x => x.GetFiles("*.acb"))
                .ToArray();


            ProcessGenOutputFile();
            string[] genFile = ReadGenOutputFile();

            foreach(string str in genFile)
            {
                string[] splitDat = str.Split('|');
                string name = splitDat[0];
                string fileName = name + ".acb";

                byte category = byte.Parse(splitDat[1]);

                string soundGenPath = Path.Combine("sound", fileName);
                string streamGenPath = Path.Combine("stream", name + ".awb");

                try
                {
                    soundCuesheetInfo.MainTable.SubTable.GetEntry(name);
                    Console.WriteLine(name + " already exists in sound cuesheet info, skipping...");
                    continue;
                }
                catch
                {

                }

                ArmpEntry mainEntry = soundCuesheetInfo.MainTable.AddEntry();
                mainEntry.SetValueFromColumn("name", name);
                mainEntry.SetValueFromColumn("*cuesheet_id", (ushort)(mainEntry.ID + 1));
                mainEntry.SetValueFromColumn("category", category);
                mainEntry.SetValueFromColumn("exists", true);

                ArmpEntry subTableEntry = soundCuesheetInfo.MainTable.SubTable.AddEntry(name);
                subTableEntry.SetValueFromColumn("0", (uint)(subTableEntry.ID + 1));
                subTableEntry.SetValueFromColumn("2", (uint)(subTableEntry.ID));

                bool streamAdded = false;

                Console.WriteLine(streamGenPath);

                if (File.Exists(streamGenPath))
                {
                    mainEntry.SetValueFromColumn("have_awb", true);
                    streamAdded = true;
                }

                for (int i = 0; i < localizedSoundDirFiles.Length; i++)
                {
                    if (localizedSoundDirFiles[i].FirstOrDefault(x => x.Name == fileName) != null)
                    {
                        string haveLocalizedACBColumn = "have" + localizedSoundDirPrefixes[i];
                        mainEntry.SetValueFromColumn(haveLocalizedACBColumn, true);
                    }
                }

                if(!streamAdded)
                    Console.WriteLine("Added " + name + $", category {category}, ID: " + mainEntry.GetValueFromColumn("*cuesheet_id"));
                else
                    Console.WriteLine("Added " + name + $"(and stream), category {category}, ID: " + mainEntry.GetValueFromColumn("*cuesheet_id"));
            }

            Result = soundCuesheetInfo;
            ArmpFileWriter.WriteARMPToFile(soundCuesheetInfo, Path.Combine(Program.dbPath, "sound_cuesheet_info.bin"));


            string tempCuesheetMapPath = Path.Combine(Program.dbPath, "sound_cuesheet_info.db_index");

            //Until ret fixes the ingame sound_cuesheet_info reading bug, this is a workaround
            if (!File.Exists(tempCuesheetMapPath))
                File.Create(tempCuesheetMapPath).Close();

            Dictionary<uint, string> map = new Dictionary<uint, string>();

            foreach (ArmpEntry entry in soundCuesheetInfo.MainTable.SubTable.GetAllEntries())
                map.Add((uint)entry.GetValueFromColumn("0"), entry.Name);

            File.WriteAllLines(tempCuesheetMapPath, map.Select(x => x.Key.ToString() + " " + x.Value.ToString()));

            Console.WriteLine("------|CUESHEET GEN COMPLETE|-----");
        }
    }
}
