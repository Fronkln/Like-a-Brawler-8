﻿using System;
using System.IO;
using System.Collections.Generic;
using DragonEngineLibrary;
using System.Linq;

namespace LikeABrawler2
{
    internal static class RevelationManager
    {
        private static float m_validTime;
        public static bool RevelationProcedure = false;
        private static uint m_lastPlayerLevel = 0;

        private static Dictionary<uint, List<TalkParamID>> m_revelationMap = new Dictionary<uint, List<TalkParamID>>();
        private static List<TalkParamID> m_revelationQueue = new List<TalkParamID>();

        public static void Init()
        {
            ReadRevelationsFile();

            BrawlerBattleManager.OnBattleStartEvent += OnBattleStart;
            BrawlerBattleManager.OnBattleEndEvent += OnBattleEnd;

            DragonEngine.RegisterJob(Update, DEJob.Update);
        }

        public static bool IsQueue()
        {
            return m_revelationQueue.Count > 0;
        }


        private static void ReadRevelationsFile()
        {
            m_revelationMap.Clear();

            string revelationsFilePath = Path.Combine(Mod.ModPath, "mdb.brawler", "revelations_ichiban.txt");

            if (!File.Exists(revelationsFilePath))
                return;

            string[] revelationsTxt = File.ReadAllLines(revelationsFilePath);

            foreach (string revelationEntry in revelationsTxt)
            {
                string line = revelationEntry;

                if (line.Contains("//"))
                    line = line.Remove(line.IndexOf("//"));

                if (string.IsNullOrEmpty(line))
                    continue;

                try
                {
                    string[] revelationDat = line.Split(' ');

                    if (revelationDat.Length < 2)
                        continue;

                    uint level = uint.Parse(revelationDat[0]);
                    TalkParamID hact = DBManager.GetTalkParam(revelationDat[1]);

                    if (!m_revelationMap.ContainsKey(level))
                        m_revelationMap.Add(level, new List<TalkParamID>());

                    m_revelationMap[level].Add(hact);
                }
                catch { }
            }
        }

        public static void OnPlayerSpawn()
        {
            if (BrawlerBattleManager.PlayerCharacter.IsValid())
            {
                uint startLevel = BrawlerSaveData.GetRevelationQueue(BrawlerPlayer.IsKiryu());

                if (startLevel > 0)
                    m_revelationQueue = GetRevelations(BrawlerSaveData.GetRevelationQueue(BrawlerPlayer.IsKiryu()));
                else
                    m_revelationQueue.Clear();

                DragonEngine.Log("Recovered revelations savedata. Revelation queue length: " + m_revelationQueue.Count);
            }
        }

        public static void Update()
        {
            if (!ShouldDoRevelationProcedure())
                m_validTime = 0;
            else
                m_validTime += DragonEngine.deltaTime;

            if (m_revelationQueue.Count <= 0)
                return;

            if (!RevelationProcedure)
            {
                if (m_validTime > 0.2f)
                {
                    RevelationProcedure = true;
                    DoRevelationProcedure();
                }

            }
        }


        private static List<TalkParamID> GetRevelations(uint startLevel)
        {
            List<TalkParamID> revList = new List<TalkParamID>();

            for (uint i = startLevel; i < Player.GetLevel(Player.ID.kasuga) + 1; i++)
            {
                if (!m_revelationMap.ContainsKey(i))
                    continue;

                revList.AddRange(m_revelationMap[i]);
            }

            return revList;
        }

        private static void DoRevelationProcedure()
        {
            BrawlerSaveData.SetRevelationQueue(0, BrawlerPlayer.IsKiryu());

            TalkParamID hactID = 0;

            if (BrawlerPlayer.IsKasuga())
                hactID = DBManager.GetTalkParam("y8b1260_ich_rev_start");

            HActRequestOptions opts = new HActRequestOptions();
            opts.id = hactID;
            opts.is_force_play = true;
            opts.base_mtx.matrix = BrawlerBattleManager.PlayerCharacter.GetMatrix();
            opts.Register(HActReplaceID.hu_player1, BrawlerBattleManager.PlayerCharacter);
            HeatActionManager.RequestTalk(opts);

            DragonEngine.Log("That's rad!");



            new DETask(delegate { return !BrawlerBattleManager.IsHAct; },
                delegate
                {
                    new DETaskChainHAct(delegate { RevelationProcedure = false; m_revelationQueue.Clear(); }, true, m_revelationQueue.ToArray());
                    /*
                    new DETask(
                        delegate 
                        {
                            if (!HActManager.IsPlaying() && !AuthManager.PlayingScene.IsValid())
                            {
                                PlayRevelation();
                                DragonEngine.Log("HIRAMETA!");
                            }

                            return m_revelationQueue.Count <= 0;
                        }, delegate { m_revelationProcedure = false; });
                    */
                }, true);
        }

        private static void PlayRevelation()
        {
            TalkParamID hactID = m_revelationQueue[0];

            HActRequestOptions opts = new HActRequestOptions();
            opts.id = hactID;
            opts.is_force_play = true;

            opts.Register(HActReplaceID.hu_player1, BrawlerBattleManager.PlayerCharacter);
            HActManager.RequestHActProc(opts);


            m_revelationQueue.RemoveAt(0);
        }

        private static bool ShouldDoRevelationProcedure()
        {
            if (BrawlerBattleManager.CurrentPhase != BattleTurnManager.TurnPhase.Action)
                return false;

            if (GameVarManager.GetValueBool(GameVarID.is_scene_fading))
                return false;

            if (GameVarManager.GetValueBool(GameVarID.is_hact))
                return false;

            if (GameVarManager.GetValueBool(GameVarID.is_talk))
                return false;

            if (GameVarManager.GetValueBool(GameVarID.is_pause))
                return false;

            if (GameVarManager.GetValueBool(GameVarID.is_ui_loading))
                return false;

            return true;
        }

        //Adds revelations to the queue if we just learnt any moves/hacts
        //defined in revelations.txt
        private static void CheckRevelationEligibility()
        {
            List<TalkParamID> revelations = GetRevelations(m_lastPlayerLevel + 1);
            m_revelationQueue = revelations;

            if (m_revelationQueue.Count > 0)
            {
                BrawlerSaveData.SetRevelationQueue((int)m_lastPlayerLevel + 1, BrawlerPlayer.IsKiryu());
                DragonEngine.Log("Player can experience new revelations.");
            }
        }

        private static void OnBattleStart()
        {
            m_lastPlayerLevel = Player.GetLevel(Player.ID.kasuga);
        }

        private static void OnBattleEnd()
        {
            if (Player.GetLevel(Player.ID.kasuga) > m_lastPlayerLevel)
                CheckRevelationEligibility();

            //EffectEventManager.StopScreen(7);
        }
    }
}