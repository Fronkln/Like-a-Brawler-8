using LibARMP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LibARMP.IO;

namespace DBGen
{
    public static class RPGEnemyArtsModule
    {
        public static void Procedure()
        {
            Console.WriteLine("------|RPG ENEMY ARTS GEN|-----");

            string rootDir = Path.Combine(Program.refPath, "enemy");
            string listFile = Path.Combine(rootDir, "list.txt");

            ARMP rpgEnemyArtsData = Program.GetInputTable("rpg_enemy_arts_data");

            if (rpgEnemyArtsData == null)
                return;

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            List<string> list = File.ReadAllLines(listFile).ToList();

            ARMP rpgSkillData = Program.GetOutputDBTable("rpg_skill");
            ARMP rpgEnemyArtsType = Program.GetInputTable("rpg_enemy_arts_type");

            foreach (string str in Directory.GetDirectories(rootDir))
            {
                string str2 = str.Replace(rootDir + @"\", "");

                if (!list.Contains(str2))
                    if(!string.IsNullOrEmpty(str2))
                        list.Add(str2);
            }

            File.WriteAllLines(listFile, list);

            foreach (string str in list)
            {
                string str2 = rootDir + @"\" + str;

                string[] attacksList = Directory.GetFiles(str2, "*.txt");
                attacksList = attacksList.OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x))).ToArray();


                List<RPGEnemyArtsEntry> attacksDat = new List<RPGEnemyArtsEntry>();
                List<ArmpEntry> attacksEntryMain = new List<ArmpEntry>();

                ArmpEntry enemyEntry = rpgEnemyArtsData.MainTable.SubTable.AddEntry(new DirectoryInfo(str2).Name);
                rpgEnemyArtsType.MainTable.AddEntry(enemyEntry.Name);

                List<ArmpEntry> entries = rpgEnemyArtsData.MainTable.SubTable.GetAllEntries();

                uint last = (uint)entries[entries.Count - 2].GetValueFromColumn("0");

                enemyEntry.SetValueFromColumn("0", last + 1);
                ArmpTable armp = ((ArmpTableMain)(entries[0].GetValueFromColumn("1"))).Copy(false);


                ushort ID = (ushort)((uint)enemyEntry.GetValueFromColumn("0"));

                for (int i = 0; i < attacksList.Length; i++)
                    attacksDat.Add(JsonConvert.DeserializeObject<RPGEnemyArtsEntry>(File.ReadAllText(attacksList[i])));


                for(int i = 0; i < attacksDat.Count; i++)
                {
                    ArmpEntry entry = rpgEnemyArtsData.MainTable.AddEntry();
                    RPGEnemyArtsEntry data = attacksDat[i];
                    entry.SetValueFromColumn("rate", data.Rate);
                    entry.SetValueFromColumn("skill", (ushort)rpgSkillData.MainTable.GetEntry(data.Skill).ID);
                    entry.SetValueFromColumn("*id", ID);
                    entry.SetValueFromColumn("**idx", (uint)(i + 1));

                    attacksEntryMain.Add(entry);
                }

                for (int i = 0; i < attacksEntryMain.Count; i++)
                {
                    ArmpEntry skillTblEntry = armp.AddEntry((i + 1).ToString());
                    skillTblEntry.SetValueFromColumn("0", (uint)i + 1);
                    skillTblEntry.SetValueFromColumn("2", (uint)attacksEntryMain[i].ID);
                }

                enemyEntry.SetValueFromColumn("1", (ArmpTableMain)armp);
                enemyEntry.SetValueFromColumn("2", (uint)attacksEntryMain[0].ID);

                Console.WriteLine("Added " + enemyEntry.Name);
            }

            ArmpFileWriter.WriteARMPToFile(rpgEnemyArtsData, Path.Combine(Program.dbPath, "rpg_enemy_arts_data.bin"));
            ArmpFileWriter.WriteARMPToFile(rpgEnemyArtsType, Path.Combine(Program.dbPath, "rpg_enemy_arts_type.bin"));

            Console.WriteLine("------|RPG ENEMY ARTS GEN COMPLETE|-----");
        }
    }
}
