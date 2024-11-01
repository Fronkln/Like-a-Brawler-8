using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace LikeABrawler2
{
    public unsafe static class DBManager
    {
        //private static Dictionary<string, uint> m_motionGmt = new Dictionary<string, uint>();
        private static Dictionary<string, uint> m_soundCuesheet = new Dictionary<string, uint>();
        private static Dictionary<string, uint> m_rpgSkill = new Dictionary<string, uint>();
        private static Dictionary<string, uint> m_talkParam = new Dictionary<string, uint>();
        private static Dictionary<string, uint> m_soldierInfo = new Dictionary<string, uint>();
        private static Dictionary<uint, string> m_soldierInfoIDs = new Dictionary<uint, string>();
        private static Dictionary<uint, string> m_commandSets = new Dictionary<uint, string>();
        private static Dictionary<string, uint> m_commandSets2 = new Dictionary<string, uint>();
        private static Dictionary<uint, string> m_rpgEnemy = new Dictionary<uint, string>();

        private static Dictionary<uint, EnemyRebalanceEntry> Rebalances = new Dictionary<uint, EnemyRebalanceEntry>();


        public static bool Inited = false;

        public static void Init()
        {
            Inited = true;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            DragonEngine.Log("Starting DB init.");

            //m_motionGmt = ReadCachedPuidArmp("motion_gmt");
            m_commandSets2 = ReadCachedDBArmp("battle_command_set");
            m_commandSets = FlipCachedDBArmp(m_commandSets2);
            m_talkParam = ReadCachedDBArmp("talk_param");
            m_rpgSkill = ReadCachedDBArmp("rpg_skill");
            m_soldierInfo = ReadCachedDBArmp("character_npc_soldier_personal_data");
            m_rpgEnemy = FlipCachedDBArmp(ReadCachedDBArmp("battle_rpg_enemy"));
            //BattleRpgEnemy = ReadCachedDBArmp("battle_rpg_enemy");

            foreach (var kv in m_soldierInfo)
                m_soldierInfoIDs.Add(kv.Value, kv.Key);

            //:b:roke
            //m_soundCuesheet = CacheARMPBySubtableU16(GetARMP(DB.GetBinaryPointer(67)));


            //TEMP!
            foreach (string str in File.ReadAllLines(Path.Combine(Mod.ModPath, "db.elvis.en/sound_cuesheet_info_map___TEMP.toberemoved")))
            {
                string[] split = str.Split(' ');
                m_soundCuesheet.Add(split[1], uint.Parse(split[0]));
            }

            /*
            foreach(ArmpEntry entry in soundCuesheet.MainTable.GetAllEntries())
            {
                m_soundCuesheet[(string)entry.GetValueFromColumn("name")] = (ushort)entry.GetValueFromColumn("*cuesheet_id");
            }
            */


            string rebalancePath = Path.Combine(Mod.ModPath, "mdb.brawler", "soldier_rebalance");
            
            if(Directory.Exists(rebalancePath))
            {
                foreach(string file in Directory.GetFiles(rebalancePath, "*.txt"))
                {
                    string soldierName = Path.GetFileNameWithoutExtension(file);
                    uint id = GetSoldier(soldierName);

                    if(id == 0)
                    {
                        DragonEngine.Log("REBALANCE ENTRY: " + soldierName + " does not exist.");
                        continue;
                    }

                    Rebalances[id] = JsonConvert.DeserializeObject<EnemyRebalanceEntry>(File.ReadAllText(file));
                }
            }

            stopwatch.Stop();
            DragonEngine.Log("DB Init complete, took " + stopwatch.Elapsed.TotalSeconds + " seconds.");

        }


        public static TalkParamID GetTalkParam(string name)
        {
            if (!m_talkParam.ContainsKey(name))
                return TalkParamID.invalid;
            else
                return (TalkParamID)m_talkParam[name];
        }

        public static RPGSkillID GetSkill(string skillName)
        {
            if (!m_rpgSkill.ContainsKey(skillName))
                return RPGSkillID.invalid;

            return (RPGSkillID)m_rpgSkill[skillName];
        }

        /*
        public static uint GetMotion(string motionName)
        {
            if (!m_motionGmt.ContainsKey(motionName))
                return m_motionGmt["test_dance"];

            return m_motionGmt[motionName];
        }
        */

        public static ushort GetSoundCuesheet(string name)
        {
            if (!m_soundCuesheet.ContainsKey(name))
                return 0;

            return (ushort)m_soundCuesheet[name];
        }

        public static uint GetSoldier(string name)
        {
            if (!m_soldierInfo.ContainsKey(name))
                return 0;

            return m_soldierInfo[name];
        }

        public static string GetSoldier(uint id)
        {
            if (!m_soldierInfoIDs.ContainsKey(id))
                return null;

            return m_soldierInfoIDs[id];
        }

        public static string GetRPGEnemy(uint id)
        {
            if (!m_rpgEnemy.ContainsKey(id))
                return null;

            return m_rpgEnemy[id];
        }

        public static uint GetCommandSet(string name)
        {
            if (!m_commandSets2.ContainsKey(name))
                return 0;

            return m_commandSets2[name];
        }

        public static string GetCommandSet(uint id)
        {
            if (!m_commandSets.ContainsKey(id))
                return null;

            return m_commandSets[id];
        }


        public static EnemyRebalanceEntry GetSoldierRebalance(uint id)
        {
            if (id == 0)
                return null;

            if (!Rebalances.ContainsKey(id))
                return null;

            return Rebalances[id];
        }

        public static EnemyRebalanceEntry GetSoldierRebalance(string name)
        {
            uint id = GetSoldier(name);

            if (id == 0)
                return null;

            if (!Rebalances.ContainsKey(id))
                return null;

            return Rebalances[id];
        }

        /*
        public static ARMP GetARMP(IntPtr pointer, int bufferSize = 4194304)
        {
            byte[] buffer = new byte[4194304];
            Marshal.Copy(pointer, buffer, 0, bufferSize);

            try
            {
                return ArmpFileReader.ReadARMP(buffer, baseARMPMemoryAddress: pointer);
            }
            catch
            {
                throw new Exception("Armp read error");
            }
        }

        public static ARMP GetARMPDB(uint id, int bufferSize = 4194304)
        {
            return null;
        }
        */


        private static Dictionary<string, uint> ReadCachedDBArmp(string name)
        {
            string file = Path.Combine(Mod.ModPath, "db.elvis.en/" + name + ".db_index");

            if (!File.Exists(file))
                throw new Exception("DB Cache file does not exist: " + file);

            Dictionary<string, uint> list = new Dictionary<string, uint>();

            foreach(string str in File.ReadAllLines(file))
            {
                string[] parts = str.Split(' ');

                uint id = uint.Parse(parts[0]);

                if (parts.Length == 1)
                    continue;

                list[parts[1]] = id;
            }

            return list;
        }

        private static Dictionary<string, uint> ReadCachedPuidArmp(string name)
        {
            string file = Path.Combine(Mod.ModPath, "puid.elvis/" + name + ".db_index");

            if (!File.Exists(file))
                throw new Exception("DB Cache file does not exist: " + file);

            Dictionary<string, uint> list = new Dictionary<string, uint>();

            foreach (string str in File.ReadAllLines(file))
            {
                string[] parts = str.Split(' ');

                uint id = uint.Parse(parts[0]);

                if (parts.Length == 1)
                    continue;

                list[parts[1]] = id;
            }

            return list;
        }

        private static Dictionary<uint, string> FlipCachedDBArmp(Dictionary<string, uint> list)
        {
            return list.ToDictionary(x => x.Value, x => x.Key);
        }

        /*
        private static Dictionary<string, uint> CacheARMP(ARMP armp)
        {
            Dictionary<string, uint> cache = new Dictionary<string, uint>();
            List<ArmpEntry> uniqueEntries = armp.MainTable.GetAllEntries().GroupBy(x => x.Name).Select(x => x.FirstOrDefault()).Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            foreach (ArmpEntry entry in uniqueEntries)
                cache[entry.Name] = (uint)entry.ID;

            return cache;
        }

        private static Dictionary<uint, string> CacheARMP2(ARMP armp)
        {
            Dictionary<uint, string> cache = new Dictionary<uint, string>();
            List<ArmpEntry> uniqueEntries = armp.MainTable.GetAllEntries().GroupBy(x => x.Name).Select(x => x.FirstOrDefault()).Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            foreach (ArmpEntry entry in uniqueEntries)
                cache[entry.ID] = entry.Name;

            return cache;
        }

        private static Dictionary<string, uint> CacheARMPBySubtable(ARMP armp, string column = "0")
        {
            Dictionary<string, uint> cache = new Dictionary<string, uint>();
            List<ArmpEntry> uniqueEntries = armp.MainTable.SubTable.GetAllEntries().GroupBy(x => x.Name).Select(x => x.FirstOrDefault()).Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            foreach (ArmpEntry entry in uniqueEntries)
                cache[entry.Name] = (uint)entry.GetValueFromColumn(column);

            return cache;
        }

        private static Dictionary<uint, string> CacheARMPBySubtable2(ARMP armp, string column = "0")
        {
            Dictionary<uint, string> cache = new Dictionary<uint, string>();
            List<ArmpEntry> uniqueEntries = armp.MainTable.SubTable.GetAllEntries().GroupBy(x => x.Name).Select(x => x.FirstOrDefault()).Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            foreach (ArmpEntry entry in uniqueEntries)
                cache[(uint)entry.GetValueFromColumn(column)] = entry.Name;

            return cache;
        }

        private static Dictionary<string, ushort> CacheARMPBySubtableU16(ARMP armp, string column = "0")
        {
            Dictionary<string, ushort> cache = new Dictionary<string, ushort>();
            List<ArmpEntry> uniqueEntries = armp.MainTable.SubTable.GetAllEntries().GroupBy(x => x.Name).Select(x => x.FirstOrDefault()).Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            foreach (ArmpEntry entry in uniqueEntries)
                cache[entry.Name] = (ushort)entry.GetValueFromColumn(column);

            return cache;
        }

        private static Dictionary<string, uint> CacheARMPByKey(ARMP armp, string key)
        {
            Dictionary<string, uint> cache = new Dictionary<string, uint>();
            List<ArmpEntry> uniqueEntries = armp.MainTable.GetAllEntries().GroupBy(x => x.GetValueFromColumn(key)).Select(x => x.FirstOrDefault()).ToList();

            foreach (ArmpEntry entry in uniqueEntries)
                cache[(string)entry.GetValueFromColumn(key)] = (uint)entry.ID;

            return cache;
        }
        */

        private unsafe static void* GetDBBufferPointer(uint id)
        {
            uint val1 = id + (id * 2);

            IntPtr dbPtr = DEService.GetServicePointer(14);

            void* ptr1 = *(void**)(dbPtr + 0x30);
            IntPtr ptr2 = (IntPtr)(*(void**)((long)ptr1 + val1 * 8 + 8));

            if (ptr2 == IntPtr.Zero)
                return (void*)0;

            IntPtr ptr3 = (IntPtr)(*(void**)(ptr2 + 0x50));

            if (ptr3 != IntPtr.Zero)
            {
                IntPtr ptr4 = (IntPtr)((void**)(ptr3 + 0x20));
                return (void*)ptr4;
            }
            else
                return (void*)0;
        }

        /*
        public unsafe static void OverwriteARMPData(uint id, ARMP file)
        {
            byte[] buf = ArmpFileWriter.WriteARMPToArray(file);

            long* dataPtr = (long*)GetDBBufferPointer(id);

            if (dataPtr == null)
                return;

            IntPtr addr = Marshal.AllocHGlobal(buf.Length);
            Marshal.Copy(buf, 0, addr, buf.Length);

            *dataPtr = (long)addr;
        }
        */
    }
}
