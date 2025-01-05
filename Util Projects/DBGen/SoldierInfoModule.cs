using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibARMP;
using LibARMP.IO;
using Newtonsoft.Json;

namespace DBGen
{
    internal class SoldierInfoModule
    {
        private static ARMP battleRpgEnemy;

        public static void Procedure()
        {
            System.Diagnostics.Stopwatch time = new System.Diagnostics.Stopwatch();

            string rootDir = Path.Combine(Program.refPath, "soldier");
            string listFile = Path.Combine(rootDir, "list.txt");

            if (!Directory.Exists(rootDir))
                return;

            ARMP soldierInfoArmp = Program.GetInputTable("character_npc_soldier_personal_data");

            if (soldierInfoArmp == null)
                return;

            Console.WriteLine("------|SOLDIER INFO GEN|-----");

            if (!File.Exists(listFile))
                File.Create(listFile).Close();

            List<string> list = File.ReadAllLines(listFile).ToList();

            foreach (string str in Directory.GetDirectories(rootDir))
            {
                string str2 = str.Replace(rootDir + @"\", "");

                if (!list.Contains(str2))
                    if (!string.IsNullOrEmpty(str2))
                        list.Add(str2);
            }

            File.WriteAllLines(listFile, list);
            battleRpgEnemy = Program.GetOutputDBTable("battle_rpg_enemy");

            foreach(string str in list)
            {
                string str2 = rootDir + @"\" + str + @"\";
                string soldierFile = Path.Combine(str2, "soldier.dat");
                SoldierInfoEntry soldierData = JsonConvert.DeserializeObject<SoldierInfoEntry>(File.ReadAllText(soldierFile));

                ArmpEntry entry = null;

                if (string.IsNullOrEmpty(soldierData.IDOverride))
                {
                    entry = soldierInfoArmp.MainTable.AddEntry(new DirectoryInfo(str2).Name);
                    SetSoldierDat(soldierData, entry);
                    Console.WriteLine("Added soldier entry " + entry.Name + $"({entry.ID})");
                }
                else
                {
                    foreach(ArmpEntry armp in soldierInfoArmp.MainTable.GetAllEntries())
                    {
                        if (armp.Name == soldierData.IDOverride)
                        {
                            SetSoldierDat(soldierData, armp);
                            Console.WriteLine("Updated soldier entry " + armp.Name + $"({armp.ID})");
                        }
                    }
                }

            }

            time.Start();

            ArmpFileWriter.WriteARMPToFile(soldierInfoArmp, Path.Combine(Program.dbPath, "character_npc_soldier_personal_data.bin"));
            File.WriteAllLines(Path.Combine(Program.dbPath, "character_npc_soldier_personal_data.db_index"), Program.CacheARMP(soldierInfoArmp));

            time.Stop();

            Console.WriteLine($"------|SOLDIER INFO GEN COMPLETE IN {time.Elapsed}|-----");
        }

        private static void SetSoldierDat(SoldierInfoEntry soldierData, ArmpEntry entry)
        {
            entry.SetValueFromColumn("enemy_id", (ushort)battleRpgEnemy.MainTable.GetEntry(soldierData.EnemyID).ID);
            entry.SetValueFromColumn("life_gauge_type", soldierData.LifeGaugeType);
            entry.SetValueFromColumn("force_kind", soldierData.ForceKind);
            entry.SetValueFromColumn("no_sujimon", soldierData.NoSujimon);
            entry.SetValueFromColumn("hp", soldierData.Health);
            entry.SetValueFromColumn("hp_ratio", soldierData.HPRatio);
            entry.SetValueFromColumn("attack", soldierData.Attack);
            entry.SetValueFromColumn("defence", soldierData.Defense);
        }
    }
}
