using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HActLib;
using LibARMP;
using LibARMP.IO;

namespace DBGen
{
    internal class Program
    {
        public static string lang = "en";
        public static string refPath = Path.Combine("db_gen");
        public static string refInput = Path.Combine(Environment.CurrentDirectory, refPath, "_input");
        public static string project = "elvis";

        public static bool NoCodename = false;

        public static string dbPath;
        public static string puidPath;

        public static bool isDemo;

        public static string gameOverride = "y8";

        static void Main(string[] args)
        {
            if (args.Length > 0)
                lang = args[0];

            if(args.Length > 1)
                gameOverride = args[1];

            string curDir = Directory.GetCurrentDirectory();
            string dbDir = Directory.GetDirectories(curDir).FirstOrDefault(x => x.Contains("db"));

            if (new DirectoryInfo(dbDir).Name == "db")
                NoCodename = true;

            string codeName = null;

            if (!NoCodename)
                codeName = new DirectoryInfo(dbDir).Name.Split('.')[1];
            else
                codeName = "";

            isDemo = dbDir.EndsWith(".trial");

            Console.WriteLine("DBGen Start");
            Console.WriteLine("Game is " + codeName + $", language is {lang}\n");

            project = codeName;

            string targetDbDir;
            string targetPuidDir;


            if (!NoCodename)
            {
                if (isDemo)
                    targetDbDir = Path.Combine(Directory.GetCurrentDirectory(), $"db.{codeName}.trial"); //db.elvis.trial
                else
                    targetDbDir = Path.Combine(Directory.GetCurrentDirectory(), $"db.{codeName}.{lang}"); //db.elvis.en
            }
            else
            {
                targetDbDir = Path.Combine(Directory.GetCurrentDirectory(), "db");
            }

            if (Directory.Exists(Path.Combine(targetDbDir, lang)))
                targetDbDir = Path.Combine(targetDbDir, lang);

            if(!NoCodename)
                targetPuidDir = Path.Combine(Directory.GetCurrentDirectory(), $"puid.{codeName}"); //puid.elvis
            else
                targetPuidDir = Path.Combine(Directory.GetCurrentDirectory(), $"puid");

            if (!NoCodename)
                refPath = Path.Combine($"db_gen.{lang}");
            else
                refPath = "db_gen";

            refInput = Path.Combine(Environment.CurrentDirectory, refPath, "_input");

            if (!Directory.Exists(targetDbDir))
                Directory.CreateDirectory(targetDbDir);

            if (!Directory.Exists(targetPuidDir))
                Directory.CreateDirectory(targetPuidDir);


            if (!Directory.Exists(refPath))
            {
                Console.WriteLine("Refpath " + refPath + " does not exist.");
                return;
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();

            dbPath = targetDbDir;
            puidPath = targetPuidDir;

            //Particle must get special treatment due to hact adjusting
            //PUIDModule.Procedure("particle", "particle", "*.pib");
            PUIDModule.Procedure("motion_gmt", "motion/gmt", "*.gmt");
            PUIDModule.Procedure("motion_bep", "motion/bep", "*.bep");
            PUIDModule.Procedure("behavior_set", "motion/behavior", "*.mbv");
            PUIDModule.Procedure("ui_texture", $"ui.{codeName}.en", "*.dds"); //always taking EN as basis


            if (Directory.Exists(refInput))
            {
                MotionFlagInfoModule.Procedure();
                Console.WriteLine();
                OEPropertyConversionModule.Procedure();
                Console.WriteLine();
                SoundCuesheetModule.Procedure();
                Console.WriteLine();
                ParticleModule.Procedure();
                Console.WriteLine();
                TalkParamModule.Procedure();

                TalkSelectModule.Procedure();
                Console.WriteLine();
                TalkTalkerModule.Procedure();
                Console.WriteLine();
                TalkModule.Procedure();
                Console.WriteLine();
                UICommonModule.Procedure();
                Console.WriteLine();
                ManualModule.Procedure();
                Console.WriteLine();
                InputActionModule.Procedure();
                Console.WriteLine();
                InputGameStateModule.Procedure();
                BattleCtrlTypeModule.Procedure();
                Console.WriteLine();
                BattleCommandSetModule.Procedure();


                RPGSkillModule.Procedure();
                Console.WriteLine();
                RPGEnemyArtsModule.Procedure();
                Console.WriteLine();
                BattleRPGEnemyModule.Procedure();
                Console.WriteLine();
                SoldierInfoModule.Procedure();
            }

            Console.WriteLine($"\nDBGen completed in: {watch.Elapsed}");
           // Console.ReadKey();
        }

        public static ARMP GetInputTable(string tableName)
        {
            string path = Path.Combine(refInput, tableName + ".bin");

            if (!File.Exists(path))
                return null;

            return ArmpFileReader.ReadARMP(path);
        }

        public static ARMP GetOutputPUIDTable(string tableName)
        {
            string path = "";

            if(!NoCodename)
                path = Path.Combine(("puid." + Program.project), tableName + ".bin");
            else
                path = Path.Combine("puid.", tableName + ".bin");

            return ArmpFileReader.ReadARMP(path);
        }

        public static ARMP GetOutputDBTable(string tableName)
        {
            string path = Path.Combine(dbPath, tableName + ".bin");

            if (!File.Exists(path))
                path = Path.Combine(dbPath, lang, tableName + ".bin");

            return ArmpFileReader.ReadARMP(Path.Combine(dbPath, tableName + ".bin"));
        }

        static void GenerateMotion()
        {
            Console.WriteLine("------|MOTION GEN|-----");

            string motionDir = Path.Combine("motion");
            string gmtDir = Path.Combine(motionDir, "gmt");
            string bepDir = Path.Combine("motion", "bep");


            if (!Directory.Exists(gmtDir))
            {
                Console.WriteLine("GMT folder does not exist. Aborting.");
                return;
            }

            string dbGenFilePath = Path.Combine(gmtDir, "motion_gen.txt");

            if (!File.Exists(dbGenFilePath))
                File.Create(dbGenFilePath).Close();

            string[] motionList = Directory.GetFiles(gmtDir, "*.gmt");
            List<string> motionFile = File.ReadAllLines(dbGenFilePath).ToList();

            ARMP gmtTable = GetInputTable("motion_gmt");
            ARMP bepTable = GetInputTable("motion_bep");

            foreach (string str in motionList)
            {
                string fileName = Path.GetFileNameWithoutExtension(str);

                if (!motionFile.Contains(fileName))
                    motionFile.Add(fileName);
            }

            File.WriteAllLines(dbGenFilePath, motionFile.ToArray());

            foreach (string motion in motionFile)
            {
                string fileName = Path.GetFileNameWithoutExtension(motion);

                string bepFile = Path.Combine(bepDir, fileName + ".bep");
                bool bepExists = File.Exists(bepFile);

                try
                {
                    gmtTable.MainTable.GetEntry(fileName);
                }
                catch
                {
                    gmtTable.MainTable.AddEntry(fileName);
                }

                try
                {
                    if (bepExists)
                        bepTable.MainTable.GetEntry(fileName);
                }
                catch
                {
                    bepTable.MainTable.AddEntry(fileName);
                }

                Console.WriteLine((!bepExists ? "Added GMT" : "Added GMT and BEP") + " for " + motion);
            }

            File.WriteAllLines(Path.Combine(puidPath, "motion_gmt.db_index"), CacheARMP(gmtTable));
            ArmpFileWriter.WriteARMPToFile(gmtTable, Path.Combine(puidPath, "motion_gmt.bin"));
            ArmpFileWriter.WriteARMPToFile(bepTable, Path.Combine(puidPath, "motion_bep.bin"));


            Console.WriteLine("------|MOTION GEN COMPLETE|-----\n");
        }


        public static IEnumerable<string> CacheARMP(ARMP armp)
        {
            Dictionary<uint, string> map = new Dictionary<uint, string>();

            foreach (ArmpEntry entry in armp.MainTable.GetAllEntries())
            {
                if (entry.IsValid)
                    map.Add(entry.ID, entry.Name);
            }

            return map.Select(x => x.Key.ToString() + " " + x.Value.ToString());
        }

        public static IEnumerable<string> CacheSubtableARMP(ARMP armp)
        {
            Dictionary<uint, string> map = new Dictionary<uint, string>();

            foreach (ArmpEntry entry in armp.MainTable.SubTable.GetAllEntries())
                map.Add(Convert.ToUInt32(entry.GetValueFromColumn("2")), entry.Name);

            return map.Select(x => x.Key.ToString() + " " + x.Value.ToString());
        }

        public static string TryGetDBFile(string file)
        {
            return null;
        }
    }
}
