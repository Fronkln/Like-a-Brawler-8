using LibARMP;
using LibARMP.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace DBGen
{
    public static class BattleRPGEnemyModule
    {
        public static void Procedure()
        {
            string rootDir = Path.Combine(Program.refPath, "enemy");
            string listFile = Path.Combine(rootDir, "list.txt");

            if (!Directory.Exists(rootDir))
                return;

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            ARMP battleRpgEnemy = Program.GetInputTable("battle_rpg_enemy");

            if (battleRpgEnemy == null)
                return;

            List<string> list = File.ReadAllLines(listFile).ToList();

            foreach (string str in Directory.GetDirectories(rootDir))
            {
                string str2 = str.Replace(rootDir + @"\", "");

                if (!list.Contains(str2))
                    if (!string.IsNullOrEmpty(str2))
                        list.Add(str2);
            }

            File.WriteAllLines(listFile, list);

            ARMP battleRpgEnemyArts = Program.GetOutputDBTable("rpg_enemy_arts_data");
            ARMP battleCtrlType = Program.GetOutputDBTable("battle_ctrltype");

            foreach(string str in list)
            {
                string str2 = rootDir + @"\" + str + @"\";
                string enemyFilePath = Path.Combine(str2, "enemy.dat");
                BattleRPGEnemyEntry enemyData = JsonConvert.DeserializeObject<BattleRPGEnemyEntry>(File.ReadAllText(enemyFilePath));

                ArmpEntry newEntry = battleRpgEnemy.MainTable.AddEntry(new DirectoryInfo(str2).Name);
                newEntry.SetValueFromColumn("name", enemyData.Name);
                newEntry.SetValueFromColumn("ctrltype", (ushort)battleCtrlType.MainTable.GetEntry(enemyData.CtrlType).ID);
                newEntry.SetValueFromColumn("arts", Convert.ToUInt16(battleRpgEnemyArts.MainTable.SubTable.GetEntry(enemyData.Arts).GetValueFromColumn("0")));
                newEntry.SetValueFromColumn("model", enemyData.Model);
                newEntry.SetValueFromColumn("position_ai", (byte)20);
                newEntry.SetValueFromColumn("equip_l", enemyData.EquipL);
                newEntry.SetValueFromColumn("equip_r", enemyData.EquipR);
            }

            File.WriteAllLines(Path.Combine(Program.dbPath, "battle_rpg_enemy.db_index"), Program.CacheARMP(battleRpgEnemy));
            ArmpFileWriter.WriteARMPToFile(battleRpgEnemy, Path.Combine(Program.dbPath, "battle_rpg_enemy.bin")); 
        }
    }
}
