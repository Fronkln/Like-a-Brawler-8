using LibARMP;
using LibARMP.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBGen
{
    internal static class PUIDModule
    {
        public static void Procedure(string puidName, string folder, string ext = "*.*")
        {
            string rootDir = Path.Combine("db_gen.puid");

            if (!Directory.Exists(rootDir))
            {
                Console.WriteLine("PUID dir does not exist. Skipping PUID " + puidName);
                return;
            }

            ARMP table = Program.GetInputTable(puidName);

            if (table == null)
            {
                Console.WriteLine("PUID Input table does not exist. skipping PUID " + puidName);
                return;
            }

            string listFile = Path.Combine(rootDir, $"{puidName}.txt");

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            List<string> list = File.ReadAllLines(listFile).ToList();

            foreach (string str in Directory.GetFiles(folder, ext, SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileNameWithoutExtension(str);

                if (!list.Contains(fileName))
                    list.Add(fileName);
            }

            File.WriteAllLines(listFile, list);

            Console.WriteLine($"------|{puidName.Replace('_', ' ').ToUpperInvariant()} GEN|-----");

            foreach (string str in list)
            {
                bool contains = false;

                foreach (ArmpEntry entrya in table.MainTable.GetAllEntries())
                {
                    if (entrya.Name == str)
                    {
                        contains = true;
                        break;
                    }
                }

                if (!contains)
                {
                    ArmpEntry entry = table.MainTable.AddEntry(str);
                    Console.WriteLine("Added " + str + ", ID: " + entry.ID);
                }
            }


            ArmpFileWriter.WriteARMPToFile(table, Path.Combine(Program.puidPath, puidName + ".bin"));
            Console.WriteLine($"------|{puidName.Replace('_', ' ').ToUpperInvariant()} COMPLETE|-----");
        }
    }
}
