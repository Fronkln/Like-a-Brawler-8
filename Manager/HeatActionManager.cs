using System;
using System.Linq;
using System.Threading.Tasks;
using DragonEngineLibrary;
using DragonEngineLibrary.Service;
using ElvisCommand;

namespace LikeABrawler2
{
    public static class HeatActionManager
    {
        private static bool m_canPerformHactDoOnce = false;
        public static HeatActionInformation PerformableHact = null;
        public static HeatActionInformation PerformingHAct = null;

        private static EHC m_testYhc = null;

        private static bool m_hactPlayingDoOnce = false;
        public static bool AwaitingHAct = false;
        public static bool IsY8BHact = false;
        public static bool ShowEnemyGaugeDoOnce = false;

        public static event Action OnHActStartEvent = null;
        public static event Action OnHActEndEvent = null;

        public static float DamageScale = 0;

        //Temp means ideally we might want to leave hacts to do actual heat reduction later down the line.
        private const int TEMP_HACT_COST = 85;
        private const int HACT_COST_KIRYU = 70;
        private const float HACT_COOLDOWN = 2.5f;

        private static float m_hactCd = 0;

        static HeatActionManager()
        {
            LoadContent();
        }

        public static void Init()
        {
            BrawlerBattleManager.OnBattleStartEvent += CalcHActDamageMultiplier;
        }
    
        public static void LoadContent()
        {
            m_testYhc = Mod.ReadYHC("yhc_test.ehc");
        }

        public static bool IsHAct()
        {
            return AwaitingHAct || BrawlerBattleManager.IsHAct;
        }

        public static long GetHActCost()
        {
            long heatMax = Player.GetHeatMax(BrawlerPlayer.CurrentPlayer);
            long heatNow = Player.GetHeatNow(BrawlerPlayer.CurrentPlayer);


            if (BrawlerPlayer.IsKasuga())
            {
                if (heatMax < TEMP_HACT_COST)
                {
                    if (!BrawlerPlayer.IsExtremeHeat)
                        return (long)(heatMax * 0.85f); //85%
                    else
                        return (long)(heatMax * 0.35f); //35%
                }
                else
                {
                    if (!BrawlerPlayer.IsExtremeHeat)
                        return TEMP_HACT_COST;
                    else
                        return (long)((TEMP_HACT_COST - (TEMP_HACT_COST * 0.2f))); //60% of base cost
                }
            }
            else
            {
                if (heatMax < HACT_COST_KIRYU)
                {
                    if (!BrawlerPlayer.IsExtremeHeat)
                        return (long)(heatMax * 0.65f); //65%
                    else
                        return (long)(heatMax * 0.2f); //20%
                }
                else
                {
                    if (!BrawlerPlayer.IsExtremeHeat)
                        return HACT_COST_KIRYU;
                    else
                        return (long)((HACT_COST_KIRYU - (HACT_COST_KIRYU * 0.2f))); //60% of base cost
                }
            }

        }


        public static void OnRequestHAct()
        {
            HeatActionManager.AwaitingHAct = true;
        }

        public static void PreUpdate()
        {

        }

        public static void Update()
        {
            if (BrawlerBattleManager.Battling && BrawlerBattleManager.CurrentPhase == BattleTurnManager.TurnPhase.Action)
            {
                if (m_hactCd > 0 && !BrawlerBattleManager.IsHAct)
                    m_hactCd -= DragonEngine.deltaTime;

                //HAct Priority, Stage -> Job -> Shared Player -> Player

                StageID stageID = SceneService.GetSceneInfo().ScenePlay.Get().StageID;
                RPGJobID job = Player.GetCurrentJob(BrawlerPlayer.CurrentPlayer);

                HeatActionInformation newPerformableHact = null;


                if (BrawlerPlayer.StageEHC.ContainsKey(stageID))
                    newPerformableHact = Iterate(BrawlerPlayer.StageEHC[stageID]);

                if(newPerformableHact == null)
                {
                    if(BrawlerPlayer.IsExtremeHeat || BrawlerPlayer.IsOtherPlayer())
                    {
                        if (BrawlerPlayer.JobEHC.ContainsKey(job))
                            newPerformableHact = Iterate(BrawlerPlayer.JobEHC[job]);
                    }
                }
                if (newPerformableHact == null)
                    newPerformableHact = Iterate(BrawlerPlayer.PlayerSharedHActs);

                if (newPerformableHact == null)
                     newPerformableHact = Iterate(BrawlerPlayer.GetCurrentPlayerHActSet());

                PerformableHact = newPerformableHact;

                bool canHact = CanHAct();

                if (!m_canPerformHactDoOnce)
                {
                    if (canHact)
                        OnCanPerformHAct(PerformableHact);
                }
                else
                {
                    if (!canHact)
                        OnCantPerformHAct();
                }
            }

            if (AwaitingHAct)
            {
                if (BrawlerBattleManager.IsHAct)
                {
                    m_hactPlayingDoOnce = true;
                    AwaitingHAct = false;
                    OnHActStartEvent?.Invoke();
                    DragonEngine.Log("HAct Start");
                }
            }
            else
            {
                if (m_hactPlayingDoOnce)
                {
                    if (!BrawlerBattleManager.IsHAct)
                    {
                        m_hactPlayingDoOnce = false;
                        OnHActEndEvent?.Invoke();
                        IsY8BHact = false;
                        DragonEngine.Log("HAct End");
                    }
                }
            }
        }

        //False = heat gauge is not visible, hacts are not permitted.
        public static bool AllowHAct()
        {
            return Player.GetLevel(BrawlerPlayer.CurrentPlayer) >= 2;
        }

        public static bool CanHAct()
        {
            return BrawlerBattleManager.Battling && m_hactCd <= 0 && !MortalReversalManager.Procedure && !BrawlerPlayer.IsInputDisableHAct() && !IsHAct() && AllowHAct() && (Player.GetHeatNow(BrawlerPlayer.CurrentPlayer) >= GetHActCost()) && PerformableHact != null;
        }


        public static HeatActionInformation Iterate(EHC ehc)
        {
            return HeatActionSimulator.Check(BrawlerBattleManager.PlayerFighter, ehc);
        }

        public static void OnCanPerformHAct(HeatActionInformation atk)
        {
            m_canPerformHactDoOnce = true;
            DragonEngine.Log("Can perform hact: " + atk.Hact.Name);
        }

        public static void OnCantPerformHAct()
        {
            m_canPerformHactDoOnce = false;
        }

        private static void OnBattleEnd()
        {
            m_hactCd = 0;
        }

        public static bool RequestTalk(HActRequestOptions opts, bool showGauge = true)
        {
            if (AwaitingHAct)
                return false;

            IsY8BHact = true;
            ShowEnemyGaugeDoOnce = showGauge;
            return HActManager.RequestHAct(opts);
        }

        public static void ExecHeatAction(HeatActionInformation info)
        {
            DragonEngine.Log("Execute hact: " + info.Hact.Name);

            if (info.UseHeat)
            {
                int newHeat = Player.GetHeatNow(BrawlerPlayer.CurrentPlayer) - (int)GetHActCost();

                if (newHeat < 0)
                    newHeat = 0;

                Player.SetHeatNow(BrawlerPlayer.CurrentPlayer, newHeat);
            }

            Vector3 hactPos = new Vector3(info.Hact.Position[0], info.Hact.Position[1], info.Hact.Position[2]);
            bool usePerformerPosition = !info.Hact.PreferHActPosition;

            PerformingHAct = info;

            HActRequestOptions opts = new HActRequestOptions();

            switch (info.Hact.SpecialType)
            {
                case HeatActionSpecialType.Normal:
                    if (info.Hact.Range == HeatActionRangeType.None)
                    {

                        if (usePerformerPosition)
                            opts.base_mtx.matrix = info.Performer.Character.GetMatrix();
                        else
                        {
                            opts.base_mtx.matrix.Position = hactPos;
                            info.Performer.Character.SetAngleY(info.Hact.RotationY);
                        }
                    }
                    else
                    {
                        opts.base_mtx.matrix = info.RangeInfo.GetMatrix();
                    }
                    break;
                case HeatActionSpecialType.Asset:
                    AssetUnit asset = AssetManager.FindNearestAssetFromAll(BrawlerBattleManager.PlayerCharacter.GetPosCenter(), 0).Get();
                    Vector3 assetPos = asset.GetPosCenter();

                    opts.base_mtx.matrix.Position = assetPos;
                    opts.base_mtx.matrix.ForwardDirection = asset.Transform.forwardDirection;
                    opts.base_mtx.matrix.LeftDirection = -asset.Transform.rightDirection;
                    opts.base_mtx.matrix.UpDirection = asset.Transform.upDirection;
                    break;

            }



            if (info.Hact.UseMatrix)
            {
                opts.base_mtx.matrix.ForwardDirection = new Vector4(info.Hact.Mtx.ForwardDirection.x,
                                                                    info.Hact.Mtx.ForwardDirection.y,
                                                                    info.Hact.Mtx.ForwardDirection.z,
                                                                    info.Hact.Mtx.ForwardDirection.w);

                opts.base_mtx.matrix.UpDirection = new Vector4(info.Hact.Mtx.UpDirection.x,
                                                               info.Hact.Mtx.UpDirection.y,
                                                               info.Hact.Mtx.UpDirection.z,
                                                               info.Hact.Mtx.UpDirection.w);

                opts.base_mtx.matrix.LeftDirection = new Vector4(info.Hact.Mtx.LeftDirection.x,
                                                     info.Hact.Mtx.LeftDirection.y,
                                                     info.Hact.Mtx.LeftDirection.z,
                                                     info.Hact.Mtx.LeftDirection.w);

                //overrides hact position
                if (!usePerformerPosition)
                    opts.base_mtx.matrix.Position = new Vector4(info.Hact.Mtx.Coordinates.x,
                                                     info.Hact.Mtx.Coordinates.y,
                                                     info.Hact.Mtx.Coordinates.z,
                                                     info.Hact.Mtx.Coordinates.w);
            }

            if (info.PosOverride != Vector3.zero)
                opts.base_mtx.matrix.Position = info.PosOverride;

            opts.id = DBManager.GetTalkParam(info.Hact.TalkParam);
            opts.is_force_play = true;

            if(opts.id == 0)
            {
                DragonEngine.Log("ERROR! TRIED TO PLAY HACT THAT DOES NOT EXIST " + info.Hact.TalkParam + " " + info.Hact.Name);
                return;
            }

            foreach (var kv in info.Map)
            {
                //TODO IMPORTANT: MAKE THIS LINEAR TIME
                if (info.Hact.Actors.FirstOrDefault(x => x.Type == kv.Key) == null)
                    continue;

                HActReplaceID replaceID = GetReplaceIDForActor(kv.Key, kv.Value);

                opts.Register(replaceID, kv.Value.Character);
                opts.RegisterWeapon(GetAssetReplaceIDForCharacter(replaceID, true), kv.Value.GetWeapon(AttachmentCombinationID.right_weapon));
                opts.RegisterWeapon(GetAssetReplaceIDForCharacter(replaceID, false), kv.Value.GetWeapon(AttachmentCombinationID.left_weapon));
            }

            SoundManager.PlayCue((SoundCuesheetID)7, 10, 0);
            RequestTalk(opts, info.ShowGauge);

            m_hactCd = HACT_COOLDOWN;
        }

        private static HActReplaceID GetReplaceIDForActor(HeatActionActorType type, Fighter performer)
        {
            switch (type)
            {
                case HeatActionActorType.Player:
                    if (performer.IsPlayer())
                        return HActReplaceID.hu_player1;
                    else
                        return HActReplaceID.hu_player1;
                case HeatActionActorType.Fighter:
                    if (!performer.IsMainPlayer() && !performer.IsPlayer())
                        return HActReplaceID.hu_npc_00;
                    else
                    {
                            return HActReplaceID.hu_player1;
                    }
                case HeatActionActorType.Enemy1:
                    return HActReplaceID.hu_enemy_00;
                case HeatActionActorType.Enemy2:
                    return HActReplaceID.hu_enemy_01;
                case HeatActionActorType.Enemy3:
                    return HActReplaceID.hu_enemy_02;
                case HeatActionActorType.Enemy4:
                    return HActReplaceID.hu_enemy_03;
                case HeatActionActorType.Enemy5:
                    return HActReplaceID.hu_enemy_04;
                case HeatActionActorType.Enemy6:
                    return HActReplaceID.hu_enemy_05;
                case HeatActionActorType.Ally1:
                    return HActReplaceID.hu_npc_00;
                case HeatActionActorType.Ally2:
                    return HActReplaceID.hu_npc_01;
                case HeatActionActorType.Ally3:
                    return HActReplaceID.hu_npc_02;
                case HeatActionActorType.Ally4:
                    return HActReplaceID.hu_npc_03;
            }

            return HActReplaceID.invalid;
        }

        private static AuthAssetReplaceID GetAssetReplaceIDForCharacter(HActReplaceID ID, bool right)
        {
            switch (ID)
            {
                case HActReplaceID.hu_player:
                    if (right)
                        return AuthAssetReplaceID.we_player_r;
                    else
                        return AuthAssetReplaceID.we_player_l;
                case HActReplaceID.hu_player1:
                    if (right)
                        return AuthAssetReplaceID.we_player1_r;
                    else
                        return AuthAssetReplaceID.we_player1_l;
                case HActReplaceID.hu_enemy_00:
                    if (right)
                        return AuthAssetReplaceID.we_enemy_00_r;
                    else
                        return AuthAssetReplaceID.we_enemy_00_l;

                case HActReplaceID.hu_enemy_01:
                    if (right)
                        return AuthAssetReplaceID.we_enemy_01_r;
                    else
                        return AuthAssetReplaceID.we_enemy_01_l;

                case HActReplaceID.hu_enemy_02:
                    if (right)
                        return AuthAssetReplaceID.we_enemy_02_r;
                    else
                        return AuthAssetReplaceID.we_enemy_02_l;

                case HActReplaceID.hu_enemy_03:
                    if (right)
                        return AuthAssetReplaceID.we_enemy_03_r;
                    else
                        return AuthAssetReplaceID.we_enemy_03_l;
                case HActReplaceID.hu_enemy_04:
                    if (right)
                        return AuthAssetReplaceID.we_enemy_04_r;
                    else
                        return AuthAssetReplaceID.we_enemy_04_l;
                case HActReplaceID.hu_npc_00:
                    if (right)
                        return AuthAssetReplaceID.we_npc_00_r;
                    else
                        return AuthAssetReplaceID.we_npc_00_l;
                case HActReplaceID.hu_npc_01:
                    if (right)
                        return AuthAssetReplaceID.we_npc_01_r;
                    else
                        return AuthAssetReplaceID.we_npc_01_l;
                case HActReplaceID.hu_npc_02:
                    if (right)
                        return AuthAssetReplaceID.we_npc_02_r;
                    else
                        return AuthAssetReplaceID.we_npc_02_l;
                case HActReplaceID.hu_npc_03:
                    if (right)
                        return AuthAssetReplaceID.we_npc_03_r;
                    else
                        return AuthAssetReplaceID.we_npc_03_l;
            }

            return AuthAssetReplaceID.invalid;
        }

        public static void CalcHActDamageMultiplier()
        {
            uint playerLevel = Player.GetLevel(BrawlerBattleManager.PlayerCharacter.Attributes.player_id);
            float mult = 1;
            int numIncrease = 0;

            for (int i = 7; i < 61 && i < playerLevel; i += 4)
                numIncrease++;

            mult = (float)Math.Pow(1.255f, numIncrease);
            DamageScale = mult;

            DragonEngine.Log("Multiplier: " + mult + "\n20 damage with multiplier: " + 20 * mult);
        }
    }
}
