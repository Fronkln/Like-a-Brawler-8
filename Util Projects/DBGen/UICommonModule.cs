using LibARMP;
using LibARMP.IO;
using System;
using System.IO;


namespace DBGen
{
    internal static class UICommonModule
    {
        public static void Procedure()
        {
            string rootDir = Path.Combine("db_gen.puid");
            string listFile = Path.Combine(rootDir, "ui_texture_common.txt");

            ARMP uiCommonTexture = Program.GetInputTable("ui_texture_common");

            if (uiCommonTexture == null)
                return;

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            ARMP uiTexture = Program.GetOutputPUIDTable("ui_texture");

            Console.WriteLine("------|UI TEXTURE COMMON GEN|-----");

            foreach (string str in File.ReadLines(listFile))
            {
                ArmpEntry commonTex = uiCommonTexture.MainTable.AddEntry();
                commonTex.SetValueFromColumn("*", (ushort)uiTexture.MainTable.GetEntry(str).ID);
            }

            ArmpFileWriter.WriteARMPToFile(uiCommonTexture, Path.Combine(Program.dbPath, "ui_texture_common.bin"));
            Console.WriteLine("------|UI TEXTURE COMMON GEN COMPLETE|-----");
        }
    }
}
