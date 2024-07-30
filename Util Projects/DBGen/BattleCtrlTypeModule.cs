using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LibARMP;
using LibARMP.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DBGen
{
    internal static class BattleCtrlTypeModule
    {
        public static void Procedure()
        {
            string rootDir = Path.Combine(Program.refPath, "ctrltype");
            string listFile = Path.Combine(rootDir, "list.txt");

            if (!Directory.Exists(rootDir))
                return;

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            ARMP ctrlTypeBin = Program.GetInputTable("battle_ctrltype");

            if (ctrlTypeBin == null)
                return;

            List<string> list = File.ReadAllLines(listFile).ToList();

            foreach (string str in Directory.GetFiles(rootDir, "*.cfe"))
            {
                string str2 = str.Replace(rootDir + @"\", "");

                if (!list.Contains(str2))
                    list.Add(str2);
            }

            File.WriteAllLines(listFile, list);

            JObject file = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(@"battle\cfc\File Information.json"));
            var cfcList = file["File Specific Info"]["Set Order"].ToArray().Cast<JProperty>();

            Dictionary<string, byte> m_cfcDict = new Dictionary<string, byte>();
            
            foreach(var cfc in cfcList)
            {
                m_cfcDict.Add(cfc.Name, (byte)cfc.Last);
            }

            foreach(string cfeFile in list)
            {
                BattleCtrlTypeEntry cfe = JsonConvert.DeserializeObject<BattleCtrlTypeEntry>(File.ReadAllText(rootDir + @"\" + cfeFile));
                ArmpEntry entry = ctrlTypeBin.MainTable.AddEntry(Path.GetFileNameWithoutExtension(cfeFile));
                entry.SetValueFromColumn("command_set", m_cfcDict[cfe.CommandSet]);
                entry.SetValueFromColumn("is_boss", cfe.IsBoss);
                entry.SetValueFromColumn("chara_id", cfe.CharacterID);
                entry.SetValueFromColumn("fighter_type", cfe.FighterType);
                entry.SetValueFromColumn("is_npc", cfe.IsNPC);
            }

            ArmpFileWriter.WriteARMPToFile(ctrlTypeBin, Path.Combine(Program.dbPath, "battle_ctrltype.bin"));
        }
    }
}
