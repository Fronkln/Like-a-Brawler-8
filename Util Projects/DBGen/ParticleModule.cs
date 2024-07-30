using LibARMP;
using LibARMP.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIBLib;

namespace DBGen
{
    internal static class ParticleModule
    {
        public static Dictionary<string, uint> pibMap = new Dictionary<string, uint>();
        private static List<string> pibPaths = new List<string>();

        public static void Procedure()
        {
            string rootDir = Path.Combine("db_gen.puid");
            string listFile = Path.Combine(rootDir, "particle.txt");

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            if (!Directory.Exists("particle"))
                return;

            Console.WriteLine("------|PARTICLE GEN|-----");

            List<string> list = File.ReadAllLines(listFile).ToList();

            foreach (string str in Directory.GetFiles("particle", "*.pib", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileNameWithoutExtension(str);

                if (!list.Contains(fileName))
                    list.Add(fileName);

                pibPaths.Add(str);
            }

            File.WriteAllLines(listFile, list);

            ARMP particles = Program.GetInputTable("particle");

            foreach(string str in list)
            {
                string pibName = str.Replace(".pib", ""); 

                try
                {
                    var entry = particles.MainTable.GetEntry(pibName);
                    pibMap[pibName] = entry.ID;
                    Console.WriteLine(pibName + " already exists. Skipping");
                }
                catch
                {
                    ArmpEntry entry = particles.MainTable.AddEntry(pibName);
                    pibMap[pibName] = entry.ID;
                    Console.WriteLine("Added " + pibName + ", ID: " + entry.ID);
                }
            }

            foreach(string str in pibPaths)
            {
                BasePib pib = PIB.Read(str);
                pib.ParticleID = particles.MainTable.GetEntry(Path.GetFileNameWithoutExtension(str)).ID;
                PIB.Write(pib, str);
            }

            ArmpFileWriter.WriteARMPToFile(particles, Path.Combine(Program.puidPath, "particle.bin"));
            Console.WriteLine("------|PARTICLE GEN COMPLETE|-----");
        }
    }
}
