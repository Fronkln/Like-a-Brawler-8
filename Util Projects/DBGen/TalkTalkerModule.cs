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
    internal static class TalkTalkerModule
    {
        public static void Procedure()
        {
            Console.WriteLine("------|TALK TALKER GEN|-----");

            string rootDir = Path.Combine(Program.refPath);
            string listFile = Path.Combine(rootDir, "talk_talker.txt");

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            ARMP speaker = Program.GetInputTable("talk_talker");

            if (speaker == null)
                return;

            foreach (string str in File.ReadAllLines(listFile))
            {
                ArmpEntry speakerEntry = speaker.MainTable.AddEntry(str);
                speakerEntry.SetValueFromColumn("talk_talker", str);
            }

            ArmpFileWriter.WriteARMPToFile(speaker, Path.Combine(Program.dbPath, "talk_talker.bin"));
            Console.WriteLine("------|TALK TALKER COMPLETE|-----");
        }
    }
}
