using LibARMP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibARMP.IO;

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

            DirectoryInfo[] localizedSoundDirs = genDirInf.GetDirectories()
                .Where(x => x.Name.StartsWith("sound"))
                .Where(x => x.Name.Length <= 8)
                .OrderBy(x => x.Name)
                .ToArray();

            FileInfo[][] localizedSoundDirFiles = localizedSoundDirs
                .Select(x => x.GetFiles("*.acb"))
                .ToArray();

            string[] curDat = ReadGenOutputFile();
            List<string> output = new List<string>();

            foreach(FileInfo inf in localizedSoundDirFiles[0])
            {
                string[] nameSplit = inf.Name.Split('_');
                string name = "";

                for (int i = 1; i < nameSplit.Length; i++)
                {
                    name += nameSplit[i];

                    if (i != nameSplit.Length - 1)
                        name += "_";
                }

                string fileName = name;
                name = name.Replace(".acb", "");

                string soundGenPath = Path.Combine(genDir, "sound", fileName);
                string streamGenPath = Path.Combine(genDir, "stream", fileName);

                byte category = byte.Parse(nameSplit[0].Replace("ctg", ""));

                output.Add(name + "|" + category);

                inf.CopyTo(Path.Combine("sound", fileName), true);
                inf.Delete();

                if(File.Exists(streamGenPath))
                {
                    File.Copy(streamGenPath, Path.Combine("stream", name + ".awb"), true);
                    File.Delete(streamGenPath);
                }
            }

            File.AppendAllLines(genListPath, output);
        }

        public static void Procedure()
        {
            Console.WriteLine("------|CUESHEET GEN|-----");

            DirectoryInfo genDirInf = new DirectoryInfo(genDir);

            ARMP soundCuesheetInfo = Program.GetInputTable("sound_cuesheet_info");

            if (soundCuesheetInfo == null)
            {
                Console.WriteLine("Sound cuesheet info does not exist in input folder, aborting...");
                return;
            }

            DirectoryInfo[] localizedSoundDirs = genDirInf.GetDirectories()
                .Where(x => x.Name.StartsWith("sound"))
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

            //ctg10_hact_y7b1550_amogus
            localizedSoundDirFiles[0] = localizedSoundDirFiles[0]
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

                if(File.Exists(streamGenPath))
                    mainEntry.SetValueFromColumn("have_awb", true);

                for (int i = 1; i < localizedSoundDirFiles.Length; i++)
                {
                    if (localizedSoundDirFiles[i].FirstOrDefault(x => x.Name == fileName) != null)
                    {
                        string haveLocalizedACBColumn = "have" + localizedSoundDirPrefixes[i];
                        mainEntry.SetValueFromColumn(haveLocalizedACBColumn, true);
                    }
                }

                Console.WriteLine("Added " + name + $", category {category}");
            }

            Result = soundCuesheetInfo;
            ArmpFileWriter.WriteARMPToFile(soundCuesheetInfo, Path.Combine(Program.dbPath, "sound_cuesheet_info.bin"));


            string tempCuesheetMapPath = Path.Combine(Program.dbPath, "sound_cuesheet_info_map___TEMP.toberemoved");

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
