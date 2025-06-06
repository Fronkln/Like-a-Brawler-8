﻿using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LikeABrawler2
{
    internal static class EnemyManager
    {
        public static Dictionary<uint, BaseEnemyAI> Enemies = new Dictionary<uint, BaseEnemyAI>();
        public static BaseAI ForcedAttacker = null;

        static EnemyManager()
        {
            BrawlerBattleManager.OnBattleEndEvent += OnBattleEnd;
        }

        public static void Reset()
        {
            Enemies.Clear();
            ForcedAttacker = null;
        }

        public static void OnBattleEnd()
        {
            Reset();
        }

        public static void Update()
        {
            if (BrawlerBattleManager.AllEnemies.Length <= 0)
            {
                Reset();
                return;
            }
            
            if (BrawlerBattleManager.CurrentPhase == BattleTurnManager.TurnPhase.Start || BrawlerBattleManager.CurrentPhase == BattleTurnManager.TurnPhase.StartWait)
                return;

            foreach (Fighter fighter in BrawlerBattleManager.AllFighters)
            {
                if (fighter.IsEnemy() && !fighter.IsDead())
                    if (!Enemies.ContainsKey(fighter.Character.UID))
                        Enemies.Add(fighter.Character.UID, CreateEnemy(fighter));
            }

            Enemies = Enemies.Where(x => !x.Value.Fighter.IsDead())
                             .Where(x => new EntityHandle<Character>(x.Key).IsValid())
                             .ToDictionary(x => x.Key, x => x.Value);


            BaseEnemyAI attackerEnemy = GetAI(BattleTurnManager.SelectedFighter.Get().UID);

            foreach (var kv in Enemies)
            {
                Character chara = new EntityHandle<Character>(kv.Key);

                if (chara.IsValid())
                    kv.Value.Update();
            }

            foreach (var kv in Enemies)
            {
                BaseEnemyAI enemy = kv.Value;
                enemy.Update();

                if (BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action && !HeatActionManager.IsHAct() && !Mod.IsGamePaused)
                {
                    enemy.CombatUpdate();

                    if (attackerEnemy == enemy)
                        enemy.MyTurnUpdate();
                }
            }
        }

        public static BaseEnemyAI CreateEnemy(Fighter fighter)
        {
            BaseEnemyAI ai = null;
            string rpgEnemyName = DBManager.GetRPGEnemy((uint)fighter.Character.Attributes.enemy_id);
            string soldierName = DBManager.GetSoldier((uint)fighter.Character.Attributes.soldier_data_id);

            if (soldierName == null)
                soldierName = "";

            //story bosses
            switch (soldierName)
            {
                case "elvis_btl01_0400_000_6":
                    ai = new EnemyAIAsakura1();
                    break;
                case "lng01_btl01_0040_1":
                    ai = new EnemyAIBossKuwaki();
                    break;
                case "elvis_btl02_0100_000_1":
                    ai = new EnemyAITomizawa();
                    break;
                case "elvis_btl02_0350_000_1":
                    ai = new EnemyAIBossYamai1();
                    break;
                case "elvis_btl03_0300_000_1":
                    ai = new EnemyAIBossRoman();
                    break;
                case "elvis_btl04_0580_000_1": //Runway boss
                    ai = new EnemyAIBossWPG();
                    break;
                case "elvis_btl06_0200_000_1": //Yamai that only targets Kiryu
                    ai = new EnemyAIBossYamai2();
                    break;
                case "elvis_btl07_0100_000_1":
                    ai = new EnemyAIBossYamai2();
                    break;
                case "elvis_btl07_0400_000_1":
                    ai = new EnemyAIBossYamai2();
                    break;
                case "elvis_special_hostess":
                    ai = new EnemyAIYamaiHostess();
                    break;
                case "elvis_btl07_0500_000_1":
                    ai = new EnemyAIBossWPE();
                    break;
                case "elvis_btl07_0500_000_2":
                    ai = new EnemyAIBossWPE();
                    break;
                case "elvis_boss_amon_ult_lose":
                    ai = new EnemyAIAmonLose();
                    break;
                case "elvis_btl09_0300_000_1":
                    ai = new EnemyAIBossYamai2();
                    break;
                case "elvis_lng04_btl11_0060_1":
                    ai = new EnemyAIBossWPJ_BTL11_0060();
                    break;
                case "elvis_btl12_0300_000_1":
                    ai = new EnemyAIBossMajima();
                    break;
                case "elvis_btl12_0300_000_2":
                    ai = new EnemyAIBossSaejima();
                    break;
                case "elvis_btl12_0300_000_3":
                    ai = new EnemyAIBossDaigo();
                    break;
                case "elvis_btl14_0300_7":
                    ai = new EnemyAIBossSupporterBryce();
                    break;

            }

            if(ai == null)
            {
                switch((uint)fighter.Character.Attributes.ctrl_type)
                {
                    case 178:
                        ai = new EnemyAIBossSawashiro();
                        break;
                    case 263:
                        ai = new EnemyAIBossWong();
                        break;
                    case 186:
                        ai = new EnemyAIMachinery();
                        break;
                }
            }

            if(ai == null)
            {
                switch((uint)fighter.Character.Attributes.enemy_id)
                {
                    //Dwight first fight
                    case 327:
                        ai = new EnemyAIBossDwight1();
                        break;
                }
            }

            bool bossCtrlType = (!string.IsNullOrEmpty(rpgEnemyName) && rpgEnemyName.Contains("boss")) || soldierName.Contains("boss");
            //elvis_crm_hbs03_c03_01b
            bool encounterBoss = BrawlerBattleManager.IsEncounter && ( (soldierName.Contains("_crm_") && !soldierName.Contains("_men")) || soldierName.StartsWith("elvis_job"));

            //generic bosses
            switch(soldierName)
            {
                case "elvis_dungeon_h_1-5F_104":
                    encounterBoss = true;
                    break;
                case "elvis_dungeon_h_1-10F_rescue_510":
                    encounterBoss = true;
                    break;
                case "elvis_dungeon_y_1-10F_rescue_510": //pompadeur
                    encounterBoss = true;
                    break;
                case "elvis_dungeon_y_1-10F_rescue_530": //pipe crit guy
                    encounterBoss = true;
                    break;
                case "elvis_dungeon_y_1-10F_rescue_540":
                    encounterBoss = true;
                    break;
                case "elvis_dungeon_y_1-10F_rescue_550": //billboard
                    encounterBoss = true;
                    break;
                case "elvis_dungeon_y_6-10F_boss_001": //cigar zombie ctrltype 248
                    encounterBoss = true;
                    break;
                case "elvis_lng05_btl12_0018_1":
                    encounterBoss = true;
                    break;
                case "elvis_lng06_btl13_0060_1": //samuel: chainsaw boss, uses WPC
                    encounterBoss = true;
                    break;
                case "elvis_btl14_0300_1": //ichiban finale palekana officer
                    encounterBoss = true;
                    break;
                case "elvis_btl14_0350_1": //ichiban finale SMG palekana boss
                    encounterBoss = true;
                    break;
            }

            uint ctrlType = (uint)fighter.Character.Attributes.ctrl_type;
            bool forcedBoss = BrawlerBattleManager.AllEnemies.Length <= 2;

            if (ai == null)
            {
                if (forcedBoss || (bossCtrlType || encounterBoss))
                {
                    switch (ctrlType)
                    {
                        case 159:
                            ai = new EnemyAIBossHecaton();
                            break;
                        case 172:
                            ai = new EnemyAIBossWeaponMaster();
                            break;
                        case 208:
                            ai = new EnemyAIBossLandSurfer();
                            break;
                        case 213:
                            ai = new EnemyAIBossCalorieKnight();
                            break;
                        case 242:
                            ai = new EnemyAIBossSumo();
                            break;
                        case 243:
                            ai = new EnemyAIBossLongPierrot();
                            break;
                        case 260:
                            ai = new EnemyAIBossDwight1();
                            break;
                        case 274:
                            ai = new EnemyAIBossEbina();
                            break;
                        case 278:
                            ai = new EnemyAIBossBryce();
                            break;
                    }

                    if (ai == null)
                    {
                        switch (Asset.GetArmsCategory(fighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().AssetID))
                        {
                            default:
                                ai = new EnemyAIBoss();
                                break;
                            case AssetArmsCategoryID.B:
                                ai = new EnemyAIBossWPB();
                                break;
                            case AssetArmsCategoryID.E:
                                ai = new EnemyAIBossWPE();
                                break;
                            case AssetArmsCategoryID.D:
                                ai = new EnemyAIBossWPD();
                                break;
                            case AssetArmsCategoryID.R:
                                ai = new EnemyAIBossWPR();
                                break;
                            case AssetArmsCategoryID.Y:
                                ai = new EnemyAIBossWPY();
                                break;
                        }
                    }
                }
                else
                {
                    switch(ctrlType)
                    {
                        case 243:
                            ai = new EnemyAILongPierrot();
                            break;
                    }

                    if(ai == null)
                        ai = new BaseEnemyAI();
                }
            }

            if (forcedBoss)
            {
                var aiBoss = (ai as EnemyAIBoss);

                if(aiBoss != null)
                    aiBoss.IsForcedBoss = forcedBoss;
            }

            ai.Fighter = fighter;
            ai.Character = fighter.Character;


            BrawlerFighterInfo inf = new BrawlerFighterInfo() { Fighter = fighter};

            if (!BrawlerFighterInfo.Infos.ContainsKey(ai.Character.UID))
                BrawlerFighterInfo.Infos.Add(ai.Character.UID, inf);

            ai.Awake();

            return ai;
        }

        public static BaseEnemyAI GetAI(Fighter fighter)
        {
            if (Enemies.ContainsKey(fighter.Character.UID))
                return Enemies[fighter.Character.UID];

            return null;
        }

        public static BaseEnemyAI GetAI(uint UID)
        {
            if (Enemies.ContainsKey(UID))
                return Enemies[UID];

            return null;
        }

        public static void ReloadContent()
        {
            foreach (var kv in Enemies)
                kv.Value.LoadContent();
        }


        public static bool IsAnyoneMortalAttacking()
        {
            return Enemies.Any(x => x.Value.IsMortalAttackOrPreparing());
        }


        public static bool IsAnyoneCounterAttacking()
        {
            return Enemies.Any(x => x.Value.IsCounterAttacking());
        }

        /// <summary>
        /// "Skipping" a turn would mean another enemy can immediately get another one, possibly making combat more fast paced.
        /// </summary>
        /// <returns></returns>
        public static bool ShouldSkip(BaseEnemyAI enemy)
        {
            if (Enemies.Count < 2)
                return false;

            if (enemy.IsBoss())
                return false;

            //allow skip only if we are doing normal attacks.
           //because it will mess up skills.
           //TODO MAYBE: Only skip on skills when we are within the timing of "Battle" node?
            string cmdName = enemy.Character.HumanModeManager.GetCommandName();

            if (!cmdName.StartsWith("Light_") && !cmdName.StartsWith("Heavy_"))
                return false;

            return enemy.CanAttackCancel();
        }
    }
}
