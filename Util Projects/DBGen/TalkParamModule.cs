using System;
using System.Collections.Generic;
using System.Linq;
using LibARMP;
using LibARMP.IO;
using System.IO;
using HActLib;
using System.ComponentModel.DataAnnotations;

namespace DBGen
{
    internal static class TalkParamModule
    {
        //adjust references
        public static bool ComplexMode = true;

        public static void Procedure()
        {

                //false in Y6, only adjusts
            bool canAdd = true;
            bool isRepackGame = false;

            ARMP talkParamBin = Program.GetInputTable("talk_param");

            if (talkParamBin == null)
                canAdd = false;

            Console.WriteLine("------|TALK PARAM GEN|-----");

            string genFilePath = "hact_yazawa/hact_gen.txt";

            if (File.Exists(genFilePath))
                isRepackGame = true;

            string hactSrcDir = "";

            if (isRepackGame)
                hactSrcDir = "hact_yazawa";
            else
            {
                if (Program.NoCodename)
                    hactSrcDir = "hact";
                else
                    hactSrcDir = "hact." + Program.project;
            }


            if (!Directory.Exists(hactSrcDir))
                return;

            List<string> genFileDat = null;


            if (canAdd)
            {
                if (!File.Exists(genFilePath))
                    File.Create(genFilePath).Close();

                genFileDat = File.ReadAllLines(genFilePath).ToList();
            }

            foreach (string hactDir in Directory.GetDirectories(hactSrcDir))
            {
                string dirName = new DirectoryInfo(hactDir).Name;

                if (canAdd)
                {
                    try
                    {
                        talkParamBin.GetMainTable().GetEntry(dirName);
                        continue;
                    }
                    catch
                    {
                    }

                    if (genFileDat.Contains(dirName))
                        continue;
                    else
                    {
                        string metadatPath = Path.Combine(hactDir, "metadata.txt");

                        if (!File.Exists(metadatPath))
                            File.WriteAllText(metadatPath, "9"); //type

                        genFileDat.Add(dirName);
                        Console.WriteLine("Added " + hactDir);
                    }


                    File.WriteAllLines(genFilePath, genFileDat.ToArray());
                }
            }

            if (!canAdd)
            {
                genFileDat = Directory.GetDirectories(hactSrcDir).ToList();

                if (Directory.Exists("auth"))
                    genFileDat.AddRange(Directory.GetDirectories("auth"));
            }

            string[] auths = new string[0];

            if (Directory.Exists("auth"))
                auths = Directory.GetDirectories("auth");

            foreach (string str in genFileDat)
            {
                string hactDir = null;
                string metaDataPath = null;


                hactDir = Path.Combine(hactSrcDir, str);

                if (canAdd)
                {
                    try
                    {
                        talkParamBin.GetMainTable().GetEntry(str);
                        Console.WriteLine(str + " already exists in talk param bin, skipping...");
                        continue;
                    }
                    catch
                    {
                    }
                    metaDataPath = Path.Combine(hactDir, "metadata.txt");
                }

                string cmnPath = Path.Combine(hactDir, "cmn", "cmn.bin");

                if (canAdd)
                {
                    ArmpEntry entry = null;
                    if (!talkParamBin.GetMainTable().TryGetEntry(str, out entry))
                    {
                        ArmpEntry talkEntry = talkParamBin.GetMainTable().AddEntry(str);
                        try
                        {
                            string path = "hact";

                            if (!Program.NoCodename)
                                path += "_" + Program.project;

                            path += "/";

                            talkEntry.SetValueFromColumn("path", path + str);
                            talkEntry.SetValueFromColumn("type", byte.Parse(File.ReadAllText(metaDataPath)));
                        }
                        catch
                        {

                        }

                        Console.WriteLine($"Added {str}, ID: {talkEntry.ID}");
                    }
                }

                bool dirty = false;

                if(ComplexMode)
                    AdjustHAct(cmnPath, str);

                if(dirty)
                {

                }
            }

            foreach(string str in auths)
            {
                string cmnPath = Path.Combine(str, "cmn", "cmn.bin");

                if(ComplexMode)
                    AdjustHAct(cmnPath, str);
            }

            if (canAdd)
            {
                ArmpFileWriter.WriteARMPToFile(talkParamBin, Path.Combine(Program.dbPath, "talk_param.bin"));
                File.WriteAllLines(Path.Combine(Program.dbPath, "talk_param.db_index"), Program.CacheARMP(talkParamBin));
            }

            Console.WriteLine("------|TALK PARAM GEN COMPLETE|-----");
        }

        private static void AdjustHAct(string cmnPath, string str)
        {
            bool dirty = false;

            if (!File.Exists(cmnPath))
                return;

            if (!ComplexMode)
                return;

            CMN hact = CMN.Read(cmnPath, Program.Game);
            
            if (AdjustSound(hact, str))
                dirty = true;
            if (AdjustPib(hact, str))
                dirty = true;

            if(dirty)
            {
                CMN.Write(hact, cmnPath);
            }
        }

        private static bool AdjustSound(CMN hact, string name)
        {
            if (SoundCuesheetModule.Result == null)
                return false;

            bool dirty = false;

            string str = name;

            string findName = str.Substring(0, (str.Length >= 8 ? 7 : str.Length));
            NodeElement[] soundNodes = hact.AllElements.Where(x => x.Name.ToLowerInvariant().Contains(findName)).ToArray();

            ushort newCuesheetID = 0;

            if (soundNodes.Length > 0)
            {
                string nameToFind = str.ToLowerInvariant();

                if (!nameToFind.StartsWith("hact"))
                    nameToFind = "hact_" + nameToFind;

                ArmpEntry cuesheetEntry = SoundCuesheetModule.Result.GetMainTable().GetAllEntries().FirstOrDefault(x => x.GetValueFromColumn("name").ToString().Contains(nameToFind));

                if (cuesheetEntry != null)
                {
                    newCuesheetID = ((ushort)cuesheetEntry.GetValueFromColumn("*cuesheet_id"));

                    foreach (NodeElement soundNode in soundNodes)
                    {
                        DEElementSE se = soundNode as DEElementSE;

                        if (se != null)
                        {
                            if (se.CueSheet != newCuesheetID)
                            {
                                se.CueSheet = newCuesheetID;
                                dirty = true;
                            }
                        }
                    }
                    Console.WriteLine("Adjusted hact cuesheet IDs for " + str);
                }
            }

            return dirty;
        }

        private static bool AdjustPib(CMN hact, string name)
        {
            ARMP particlePUID = Program.GetOutputPUIDTable("particle");

            if (particlePUID == null)
                return false;

            DEElementParticle[] particleNodes = hact.AllElements.Where(x => x is DEElementParticle).Cast<DEElementParticle>().ToArray();
            GameVersion hactVer = hact.GameVersion;

            bool dirty = false;

            foreach(DEElementParticle particle in particleNodes)
            {
                string ptcName = "";

                if (hactVer < GameVersion.DE1)
                    ptcName = particle.Name.Substring(0, 7);
                else
                    ptcName = particle.ParticleName;

                uint newID = 0;

                try
                {
                    newID = ParticleModule.pibMap[ptcName];
                }
                catch
                {
                    ArmpEntry foundEntry = null;
                    bool found = particlePUID.GetMainTable().TryGetEntry(ptcName, out foundEntry);

                    if (found)
                        newID = foundEntry.ID;
                }

                if (newID <= 0)
                    continue;

                if(particle.ParticleID != newID)
                {
                    dirty = true;
                    particle.ParticleID = newID;
                }    
            }


            return dirty;
        }
    }
}
