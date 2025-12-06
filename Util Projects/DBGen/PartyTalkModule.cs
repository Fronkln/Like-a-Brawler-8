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
    internal static class PartyTalkModule
    {
        public static void Procedure()
        {
            Console.WriteLine("------|PARTY TALK GEN|-----");

            string rootDir = Path.Combine(Program.refPath, "party_chat");
            string listFile = Path.Combine(rootDir, "list.txt");

            if (!Directory.Exists(rootDir) || !File.Exists(listFile))
                return;

            ARMP pTalk = Program.GetInputTable("talk_party_chat_text_data");
            ARMP pTalkList = Program.GetInputTable("talk_party_chat_list");

            if (pTalk == null || pTalkList == null)
                return;

            List<string> list = File.ReadAllLines(listFile).ToList();

            foreach (string str in new DirectoryInfo(rootDir).GetDirectories().Select(x => x.FullName))
            {
                string name = new DirectoryInfo(str).Name;

                if (!list.Contains(name))
                    list.Add(name);
            }

            File.WriteAllLines(listFile, list);

            foreach (string str in list)
            {
                string path = Path.Combine(rootDir, str);
                string name = new DirectoryInfo(str).Name;
                var mainEntry = pTalkList.MainTable.AddEntry(name);
                ArmpEntry start = null;

                mainEntry.SetValueFromColumn("start_condition", 4);
                mainEntry.SetValueFromColumn("name", "thing");

                if (Directory.Exists(path))
                {
                    string[] texts = Directory.GetFiles(path);

                    for (int i = 0; i < texts.Length; i++)
                    {
                        string[] fileBuff = File.ReadAllLines(texts[i]);
                        string[] split1 = fileBuff[0].Split(' ');
                        var entry = pTalk.MainTable.AddEntry(name + "_" + i.ToString("D3"));
                        entry.SetValueFromColumn("character", byte.Parse(split1[0]));
                        entry.SetValueFromColumn("emote", byte.Parse(split1[1]));

                        if (!string.IsNullOrEmpty(fileBuff[1]))
                        {
                            //cuename
                            string[] cueSplit = fileBuff[1].Split(" ");
                            string sName = cueSplit[0];
                            uint idx = uint.Parse(cueSplit[1]);

                            if (SoundCuesheetModule.Result != null)
                                entry.SetValueFromColumn("cuesheet_name", SoundCuesheetModule.Result.MainTable.SubTable.GetEntry(sName).ID + 1);

                            entry.SetValueFromColumn("cue_name", (ushort)idx);
                        }

                        string text = "";

                        for (int k = 2; k < fileBuff.Length; k++)
                        {
                            text += fileBuff[k];
                        }

                        entry.SetValueFromColumn("text", text);

                        if (start == null)
                            start = entry;
                    }
                }

                if (start != null)
                {
                    mainEntry.SetValueFromColumn("text_id", (ushort)start.ID);
                }

                mainEntry.SetValueFromColumn("is_no_check_main_story", true);
                var endEntry = pTalk.MainTable.AddEntry(name + "_999");
                endEntry.SetValueFromColumn("character", 5);
            }



            ArmpFileWriter.WriteARMPToFile(pTalk, Path.Combine(Program.dbPath, "talk_party_chat_text_data.bin"));
            ArmpFileWriter.WriteARMPToFile(pTalkList, Path.Combine(Program.dbPath, "talk_party_chat_list.bin"));
            Console.WriteLine("------|PARTY TALK OVER|-----");
        }
    }
}
