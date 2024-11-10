using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElvisCommand
{
    public class EHC
    {
        //Start of Elvis Command is Version 11
        public const uint VERSION = 13;

        public List<HeatActionAttack> Attacks = new List<HeatActionAttack>();

        public static void Write(EHC ehc, string filePath)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write("JHRINO_YHC".ToCharArray());
            writer.Write(new byte[2]);
            writer.Write(VERSION);

            writer.Write(ehc.Attacks.Count);
            writer.Write(new byte[12]);

            foreach (HeatActionAttack atk in ehc.Attacks)
            {
                atk.Write(writer);
            }

            File.WriteAllBytes(filePath, ms.ToArray());
        }

        public static EHC Read(string path)
        {
            if (!File.Exists(path))
                return null;

            EHC ehc = new EHC();

            try
            {
                BinaryReader reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(path)));

                string magic = Encoding.UTF8.GetString(reader.ReadBytes(10));

                if (magic != "JHRINO_YHC")
                    return null;

                reader.BaseStream.Position += 2;
                int version = reader.ReadInt32();

                if (version > VERSION)
                    return null;

                int attacksCount = reader.ReadInt32();
                reader.BaseStream.Position += 12;

                for (int i = 0; i < attacksCount; i++)
                {
                    HeatActionAttack attack = new HeatActionAttack();
                    attack.Read(reader, (uint)version);
                    ehc.Attacks.Add(attack);
                }

                return ehc;
            }
            catch
            {
                return null;
            }
        }
    }
}
