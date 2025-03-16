using DragonEngineLibrary;
using ElvisCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace LikeABrawler2
{
    internal static class BrawlerBattleManager
    {
        public static Character PlayerCharacter = new Character();
        public static Fighter PlayerFighter = new Fighter(IntPtr.Zero);

        public static CameraBase RPGCamera = new CameraBase();

        public static Fighter[] AllFighters = new Fighter[0];
        public static Fighter[] AllEnemiesNearest = new Fighter[0];
        public static Fighter[] AllEnemies = new Fighter[0];
        public static Fighter NearestEnemyBehindPlayer = new Fighter(IntPtr.Zero);

        public static bool Battling = false;
        public static bool IsEncounter { get; private set; }
        public static uint BattleConfigID { get; private set; }
        private static bool m_battleStartedDoOnce = false;
        private static bool m_battleActionStartedDoOnce = false;

        public static bool DisableTargetingOnce = false;
        public static bool DisableTargetingThisFrame = false;

        public static event Action OnBattleStartEvent = null;
        public static event Action OnBattleEndEvent = null;

        public static event Action OnActionStartEvent = null;

        public static bool IsReinforcementsFight { get; private set; }
        public static bool IsHAct;
        public static bool IsHActOrWaiting { get { return IsHAct || HeatActionManager.AwaitingHAct; } }

        public static BattleTurnManager.TurnPhase CurrentPhase;
        public static BattleTurnManager.ActionStep CurrentActionStep;

        public static float BattleTime = 0;
        public static float ActionBattleTime = 0;
        public static float CurrentPhaseTime = 0;
        public static float CurrentActionStepTime = 0;

        public static bool BattleEndedInY8BHAct { get; private set; } = false;

        public static bool AllowPlayerTransformThisFight = true;
        public static bool AllowAllyTransformThisFight = true;

        private static bool m_givePlayerTurnOnce = false;
        private static uint m_skippedEnemyUID = 0;
        private static bool m_actionStartedDoOnce = false;

        private static Player.ID[] m_partyMemberIDs = new Player.ID[3];

        public static bool IsDeliveryHelping = false;
        private static IntPtr m_forceDeliveryAddr = IntPtr.Zero;
        public static bool UsedDeliveryOnce = false;


        private static uint m_bgmID;
        private static uint m_bgmTime;
        private static bool m_specialBgmPlaying = false;

        public static bool SoloBattleOnce = false;

        public static void Init()
        {
            HeatActionManager.OnHActEndEvent += OnHActEnd;

            m_forceDeliveryAddr = DragonEngineLibrary.Unsafe.CPP.PatternSearch("41 8B 86 D0 0C 00 00");
        }

        public static void InputUpdate()
        {
            if (Mod.IsTurnBased())
                return;

            if (PlayerFighter.IsValid())
                BrawlerPlayer.InputUpdate();
        }

        public static void PreUpdate()
        {
            HeatActionManager.PreUpdate();
        }

        public unsafe static void Update()
        {
            Character character = null;

            if (PlayerFighter.IsValid())
                character = PlayerFighter.Character;
            else
                character = DragonEngine.GetHumanPlayer();


            if (!PlayerCharacter.IsValid() && character.IsValid())
            {
                PlayerCharacter = character;
                OnPlayerSpawn();
            }

            PlayerCharacter = character;

#if DEMO
            //DEMO: Chapter 1 only
            //Check if we are in chapter 2.
            if (TimelineManager.CheckClockAchievement(1, 79, 2))
            {
                DragonEngine.MessageBox(IntPtr.Zero, "Like A Brawler 8 Demo is only for chapter 1!\n Thank you for playing, look forward to full release!", "Demo", 0);
                Environment.Exit(0);
            }
#endif

            TutorialManager.Update();
            ScreenEffectManager.Update(); 

            AllFighters = FighterManager.GetAllFighters();
            AllEnemies = AllFighters.Where(x => x.IsEnemy() && !x.IsDead()).ToArray();
            AllEnemiesNearest = AllEnemies.OrderBy(x => Vector3.Distance(PlayerFighter.Character.Transform.Position, x.Character.Transform.Position)).ToArray();

            var enemiesBehindMe = AllEnemies.Where(x => !PlayerCharacter.IsFacingEntity(x.Character)).OrderBy(x => Vector3.Distance(PlayerFighter.Character.Transform.Position, x.Character.Transform.Position));

            if (enemiesBehindMe.Any())
            {
                NearestEnemyBehindPlayer = enemiesBehindMe.First();

           //    if (Vector3.Distance(NearestEnemyBehindPlayer.Character.Transform.Position, PlayerCharacter.Transform.Position) >= 3.5f)
                   // NearestEnemyBehindPlayer = new Fighter();
            }
            else
                NearestEnemyBehindPlayer = new Fighter();


            RPGCamera = BattleTurnManager.RPGCamera;

            if(!Battling)
                PlayerFighter = FighterManager.GetPlayer();
            else
            {
                int idx = NakamaManager.FindIndex(BrawlerPlayer.CurrentPlayer);

                if (idx >= 0)
                    PlayerFighter = FighterManager.GetFighter((uint)idx);
                else
                    PlayerFighter = FighterManager.GetFighter(0);
            }

            //TODO: Improve this battle start detection
            if (!m_battleStartedDoOnce)
            {
                if (PlayerFighter.IsValid())
                {
                    if (Mod.IsRealtime())
                        OnRealtimeBattleStart();
                    else
                        BrawlerPatches.Disable();
                }
            }
            else
            {
                if (!m_battleActionStartedDoOnce && BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action)
                    if (Mod.IsRealtime())
                        OnRealtimeBattleActionStart();

                if (!PlayerFighter.IsValid())
                    OnBattleEnd();
            }

            GenericUpdate();

            if (Mod.IsTurnBased())
                return;

            PhaseUpdate();

            AuraManager.Update();
            HeatActionManager.Update();

            if (Battling)
            {
                RealtimeUpdate();

                if (CurrentPhase == BattleTurnManager.TurnPhase.Action)
                    RealtimeCombatUpdate();

                bool paused = GameVarManager.GetValueBool(GameVarID.is_pause);

                if (paused || (CurrentPhase != BattleTurnManager.TurnPhase.Action))
                {
                    BrawlerPatches.CombatPatches.EnableAssignment();

                    if (paused)
                    {
                        PlayerFighter.GetStatus().SetHPCurrent(Player.GetHPNow(BrawlerPlayer.CurrentPlayer));

                        Fighter p2 = FighterManager.GetFighter(1);
                        Fighter p3 = FighterManager.GetFighter(2);
                        Fighter p4 = FighterManager.GetFighter(3);

                        p2.GetStatus().SetHPCurrent(Player.GetHPNow(p2.Character.Attributes.player_id));
                        p3.GetStatus().SetHPCurrent(Player.GetHPNow(p3.Character.Attributes.player_id));
                        p4.GetStatus().SetHPCurrent(Player.GetHPNow(p4.Character.Attributes.player_id));
                    }
                }
                else
                {
                    BrawlerPatches.CombatPatches.DisableAssignment();
                    InputState.Push(0x5D);
                }
            }

            BrawlerUIManager.Update();
        }

        private static void PhaseUpdate()
        {
            IsHAct =/* AuthManager.PlayingScene.IsValid() || */GameVarManager.GetValueBool(GameVarID.is_hact);

            BattleTurnManager.TurnPhase phase = BattleTurnManager.CurrentPhase;
            BattleTurnManager.ActionStep step = BattleTurnManager.CurrentActionStep;

            if (phase != CurrentPhase)
            {
                CurrentPhaseTime = 0;
                DragonEngine.Log(CurrentPhase + " -> " + phase);
            }

            if (step != CurrentActionStep)
            {
                CurrentActionStepTime = 0;

                BaseEnemyAI enemy = EnemyManager.GetAI(BattleTurnManager.SelectedFighter.UID);

                if (enemy != null)
                    if (phase == BattleTurnManager.TurnPhase.Action && step == BattleTurnManager.ActionStep.Ready)
                        enemy.OnMyTurnStart();

            }

            if (CurrentPhase != BattleTurnManager.TurnPhase.BattleResult && phase == BattleTurnManager.TurnPhase.BattleResult)
                OnBattleResult();

            if (CurrentPhase != BattleTurnManager.TurnPhase.Start && phase == BattleTurnManager.TurnPhase.Start)
                OnRealtimeBattleStartPhase();

            if (CurrentPhase < BattleTurnManager.TurnPhase.Action && phase == BattleTurnManager.TurnPhase.Action)
                if (!m_actionStartedDoOnce)
                {
                    OnActionStartEvent?.Invoke();
                    m_actionStartedDoOnce = true;
                }


            CurrentPhase = phase;
            CurrentActionStep = step;

            float dt = DragonEngine.deltaTime;

            CurrentPhaseTime += dt;
            CurrentActionStepTime += dt;
        }

        public unsafe static void OnPlayerEnterEXHeat()
        {
            m_bgmID = SoundManager.GetBGMSeID(1);
        }

        public unsafe static void OnPlayerExitEXHeat()
        {
            if(SoundManager.GetBGMSeID(1) != m_bgmID)
            {
                //SoundManager.PlayBGM(m_bgmID, m_bgmTime);
            }

            //test.Invoke(SoundManager.Pointer(), handle, false);


            StopSpecialMusic();
        }


        /// <summary>
        /// (EXPERIMENTAL) makes the provided nakama the main player.
        /// </summary>
        public unsafe static void MakeNakamaMain(uint idx)
        {
            Character oldChara = PlayerCharacter;
            Character chara = NakamaManager.GetCharacterHandle(idx);

            unsafe
            {
                byte* free_movement_mode = (byte*)(oldChara.Pointer.ToInt64() + 0x11A3);
                *free_movement_mode = 0;
            }

            PlayerCharacter = chara;
            PlayerFighter = FighterManager.GetFighter(idx);
            BrawlerPlayer.CurrentPlayer = chara.Attributes.player_id;

            EntityHandle<CameraBase> cam = PlayerCharacter.GetSceneEntity<CameraBase>(SceneEntity.camera_free);

            if(cam.IsValid())
            {
                IntPtr camPtr = cam.Get().Pointer;
                uint* camTarget = (uint*)(camPtr + 692);
                *camTarget = chara.UID;
            }
        }

        private static bool AllowSpecialMusic()
        {
            //ebina battle
            return BattleConfigID != 174;
        }

        public unsafe static void PlaySpecialMusic(ushort cue, ushort id)
        {
            if (!AllowSpecialMusic())
                return;

            SoundManager.PlayBGM(0, cue, id, 0);
            int handle1 = *(int*)(SoundManager.Pointer() + 0xD4);
            int handle2 = *(int*)(SoundManager.Pointer() + 0xD8);

            SoundManager.Pause(handle1, false);
            SoundManager.Pause(handle2, true);

            m_specialBgmPlaying = true;
        }

        public unsafe static void StopSpecialMusic()
        {
            if (!m_specialBgmPlaying)
                return;

            int handle1 = *(int*)(SoundManager.Pointer() + 0xD4);
            int handle2 = *(int*)(SoundManager.Pointer() + 0xD8);

            SoundManager.PlayBGM(0, 1, 1, 999999);

            SoundManager.Pause(handle1, true);
            SoundManager.Pause(handle2, false);

            m_specialBgmPlaying = false;
        }

        private static void DoBadBattleWarning()
        {
            HActRequestOptions opts = new HActRequestOptions();
            opts.base_mtx.matrix = PlayerCharacter.GetMatrix();
            opts.id = DBManager.GetTalkParam("y8b_warning_poor_battle");
            opts.is_force_play = true;

            HeatActionManager.RequestTalk(opts);
        }

        private static void ProcSpecialBattle()
        {
            switch(BattleConfigID)
            {
                /*
                //Yami 2nd fight
                case 36:
                    new DETask(delegate { return BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action; }, delegate
                    {
 
                        new DETaskTime(1f, delegate { DoBadBattleWarning(); });
                    });
                    break;
                */
                //Yamai first fight
                case 8:
                    new DETask(delegate { return !IsHActOrWaiting && ActionBattleTime > 3.5f && AllEnemies.Length <= 3; }, 
                        delegate 
                        {
                            HActRequestOptions opts = new HActRequestOptions();
                            opts.id = DBManager.GetTalkParam("eb1560_boss_yam_pc");
                            opts.is_force_play = true;

                            opts.Register(HActReplaceID.hu_player1, PlayerCharacter);
                            opts.Register(HActReplaceID.hu_enemy_00, AllEnemiesNearest[0].CharacterUID);

                            HActManager.RequestHAct(opts);
                        });
                    break;

                case 37: //Wong Tou
                    AllowAllyTransformThisFight = false;
                    AllowPlayerTransformThisFight = false;
                    break;
                case 38: //Special Suit Fight
                    AllowAllyTransformThisFight = false;
                    AllowPlayerTransformThisFight = false;
                    break;
                case 39: //Special Suit Fight
                    AllowAllyTransformThisFight = false;
                    AllowPlayerTransformThisFight = false;
                    break;
                case 40: //Special Suit Fight
                    AllowAllyTransformThisFight = false;
                    AllowPlayerTransformThisFight = false;
                    break;

                case 85: //narasaki fight
                    new DETask(delegate { return BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action; }, delegate
                    {
                        new DETaskTime(0.5f, delegate { DoBadBattleWarning(); });
                    });
                    break;

                //Poundmates Introduction
                //TODO IMPORTANT: Give player 200 USD for this fight because our tutorial makes the poundmate paid for some reason.
                case 147:
                    new DETask(delegate { return BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action; }, delegate
                    {
                        ForceGivePlayerTurn();

                        new DETaskTime(1f, delegate { TutorialManager.StartTutorial(TutorialManager.Tutorial05_Kasuga()); });
                    });
                    break;

                case 161:
                    SpecialBattle.TriosFight();
                    break;
                case 174: // ebina
                    new DETask(delegate { return BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action && !GameVarManager.GetValueBool(GameVarID.is_hact); }, delegate
                    {
                        new DETaskTime(0.06f, delegate
                        {
                            if (NakamaManager.GetCharacterHandle(1).IsValid())
                            {
                                HActRequestOptions opts = new HActRequestOptions();
                                opts.base_mtx.matrix = PlayerCharacter.GetMatrix();
                                opts.base_mtx.matrix.ForwardDirection = new Vector4(0, 0, -1);
                                opts.base_mtx.matrix.LeftDirection = new Vector4(-1, 0, 0);
                                opts.id = DBManager.GetTalkParam("y8bb1780_ebn_party_decide");
                                opts.is_force_play = true;

                                opts.Register(HActReplaceID.hu_player1,PlayerCharacter);
                                opts.Register(HActReplaceID.hu_npc_00, FighterManager.GetFighter(1).Character);
                                opts.Register(HActReplaceID.hu_npc_01, FighterManager.GetFighter(2).Character);
                                opts.Register(HActReplaceID.hu_npc_02, FighterManager.GetFighter(3).Character);

                                HeatActionManager.RequestTalk(opts);

                                new DETask(delegate { return HeatActionManager.IsHAct(); }, delegate
                                {
                                    SkipTurn();
                                    new DETask(delegate
                                    {
                                        SkipTurn();
                                        return !HeatActionManager.IsHAct();
                                    }, null);
                                });
                                
                            }
                        });
                    });
                    break;
                case 175: //shark fight (vs player)
                    new DETask(delegate { return BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action; }, delegate
                    {
                        new DETaskTime(0.3f, delegate { DoBadBattleWarning(); });
                    });
                    break;
                case 182: //blessed leviathan fight
                    new DETask(delegate { return BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action; }, delegate
                    {
                        new DETaskTime(0.3f, delegate { DoBadBattleWarning(); });
                    });
                    break;

                case 188: //crane fight
                    new DETask(delegate { return BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action; }, delegate
                    {
                        new DETaskTime(1f, delegate { DoBadBattleWarning(); });
                    });
                    break;
                case 189:
                    new DETask(delegate { return BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action; }, delegate
                    {
                        //Adachi protagonist
                        new DETaskTime(0.05f, delegate { SpecialBattle.SplitFight(); });
                    });
                    break;

            }
        }

        public static bool IsBriefTurnBased()
        {
            return IsDeliveryHelping;
        }

        private static void GenericUpdate()
        {
            if (IsBriefTurnBased())
            {
                DeliveryHelpUpdate();
            }
        }

        private static void DeliveryHelpUpdate()
        {
            BrawlerPatches.CombatPatches.EnableAssignment();

            if (BattleTurnManager.SelectedFighter.UID != BrawlerBattleManager.PlayerFighter.CharacterUID)
                SkipTurn();
            else
            {
                //TEMP: CANCEL
                //DOES NOT ACCOUNT ON WHERE YOU PRESS IT!!!!!!!!!!!
                if (BattleManager.PadInfo.IsJustPush(BattleButtonID.action))
                {
                    IsDeliveryHelping = false;
                    OnForceDeliveryHelpOFF();

                    new DETaskTime(0.1f, delegate
                    {
                        ChangeToRealtime();
                        OnForceDeliveryHelpOFF();
                    });

                    return;
                }
            }

            //Ichiban called for reinforcements!
            if (GameVarManager.GetValueBool(GameVarID.is_hact))
            {
                IsDeliveryHelping = false;
                bool readyOnce = false;
                bool over = false;


                new DETask(
                    delegate
                    {
                        if (!readyOnce)
                        {
                            if (BattleTurnManager.CurrentActionStep == BattleTurnManager.ActionStep.Ready)
                                readyOnce = true;
                        }
                        else
                        {
                            if (BattleTurnManager.CurrentActionStep == BattleTurnManager.ActionStep.Action)
                            {
                                new DETaskTime(0.5f, delegate
                                {
                                    //HAct poundmate
                                    if (GameVarManager.GetValueBool(GameVarID.is_hact))
                                        new DETask(delegate { return !GameVarManager.GetValueBool(GameVarID.is_hact); }, delegate { over = true; });
                                    else
                                        over = true;
                                });
                            }
                        }

                        if(over)
                            DragonEngine.Log("Forced delivery OVER");

                        return over;
                    }, ForcedDeliveryHActEventEnd);
                OnForceDeliveryHelpOFF();
            }
        }


        //Disables hact phase control
        private static bool CheckReinforcementsFight()
        {
            if (BattleConfigID == 35)
                return true;

            if (BattleConfigID == 100)
                return true;

            if (BattleConfigID == 228)
                return true;

            return false;
        }
        private static void RealtimeUpdate()
        {
            BattleTime += DragonEngine.deltaTime;

            if (!BattleEndedInY8BHAct)
                if (CurrentPhase == BattleTurnManager.TurnPhase.Action && BattleTurnManager.SelectedFighter == PlayerCharacter)
                {
                    if (!IsBriefTurnBased())
                    {
                        DragonEngine.Log("SKIPPED A TURN THAT WAS GIVEN TO THE PLAYER FOR SOME REASON!");
                        SkipTurn();
                    }
                }

            if (CurrentPhase == BattleTurnManager.TurnPhase.Action)
                ActionBattleTime += DragonEngine.deltaTime;

            if (AllEnemies.Length <= 0)
                if (HeatActionManager.IsY8BHact)
                    if (CurrentPhase == BattleTurnManager.TurnPhase.Action)
                    {
                        if (!BattleEndedInY8BHAct)
                            OnEnemiesDefeatedInY8BHact();
                    }
            SupporterManager.Update();
            EnemyManager.Update();
            WeaponManager.Update();
            SpecialBattle.Update();
            HActLifeGaugeManager.Update();
        }

        private static void RealtimeCombatUpdate()
        {
            if (Mod.IsGamePaused)
                return;

            BrawlerPlayer.Update();
            WeaponManager.RealtimeCombatUpdate();

            RPGCamera.Sleep();

            Dictionary<uint, BrawlerFighterInfo> brawlerInfos = new Dictionary<uint, BrawlerFighterInfo>(BrawlerFighterInfo.Infos);

            foreach (var kv in brawlerInfos)
                kv.Value.Update(kv.Value.Fighter);

            BaseEnemyAI enemyAttackerAI = EnemyManager.GetAI(BattleTurnManager.SelectedFighter.UID);
            BaseSupporterAI supporterAttackerAI = SupporterManager.GetAI(BattleTurnManager.SelectedFighter.UID);

            if (supporterAttackerAI != null)
            {
                //Process generic supporter turns fast

                /*
                if (CurrentActionStep == BattleTurnManager.ActionStep.Ready && CurrentActionStepTime > 2f)
                {
                    if (supporterAttackerAI.CanAttackCancel())
                    {
                        BattleTurnManager.ChangePhase(BattleTurnManager.TurnPhase.Action);
                        BattleTurnManager.ChangeActionStep(BattleTurnManager.ActionStep.ActionFinalize);
                    }
                }
               */
                if (CurrentActionStep == BattleTurnManager.ActionStep.Init)
                    BattleTurnManager.ChangeActionStep(BattleTurnManager.ActionStep.SelectCommand);

            }
            else if (enemyAttackerAI != null)
            {
                //13.04.2024, always skipping every frame ensures kiryu never moves and his attacks never transited
                //if you want to delve into why kiryu stops moving for a frame on turn start
                //do exactly that and see what happens.
                //Guesses:
                //1) BattleManager Input does not get updated when that happens
                //2) There is an actual check before doing any of that

                if (EnemyManager.Enemies.Count > 1)
                {
                    if (CurrentActionStep == BattleTurnManager.ActionStep.Ready && CurrentActionStepTime > 0.5f)
                    {
                        if (EnemyManager.ShouldSkip(enemyAttackerAI))
                        {
                            m_skippedEnemyUID = enemyAttackerAI.Character.UID;

                            BattleTurnManager.ChangePhase(BattleTurnManager.TurnPhase.Action);
                            BattleTurnManager.ChangeActionStep(BattleTurnManager.ActionStep.ActionFinalize);

                            DragonEngine.Log("Skipped");
                        }


                        if (CurrentActionStep == BattleTurnManager.ActionStep.Init)
                            BattleTurnManager.ChangeActionStep(BattleTurnManager.ActionStep.SelectCommand);
                    }
                }
            }
        }


        public static void ChangeToRealtime()
        {
            if(Battling)
                BrawlerPatches.Enable();

            BattleTurnManager.OverrideAttackerSelection2(DecideTurnAttacker);
            if (Mod.IsTurnBased())
            {
                if (PlayerFighter.IsValid())
                {
                    BrawlerPlayer.CurrentPlayer = PlayerCharacter.Attributes.player_id;
                    SkipTurn();

                    BrawlerPlayer.ToNormalMoveset();
                   // PlayerCharacter.HumanModeManager.CommandsetModel.SetCommandSet(0, BrawlerPlayer.GetCommandSetForJob(BrawlerPlayer.CurrentPlayer, Player.GetCurrentJob(BrawlerPlayer.CurrentPlayer)));
                }
            }

            Mod.Gamemode = 1;
            DragonEngine.Log("Changed gamemode to realtime.");

            if (BrawlerPlayer.IsKasuga())
                IniSettings.IsIchibanRealtime = 1;
            else
                IniSettings.IsKiryuRealtime = 1;
        }

        public static void ChangeToTurnBased()
        {
            BattleTurnManager.OverrideAttackerSelection2(null);
            BrawlerPatches.Disable();

            if (Mod.IsRealtime())
                if (BattleTurnManager.CurrentPhase != BattleTurnManager.TurnPhase.NumPhases)
                    PlayerCharacter.HumanModeManager.CommandsetModel.SetCommandSet(0, RPG.GetJobCommandSetID(BrawlerPlayer.CurrentPlayer, Player.GetCurrentJob(BrawlerPlayer.CurrentPlayer)));

            BrawlerUIManager.OnSwitchToTurnBased();


            Mod.Gamemode = 0;
            DragonEngine.Log("Changed gamemode to turn based.");

            if (BrawlerPlayer.IsKasuga())
                IniSettings.IsIchibanRealtime = 0;
            else
                IniSettings.IsKiryuRealtime = 0;

        }

        //On player character valid, not fighter
        private static void OnPlayerSpawn()
        {
            DragonEngine.Log("Player spawned, address: " + PlayerCharacter.Pointer.ToString("X"));

            BrawlerPlayer.CurrentPlayer = PlayerCharacter.Attributes.player_id;

            if (BrawlerPlayer.IsKasuga())
            {
                if (IniSettings.IsIchibanRealtime == 1)
                    ChangeToRealtime();
                else
                    ChangeToTurnBased();
            }
            else
            {
                if (IniSettings.IsKiryuRealtime == 1)
                    ChangeToRealtime();
                else
                    ChangeToTurnBased();
            }

            RevelationManager.OnPlayerSpawn();
        }

        private static void PrepareRealtimeBattle()
        {
            DBManager.Init();
        }

        private static void OnRealtimeBattleStart()
        {
            DragonEngine.Log("Realtime battle start!");

            BrawlerPatches.Enable();
            BrawlerPatches.CombatPatches.OnCombatStart();

            Battling = true;
            BattleTime = 0;
            ActionBattleTime = 0;
            m_battleStartedDoOnce = true;
            AllowAllyTransformThisFight = true;
            AllowPlayerTransformThisFight = true;

            HeatActionManager.AwaitingHAct = false;

            CharacterAttributes playerAttribs = PlayerCharacter.Attributes;

            BrawlerPlayer.CurrentPlayer = playerAttribs.player_id;
            BrawlerPlayer.OriginalPlayerAttributes = playerAttribs;
            BrawlerPlayer.OriginalPlayerAttributes.player_id = BrawlerPlayer.CurrentPlayer;

            BrawlerFighterInfo.Infos.Add(PlayerCharacter.UID, new BrawlerFighterInfo() { Fighter = PlayerFighter });

            BrawlerPlayer.OnBattleStart();

            OnBattleStartEvent?.Invoke();

            DragonEngine.Log("Realtime battle event complete.");
        }

        private static void LoadBattleResources()
        {
            SoundManager.LoadCuesheet(DBManager.GetSoundCuesheet("act_kiryu"));
            SoundManager.LoadCuesheet(DBManager.GetSoundCuesheet("y8b_common"));
            SoundManager.LoadCuesheet(DBManager.GetSoundCuesheet("y8b_common_sfx"));

            if (BrawlerPlayer.IsDragon())
                SoundManager.LoadCuesheet(DBManager.GetSoundCuesheet("bbg_k"));

            if(BrawlerPlayer.IsKasuga())
                SoundManager.LoadCuesheet(DBManager.GetSoundCuesheet("style_freeter"));

            if(BrawlerPlayer.IsDragon())
                SoundManager.LoadCuesheet(DBManager.GetSoundCuesheet("style_oedragon"));

            EffectEventManager.LoadScreen(28); //Judge_fatalblow
            EffectEventManager.LoadScreen(68); //Brawler_finishblow
            EffectEventManager.LoadScreen(69); //PhysicalWarning_Brawler
        }

        //BattleTurnManager start
        public static void OnRealtimeBattleStartPhase()
        {
            BattleConfigID = BattleTurnManager.BattleConfigID;
            IsEncounter = BattleConfigID <= 2;

            IsReinforcementsFight = true;  //CheckReinforcementsFight();
            ProcSpecialBattle();
            LoadBattleResources();


            foreach (var kv in SupporterManager.Supporters)
                kv.Value.BattleStartEvent();

            DragonEngine.Log("Battle Config ID: " + BattleConfigID);
        }

        public static void SkipTurn()
        {
            BattleTurnManager.ChangePhase(BattleTurnManager.TurnPhase.Action);
            BattleTurnManager.ChangeActionStep(BattleTurnManager.ActionStep.ActionFinalize);
        }

        public static void ForceGivePlayerTurn()
        {
            m_givePlayerTurnOnce = true;
            SkipTurn();
        }

        public static void OnForceDeliveryHelpON()
        {
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_forceDeliveryAddr, 
                new byte[] { 
                    0xB8, 0x02, 0x0, 0x0, 0x0, 
                    0x90, 0x90, 0x90, 0x90, 0x90 });
        }

        private static void OnForceDeliveryHelpOFF()
        {
            DragonEngineLibrary.Unsafe.CPP.PatchMemory(m_forceDeliveryAddr,
                new byte[] {
                    0x41, 0x8B, 0x86, 0xD0, 0x0C, 0x0, 0x0,
                    0x83, 0xC0, 0xFE});
        }

        private static void ForcedDeliveryHActEventEnd()
        {
            ChangeToRealtime();
            DragonEngine.Log("Hact Evento bro");
        }

        public static void NotifyFighterDeath(IntPtr inf)
        {
            FighterID id = Marshal.PtrToStructure<FighterID>(inf + 0x8);
            BaseEnemyAI enemyAI = EnemyManager.GetAI(id.Handle);

            //death of an enemy
            if (enemyAI != null)
            {
                IntPtr damageInfo = Marshal.ReadIntPtr(inf);
                long attr = Marshal.ReadInt64(damageInfo + 0x8C);

                //"heavy" or "Break" attack attribute
                if ((attr & (1 << 14)) != 0 || (attr & (1 << 5)) != 0)
                {
                    if (!EffectEventManager.IsPlayingScreen(68))
                        PlayFinishingBlowEffect();

                    DragonEngine.Log("Killed by finishing blow");
                }
            }

        }

        public static void PlayFinishingBlowEffect()
        {
            EffectEventManager.PlayScreen(68); //Brawler_finishblow
            SoundManager.PlayCue(DBManager.GetSoundCuesheet("y8b_common"), 2, 0);
        }

        //We entered action for the first time
        public static void OnRealtimeBattleActionStart()
        {
            m_battleActionStartedDoOnce = true;
            ConvertAllies();

            if(PlayerCharacter.HumanModeManager.CurrentMode.ModeName == "BattleStartAction")
            {
                PlayerCharacter.HumanModeManager.ToEndReady();
            }
        }

        private static void ConvertAllies()
        {
            if (!SupporterManager.ConvertPartyMemberToSupporter)
                return;

            SupporterManager.UpdateSupporterDB();

            Fighter member1 = FighterManager.GetFighter(1);
            Fighter member2 = FighterManager.GetFighter(2);
            Fighter member3 = FighterManager.GetFighter(3);

            m_partyMemberIDs[0] = member1.Character.Attributes.player_id;
            m_partyMemberIDs[1] = member2.Character.Attributes.player_id;
            m_partyMemberIDs[2] = member3.Character.Attributes.player_id;

            ConvertAllyToSupporter(member1);
            ConvertAllyToSupporter(member2);
            ConvertAllyToSupporter(member3);
        }

        private static Character ConvertAllyToSupporter(Fighter fighter)
        {
            if (!fighter.IsValid())
                return null;

            string GetSoldierForAlly(BattleControlType ctrlType, Player.ID playerID)
            {
                switch (ctrlType)
                {
                    default:
                        return "supporter_" + playerID.ToString();
                    case BattleControlType.player:
                        return "supporter_kasuga";
                    case BattleControlType.player_kiryu:
                        return "supporter_kiryu_legend";
                    case BattleControlType.kasuga:
                        return "supporter_kasuga";
                    case BattleControlType.kiryu:
                        return "supporter_kiryu_legend";
                    case BattleControlType.adachi:
                        return "supporter_adachi";
                    case BattleControlType.nanba:
                        return "supporter_nanba";
                    case BattleControlType.tomizawa:
                        return "supporter_tomizawa";
                    case BattleControlType.chitose:
                        return "supporter_chitose";

                }
            }

            CharacterID GetSpecialCharacterIDForAlly(BattleControlType ctrlType)
            {
                //battle config 37 - wong tou fight

                switch (ctrlType)
                {
                    default:
                        return 0;
                    case BattleControlType.kiryu:
                        if (BattleConfigID == 37)
                            return (CharacterID)25111;
                        break;
                    case BattleControlType.tomizawa:
                        if (BattleConfigID == 37)
                            return (CharacterID)25112;
                        break;
                    case BattleControlType.chitose:
                        if (BattleConfigID == 37)
                            return (CharacterID)25113;
                        break;
                }

                return 0;
            }

            PoseInfo locationInf = new PoseInfo()
            {
                Position = fighter.Character.GetPosCenter(),
                Angle = fighter.Character.GetAngleY()
            };

            //TODO: Change to their costume instead of original character ID, and use their actual jobs instead of defaulting to classic
            CharacterAttributes attribs = fighter.Character.Attributes;

            CharacterID charaID;
            CharacterID specialCharaID = GetSpecialCharacterIDForAlly(attribs.ctrl_type);

            if (specialCharaID != 0)
                charaID = specialCharaID;
            else
                charaID = attribs.chara_id;

            BattleControlType controlType = attribs.ctrl_type;
            Player.ID plrID = attribs.player_id;
            uint soldierID = DBManager.GetSoldier(GetSoldierForAlly(controlType, plrID));

            ItemID weaponItem = Party.GetEquipItemID(plrID, PartyEquipSlotID.weapon);

            SupporterManager.PartyStats[plrID] = new PartyMemberTempStatStore() { AttackPower = fighter.GetStatus().AttackPower };

            NakamaManager.Change(fighter.GetPartyMemberIndex(), Player.ID.invalid);
            Character supporter = FighterManager.GenerateEnemyFighter(locationInf, soldierID, charaID);
            return supporter;
        }

        /// <summary>
        /// Called when player fighter is no longer valid.
        /// </summary>
        private static void OnBattleEnd()
        {
            BrawlerPlayer.CurrentStyle = PlayerStyle.Default;

            BrawlerPatches.Disable();
            BrawlerPatches.CombatPatches.OnCombatEnd();
            BrawlerFighterInfo.Infos.Clear();
            SpecialBattle.OnBattleEnd();

            Battling = false;
            m_battleStartedDoOnce = false;
            m_battleActionStartedDoOnce = false;
            m_actionStartedDoOnce = false;
            BattleEndedInY8BHAct = false;
            SoloBattleOnce = false;

            if (Mod.IsTurnBased())
                return;

            if (SupporterManager.ConvertPartyMemberToSupporter)
            {
                NakamaManager.Change(1, m_partyMemberIDs[0]);
                NakamaManager.Change(2, m_partyMemberIDs[1]);
                NakamaManager.Change(3, m_partyMemberIDs[2]);
            }

            SupporterManager.OnBattleEnd();
            BrawlerPlayer.OnBattleEnd();
            StopSpecialMusic(); //lets ensure
            OnForceDeliveryHelpOFF();
            UsedDeliveryOnce = false;

            OnBattleEndEvent?.Invoke();
        }

        private static void OnBattleResult()
        {
            StopSpecialMusic();
        }

        /// <summary>
        /// AllEnemies is zero during a Y8B hact
        /// </summary>
        private static void OnEnemiesDefeatedInY8BHact()
        {
            BattleEndedInY8BHAct = true;

            SoundManager.PlayCue(DBManager.GetSoundCuesheet("battle_common"), 21, 0);
            new DETaskTime(1.1f, null, true, delegate
            {
                if (!HeatActionManager.IsHAct())
                    return true;

                DragonEngine.SetSpeed(DESpeedType.Unprocessed, 0.1f);
                //If we dont also slow down these, things will look odd if they link out
                //during finish slowmo. Example: y7b1250_buki_g
                DragonEngine.SetSpeed(DESpeedType.General, 0.1f);
                DragonEngine.SetSpeed(DESpeedType.Character, 0.1f);
                DragonEngine.SetSpeed(DESpeedType.Player, 0.1f);

                return false;
            }, false);
        }

        private unsafe static void OnHActEnd()
        {
        }

        public static IntPtr DecideTurnAttacker(IntPtr turnMan, bool b1, bool b2)
        {
            if (IsBriefTurnBased())
                return PlayerFighter._ptr;

            if (m_givePlayerTurnOnce)
            {
                m_givePlayerTurnOnce = false;
                return PlayerFighter._ptr;
            }

            if(BrawlerBattleManager.NearestEnemyBehindPlayer.IsValid())
            {
                bool shouldGiveTurnToBehindEnemy = new Random().Next(0, 101) <= 20;
                
                if(shouldGiveTurnToBehindEnemy)
                    return NearestEnemyBehindPlayer._ptr;
            }

            // if (SupporterManager.Supporters.Count > 0)
            // return SupporterManager.Supporters.ElementAt(0).Value.Fighter;

            if (SupporterManager.NextSupporterAttacker != null && SupporterManager.NextSupporterAttacker.Character.IsValid())
            {
                Fighter fighter = SupporterManager.NextSupporterAttacker.Fighter;
                SupporterManager.NextSupporterAttacker = null;

                return fighter._ptr;
            }

            if(EnemyManager.ForcedAttacker != null && EnemyManager.ForcedAttacker.Character.IsValid())
            {
                Fighter fighter = EnemyManager.ForcedAttacker.Fighter;
                EnemyManager.ForcedAttacker = null;

                return fighter._ptr;
            }

            Fighter chosenEnemy = new Fighter();

            //TODO: Optimize perhaps
            Fighter[] allEnemies = FighterManager.GetAllEnemies().Where(x => !x.IsDead() && EnemyManager.GetAI(x.Character.UID) != null).ToArray();

            if (allEnemies.Length <= 0)
                return IntPtr.Zero;

            BaseEnemyAI[] enemyAIs = allEnemies.Select(x => EnemyManager.GetAI(x.Character.UID)).Where(x => x.AllowCanGetTurn()).ToArray();

            if (enemyAIs.Length == 1)
            {
                chosenEnemy = enemyAIs[0].Fighter;
                return chosenEnemy._ptr;
            }

            Random rnd = new Random();

            if (enemyAIs.Length > 1)
            {
                enemyAIs = enemyAIs.OrderBy(x => Vector3.Distance(PlayerCharacter.Transform.Position, x.Character.Transform.Position)).ToArray();
                chosenEnemy = enemyAIs[rnd.Next(0, 2)].Fighter; //focus on the nearest two enemies.
            }
            else
                chosenEnemy = enemyAIs[rnd.Next(0, allEnemies.Length)].Fighter;

            m_skippedEnemyUID = 0;

            if (chosenEnemy._ptr == IntPtr.Zero && AllEnemies.Length > 0)
                chosenEnemy = AllEnemies[0];

            return chosenEnemy._ptr;
        }

        public static void ChangeCharacter(Player.ID playerID)
        {
            if (playerID == BrawlerPlayer.CurrentPlayer)
                return;

            BrawlerPlayer.CurrentPlayer = playerID;

            PlayerCharacter = DragonEngine.GetHumanPlayer();

            if (playerID == Player.ID.kasuga)
                PlayerCharacter.GetRender().Reload((CharacterID)15286);
            else
                PlayerCharacter.GetRender().Reload((CharacterID)23350);
        }
    }
}
