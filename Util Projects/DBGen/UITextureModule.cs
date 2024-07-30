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
    internal static class UITextureModule
    {
        public static void Procedure()
        {
            string rootDir = Path.Combine("db_gen.puid");
            string listFile = Path.Combine(rootDir, "ui_texture.txt");

            ARMP uiTexture = Program.GetInputTable("ui_texture");

            if (uiTexture == null)
                return;

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            Console.WriteLine("------|UI TEXTURE GEN|-----");

            foreach (string str in File.ReadLines(listFile))
                uiTexture.MainTable.AddEntry(str);

            ArmpFileWriter.WriteARMPToFile(uiTexture, Path.Combine(Program.puidPath, "ui_texture.bin"));
            Console.WriteLine("------|UI TEXTURE GEN COMPLETE|-----");
        }
    }
}
