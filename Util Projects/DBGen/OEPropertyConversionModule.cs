using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HActLib.Internal;
using HActLib;
using System.Diagnostics;

namespace DBGen
{
    public static class OEPropertyConversionModule
    {
        public static void Procedure()
        {
            string genDir = "motion/gen/property/";
            string genFilePath = genDir + "convertlist.txt";

            string output = "motion/bep/";
            string output2 = new DirectoryInfo("motion/gmt/").FullName;

            if (!File.Exists(genFilePath))
                return;

            Console.WriteLine("------|OE PROPERTY TO DE BEP|-----");

            string[] dat = File.ReadAllLines(genFilePath);
            string property = dat[0];

            if (!File.Exists(property))
            {
                Console.WriteLine("Property.bin does not exist. aborting.");
                return;
            }

            var files = new FileInfo(property).Directory.GetFiles("*.gmt", SearchOption.AllDirectories);

    
            for (int i = 1; i < dat.Length; i++)
            {
                string[] entryDat = dat[i].Split(' ');
                string exportName = "";

                if (entryDat.Length > 1)
                    exportName = entryDat[1] + ".bep";
                else
                    exportName = dat[i] + ".bep";

                string exportPath = Path.Combine(output, exportName);

                if (File.Exists(exportPath))
                {
                    Console.WriteLine(dat[i] + " already exists. ignoring...");
                    continue;
                }

                string gmtName = entryDat[0] + ".gmt";

                BEP converted = OEToDEProperty.ConvertPropertyEntryToDE(property, entryDat[0], Game.YK1, Game.LADIW);
                FileInfo anim = files.FirstOrDefault(x => x.Name == gmtName);

                if (anim != null)
                {
                    char quotationMark = '"';
                    string path1 = $"{quotationMark}{anim.FullName}{quotationMark}";
                    string path2 = $"{quotationMark}{Path.Combine(output2, entryDat[1] + ".gmt")}{quotationMark}";
                    string args = $"-ig y0 -og yk2 -i {path1} -o {path2} -mtn";

                    Console.WriteLine(path1);
                    Console.WriteLine(path2);

                    Process.Start("GMT_Converter.exe", args);
                }

                if(converted != null)
                    BEP.Write(converted, exportPath, Game.LADIW);

                Console.WriteLine("Converted " + entryDat[0]);
            }

            Console.WriteLine("------|OE PROPERTY TO DE BEP COMPLETE|-----");
        }
    }
}
