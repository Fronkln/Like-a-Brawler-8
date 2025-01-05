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
    internal static class TalkModule
    {
        public static void Procedure()
        {
            string rootDir = Path.Combine(Program.refPath, "talk");
            string listFile = Path.Combine(rootDir, "list.txt");

            if (!Directory.Exists(rootDir))
                return;

            ARMP talk = Program.GetInputTable("talk");

            if (talk == null)
                return;

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            Console.WriteLine("------|TALK GEN|-----");

            List<string> list = File.ReadAllLines(listFile).ToList();

            foreach (string str in new DirectoryInfo(rootDir).GetDirectories().Select(x => x.FullName))
            {
                string name = new DirectoryInfo(str).Name;

                if (!list.Contains(name))
                    list.Add(name);
            }

            File.WriteAllLines(listFile, list);

            ARMP talkSpeaker = Program.GetOutputDBTable("talk_talker");

            foreach (string str in list)
            {
                string dir = Path.Combine(rootDir, str);

                ArmpTable table = ((ArmpTableMain)talk.MainTable.GetEntry(1).GetValueFromColumn("text")).Copy(false);
                ArmpEntry tableEntry = talk.MainTable.AddEntry(new DirectoryInfo(dir).Name);
                tableEntry.SetValueFromColumn("text", table);


                foreach (string file in new DirectoryInfo(dir).GetFiles("*.txt")
                        .Select(x => x.FullName)
                        .OrderBy(x => uint.Parse(Path.GetFileNameWithoutExtension(x))))
                {
                    string[] split = File.ReadAllLines(file);

                    string speaker = split[0];
                    StringBuilder text = new StringBuilder();

                    for (int i = 1; i < split.Length; i++)
                        text.AppendLine(split[i]);

                    ushort speakerID = 0;

                    if (!string.IsNullOrEmpty(speaker))
                        speakerID = (ushort)talkSpeaker.MainTable.GetEntry(speaker).ID;

                    ArmpEntry tableTextEntry = table.AddEntry();
                    tableTextEntry.SetValueFromColumn("1", speakerID);
                    tableTextEntry.SetValueFromColumn("2", text.ToString());
                    tableTextEntry.SetValueFromColumn("8", (float)125);
                }
            }

            ArmpFileWriter.WriteARMPToFile(talk, Path.Combine(Program.dbPath, "talk.bin"));
            Console.WriteLine("------|TALK GEN COMPLETE|-----");
        }
    }
}
