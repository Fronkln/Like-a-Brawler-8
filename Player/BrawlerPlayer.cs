using DragonEngineLibrary;
using DragonEngineLibrary.Service;
using ElvisCommand;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    public static class BrawlerPlayer
    {
        public static Player.ID CurrentPlayer = 0;
        public static PlayerStyle CurrentStyle = PlayerStyle.NotApplicable;
        public static RPGJobID CurrentJob { get { return Player.GetCurrentJob(CurrentPlayer); } }

        public static CharacterAttributes OriginalPlayerAttributes;

        public static Dictionary<StageID, EHC> StageEHC = new Dictionary<StageID, EHC>();
        public static Dictionary<RPGJobID, EHC> JobEHC = new Dictionary<RPGJobID, EHC>();
        public static EHC PlayerSharedHActs;
        public static Dictionary<Player.ID, EHC> PlayerHActs;
        public static EHC[] DODHActs;

        public static EntityHandle<AssetUnit> PocketWeapon = new EntityHandle<AssetUnit>();

        //private static EHC KasugaHAct;
        //private static EHC AdachiHAct;

        private static bool m_isAttackingDoOnce = false;

        private static bool m_getupHyperArmorDoOnce = false;
        private static bool m_transitMortal = false;

        public static bool GodMode = false;

        public static bool IsExtremeHeat = false;

        public static event Action OnPerfectGuard;
        public static event Action OnStartAttack;

        private delegate void FighterSetupWeapon(IntPtr fighter);
        private static FighterSetupWeapon SetupWeapon;

        static BrawlerPlayer()
        {
            LoadContent();
            SetupWeapon = Marshal.GetDelegateForFunctionPointer<FighterSetupWeapon>(DragonEngineLibrary.Unsafe.CPP.PatternSearch("40 53 56 41 55 41 56 48 83 EC ? 4C 8B 09"));
        }

        public static bool IsInputSway(HumanModeManager humanModeManager)
        {
            //TEMP
            return BattleManager.PadInfo.IsJustPush(BattleButtonID.sway);
        }

        public static bool IsInputDisableHAct()
        {
            return BattleManager.PadInfo.CheckCommand(BattleButtonID.npc, 1, 1000, 0)
             && !BattleManager.PadInfo.CheckCommand(BattleButtonID.npc, 2, 1000, 0);
        }

        public static bool IsInputGuard(HumanModeManager humanModeManager)
        {
            //
            return BattleManager.PadInfo.CheckCommand(BattleButtonID.guard, 1, 1000, 0)
                && !BattleManager.PadInfo.CheckCommand(BattleButtonID.guard, 2, 1000, 0);
        }

        public static bool IsInputDeliveryHelp()
        {
            PadInputInfo inf = BattleManager.PadInfo;
            return inf.IsHold(BattleButtonID.guard) && BattleManager.PadInfo.IsJustPush(BattleButtonID.down);
        }

        public static bool IsKiryu()
        {
            if (!BrawlerBattleManager.Battling)
                return BrawlerBattleManager.PlayerCharacter.Attributes.player_id == Player.ID.kiryu;

            return CurrentPlayer == Player.ID.kiryu;
        }

        public static bool IsKasuga()
        {
            if (!BrawlerBattleManager.Battling)
                return BrawlerBattleManager.PlayerCharacter.Attributes.player_id == Player.ID.kasuga;

            return CurrentPlayer == Player.ID.kasuga;
        }

        public static bool IsDragon()
        {
            return Player.GetCurrentJob(CurrentPlayer) == RPGJobID.kiryu_01;
        }

        public static bool IsOtherPlayer()
        {
            return !IsKiryu() && !IsKasuga();
        }

        public static bool IsOtherPlayerLeader()
        {
            return IsOtherPlayer() && DragonEngine.GetHumanPlayer().UID == BrawlerBattleManager.PlayerCharacter.UID;
        }

        public static bool AllowStyleChange()
        {
            if (SpecialBattle.IsDreamSequence())
                return false;

            if (!TutorialManager.Active)
                return true;

            return !TutorialManager.CurrentGoal.Modifier.HasFlag(TutorialModifier.DontAllowStyleChange);
        }

        public static bool CanExtremeHeat()
        {
            if (BrawlerFighterInfo.Player.IsSync)
                return false;

            if (SpecialBattle.IsDreamSequence())
                return false;

            //Adachi
            if (IsOtherPlayer() && GetNormalMovesetForPlayer(CurrentPlayer) == RPG.GetJobCommandSetID(CurrentPlayer, Player.GetCurrentJob(CurrentPlayer)))
                return false;

            if (TutorialManager.Active)
                if (TutorialManager.CurrentGoal.Modifier.HasFlag(TutorialModifier.DontAllowStyleChange))
                    return false;

            if (IsKasuga())
                return TimelineManager.CheckClockAchievement(1, 78, 47) || Player.GetCurrentJob(CurrentPlayer) == RPGJobID.kasuga_braver;
            else
            {
                if (IsDragon())
                    return TimelineManager.CheckClockAchievement(53, 27, 6);
                else
                    return true;
            }
        }

        public static void LoadContent()
        {
            PlayerSharedHActs = YazawaCommandManager.LoadYHC("player/shared.ehc");

            DODHActs = new EHC[3]
            {
                YazawaCommandManager.LoadYHC("player/kiryu_legend.ehc"),
                YazawaCommandManager.LoadYHC("player/kiryu_rush.ehc"),
                YazawaCommandManager.LoadYHC("player/kiryu_crash.ehc"),
            };

            PlayerHActs = new Dictionary<Player.ID, EHC>();

            //ass code lol
            foreach (string player in Enum.GetNames(typeof(Player.ID)))
            {
                string playerEhcPathRel = Path.Combine("player/", player + ".ehc");
                string playerEhcPathFull = Path.Combine(Mod.ModPath, "battle", "ehc", playerEhcPathRel);

                if (File.Exists(playerEhcPathFull))
                {
                    EHC playerEHCFile = YazawaCommandManager.LoadYHC(playerEhcPathFull);
                    PlayerHActs[(Player.ID)Enum.Parse(typeof(Player.ID), player)] = playerEHCFile;
                }
            }

            StageEHC = new Dictionary<StageID, EHC>()
            {
                [StageID.st_kamuro] = YazawaCommandManager.LoadYHC("stage/kamuro.ehc")
            };

            JobEHC = new Dictionary<RPGJobID, EHC>();

            //ass code lol
            foreach (string job in Enum.GetNames(typeof(RPGJobID)))
            {
                string jobEhcPathRel = Path.Combine("job/", job + ".ehc");
                string jobEhcPathFull = Path.Combine(Mod.ModPath, "battle", "ehc", jobEhcPathRel);

                if (File.Exists(jobEhcPathFull))
                {
                    EHC jobEHCFile = YazawaCommandManager.LoadYHC(jobEhcPathFull);
                    JobEHC[(RPGJobID)Enum.Parse(typeof(RPGJobID), job)] = jobEHCFile;
                }
            }
        }

        public static EHC GetCurrentPlayerHActSet()
        {
            Weapon weapon = BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon);

            bool isKiryu = IsKiryu();
            bool isKasuga = IsKasuga();

            if (weapon.Unit.IsValid())
                return WeaponManager.GetEHCSetForWeapon(Asset.GetArmsCategory(weapon.Unit.Get().AssetID));

            if (IsDragon())
            {
                switch (CurrentStyle)
                {
                    default:
                        return DODHActs[0];

                    case PlayerStyle.Default:
                        return DODHActs[0];
                    case PlayerStyle.Legend:
                        return DODHActs[0];
                    case PlayerStyle.Rush:
                        return DODHActs[1];
                    case PlayerStyle.Beast:
                        return DODHActs[2];
                }
            }

            if (PlayerHActs.ContainsKey(CurrentPlayer))
                return PlayerHActs[CurrentPlayer];

            //Not DOD job, but still default style hact for him
            if (isKiryu)
                return DODHActs[0];


            return null;
        }

        public static void OnBattleStart()
        {
            Character player = BrawlerBattleManager.PlayerCharacter;

            IsExtremeHeat = false;
            ToNormalMoveset();

            if (IsKiryu() || Player.GetCurrentJob(CurrentPlayer) == RPGJobID.kiryu_01)
                CurrentStyle = PlayerStyle.Default;
            else
                CurrentStyle = PlayerStyle.NotApplicable;
        }

        public static void OnBattleEnd()
        {
            m_isAttackingDoOnce = false;
            IsExtremeHeat = false;
            CurrentStyle = PlayerStyle.Default;

            BrawlerPlayer.ToNormalMoveset();
        }

        public static void Update()
        {
            Fighter player = BrawlerBattleManager.PlayerFighter;

            if (!player.IsValid() || Mod.IsTurnBased())
                return;

            unsafe
            {
                bool* free_movement_mode = (bool*)(player.Character.Pointer.ToInt64() + 0x11A3);
                *free_movement_mode = AllowPlayerMovement();
            }

            HeatModule.Update();

            if (BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action)
                CombatUpdate();

            player.GetStatus().RemoveExEffect(1, true, true);
            player.GetStatus().RemoveExEffect(2, true, true);
            player.GetStatus().RemoveExEffect(3, true, true);
            player.GetStatus().RemoveExEffect(4, true, true);
            player.GetStatus().RemoveExEffect(5, true, true);
            player.GetStatus().RemoveExEffect(12, true, true);
        }

        //OnAttackHit/OnAttackLand
        public static void OnHitEnemy(Fighter enemy, BattleDamageInfoSafe dmg)
        {
            if (!IsExtremeHeat)
            {
                int curHeat = Player.GetHeatNow(CurrentPlayer);
                int maxHeat = Player.GetHeatMax(CurrentPlayer);

                //player will recover 8% heat for each hit
                //TODO: make this fair for both early and late game by adding heat gain upgrade?
                if (curHeat < maxHeat)
                    Player.SetHeatNow(CurrentPlayer, curHeat + (int)(maxHeat * 0.08f));

                if (Player.GetHeatNow(CurrentPlayer) > maxHeat)
                    Player.SetHeatNow(CurrentPlayer, maxHeat);
            }
        }


        public unsafe static void PullOutWeapon(ItemID weapon)
        {
            AssetID assetId = Item.GetAssetID(weapon);

            if (assetId <= 0)
                return;

            AssetArmsCategoryID category = Asset.GetArmsCategory(assetId);
            DragonEngine.Log("Equipping inventory weapon, category: " + category + " Asset ID: " +(uint) assetId);

            switch (category)
            {
                default:
                    BrawlerBattleManager.PlayerFighter.Equip(weapon, AttachmentCombinationID.right_weapon);
                    break;
                case AssetArmsCategoryID.X:
                    BrawlerBattleManager.PlayerFighter.Equip(weapon, AttachmentCombinationID.right_weapon);
                    BrawlerBattleManager.PlayerFighter.Equip(weapon, AttachmentCombinationID.left_weapon);
                    break;
                case AssetArmsCategoryID.M:
                    BrawlerBattleManager.PlayerFighter.Equip(weapon, AttachmentCombinationID.right_weapon);
                    BrawlerBattleManager.PlayerFighter.Equip(weapon, AttachmentCombinationID.left_weapon);
                    break;
            }

            PocketWeapon = BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit;

            if(PocketWeapon.IsValid())
                PocketWeapon.Get().Arms.SetFromPocket(true);

            BattleTurnManager.ForceCounterCommand(BrawlerBattleManager.PlayerFighter, BrawlerBattleManager.AllEnemies[0], DBManager.GetSkill($"player_wp{category.ToString().ToLowerInvariant()}_battou"));
        }

        public static BattleCommandSetID GetCommandSetForJob(Player.ID playerID, RPGJobID id)
        {
            switch (id)
            {
                default:
                    return RPG.GetJobCommandSetID(playerID, id);
                case RPGJobID.kasuga_freeter:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_kasuga_brawler");
                case RPGJobID.kasuga_braver:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_kasuga_hero_brawler");
            }
        }

        public static bool DoesJobHaveWeapons(RPGJobID id)
        {
            return id != RPGJobID.man_actionstar && id != RPGJobID.kiryu_01 && id != RPGJobID.kasuga_freeter;
        }

        public static BattleCommandSetID GetNormalMovesetForPlayer(Player.ID player)
        {
            switch (player)
            {
                default:
                    return RPG.GetJobCommandSetID(player, Player.GetCurrentJob(player));

                case Player.ID.kasuga:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_kasuga_brawler");
                case Player.ID.kiryu:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_kiryu_legend_brawler");
                case Player.ID.saeko:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_saeko_job_01");
                case Player.ID.adachi:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_adachi_job_01");
                case Player.ID.sonhi:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_sonhi_01");
                case Player.ID.nanba:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_nanba_job_01");
                case Player.ID.chitose:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_chitose_job_01");
                case Player.ID.chou:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_chou_job_01");
                case Player.ID.tomizawa:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_tomizawa_job_01");
                case Player.ID.jyungi:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_jyungi_job_01");
            }
        }

        public static void ToNormalMoveset()
        {
            BrawlerBattleManager.PlayerCharacter.HumanModeManager.CommandsetModel.SetCommandSet(0, GetNormalMovesetForPlayer(CurrentPlayer));

            //Yakuza 8 limitation: we cannot get the equipped weapon for another job, it has to be another one
            if (DoesJobHaveWeapons(Player.GetCurrentJob(CurrentPlayer)))
                if (IsOtherPlayer())
                    if (BrawlerBattleManager.PlayerFighter.IsValid())
                        SetupWeapon(BrawlerBattleManager.PlayerFighter._ptr);

            //otherwise, dont do shit, if this is a party member that is the main playable character right now for some reason
            //its modder's responsibility to edit the player cfc.
        }

        public static bool CanAct()
        {
            BrawlerFighterInfo inf = BrawlerFighterInfo.Player;

            return !inf.IsDown && !inf.IsGettingUp && !inf.IsAttack && !BrawlerBattleManager.PlayerCharacter.HumanModeManager.IsDamage();
        }

        public unsafe static void CombatUpdate()
        {
            if (CanAct() && !Mod.IsTurnBased())
            {
                if (IsInputDeliveryHelp())
                {
                    BrawlerBattleManager.IsDeliveryHelping = true;
                    BrawlerBattleManager.OnForceDeliveryHelpON();
                    BrawlerBattleManager.ChangeToTurnBased();
                    BrawlerBattleManager.UsedDeliveryOnce = true;
                }
            }


            if (GodMode)
            {
                Player.SetHeatNow(CurrentPlayer, Player.GetHeatMax(CurrentPlayer));
                BrawlerBattleManager.PlayerFighter.GetStatus().SetHPCurrent(Player.GetHPMax(CurrentPlayer));

                Fighter p1 = FighterManager.GetFighter(1);
                Fighter p2 = FighterManager.GetFighter(2);
                Fighter p3 = FighterManager.GetFighter(3);

                p1.GetStatus().SetHPCurrent(p1.GetStatus().MaxHP);
                p2.GetStatus().SetHPCurrent(p2.GetStatus().MaxHP);
                p3.GetStatus().SetHPCurrent(p3.GetStatus().MaxHP);
            }

            EXModule.Update();
            UpdateTargeting(BrawlerBattleManager.PlayerFighter);

            GameInputUpdate();


            if (BattleManager.PadInfo.IsJustPush(BattleButtonID.down) && !BattleManager.PadInfo.IsHold(BattleButtonID.guard))
            {
                RPGJobID curJob = Player.GetCurrentJob(CurrentPlayer);

                if (curJob == RPGJobID.kasuga_freeter || curJob == RPGJobID.kiryu_01)
                {
                    DragonEngine.Log("Player equips job weapon");
                    ItemID weapon = Party.GetEquipItemID(CurrentPlayer, PartyEquipSlotID.weapon);

                    if (weapon != 0)
                        PullOutWeapon(weapon);
                }

            }


            if (IsExtremeHeat)
                BrawlerBattleManager.PlayerFighter.GetStatus().SetSuperArmor(true);

            if (!m_getupHyperArmorDoOnce)
            {
                if (BrawlerFighterInfo.Player.IsGettingUp)
                {
                    if (!IsExtremeHeat)
                        BrawlerBattleManager.PlayerFighter.GetStatus().SetSuperArmor(true);

                    //OnPlayerStartGettingUp?.Invoke();

                    m_getupHyperArmorDoOnce = true;
                }
            }
            else
            {
                if (!BrawlerFighterInfo.Player.IsGettingUp)
                {
                    if (!IsExtremeHeat)
                        BrawlerBattleManager.PlayerFighter.GetStatus().SetSuperArmor(false);

                    m_getupHyperArmorDoOnce = false;
                }

            }

            MortalReversalManager.Update();

            var fighterInf = BrawlerFighterInfo.Player;


            if (fighterInf.Fighter != null)
            {
                if (!m_isAttackingDoOnce)
                {
                    if (fighterInf.IsAttack)
                    {
                        m_isAttackingDoOnce = true;
                        OnStartAttack?.Invoke();
                    }
                }
                else
                {
                    if (!fighterInf.IsAttack)
                        m_isAttackingDoOnce = false;
                }
            }

        }

        public static bool AllowPlayerMovement()
        {
            if (SpecialBattle.IsDreamSequenceStart())
                return false;

            return true;
        }

        private static void GameInputUpdate()
        {
            if (BattleManager.PadInfo.IsJustPush(BattleButtonID.run))
            {
                if (CanExtremeHeat())
                {
                    if (!IsExtremeHeat)
                    {
                        if (Player.GetHeatNow(CurrentPlayer) > 0)
                            OnExtremeHeatModeON();
                    }
                    else
                        OnExtremeHeatModeOFF();
                }
            }

            if (CanStyleSwitch() && AllowStyleChange())
            {
                if (BattleManager.PadInfo.IsJustPush(BattleButtonID.left))
                {
                    // BrawlerBattleManager.PlayerCharacter.HumanModeManager.ToStyleChange(1);
                    OnStyleSwitch(PlayerStyle.Rush);
                }

                if (BattleManager.PadInfo.IsJustPush(BattleButtonID.up))
                {
                    //BrawlerBattleManager.PlayerCharacter.HumanModeManager.ToStyleChange(2);
                    OnStyleSwitch(PlayerStyle.Default);
                }

                if (BattleManager.PadInfo.IsJustPush(BattleButtonID.right))
                {
                    // BrawlerBattleManager.PlayerCharacter.HumanModeManager.ToStyleChange(3);
                    OnStyleSwitch(PlayerStyle.Beast);
                }
            }
        }

        public static bool CanStyleSwitch()
        {
            return IsDragon();
        }

        public static bool CanExtremeHeatMode()
        {
            return true;
        }

        public static bool AllowDamage(BattleDamageInfo inf)
        {
            if (GodMode)
                return false;

            return true;
        }

        public static void InputUpdate()
        {
            if (!BrawlerBattleManager.Battling || BrawlerBattleManager.CurrentPhase != BattleTurnManager.TurnPhase.Action)
                return;

            if (BattleManager.PadInfo.IsJustPush(BattleButtonID.heavy))
                if (HeatActionManager.CanHAct())
                    HeatActionManager.ExecHeatAction(HeatActionManager.PerformableHact);
        }


        public static void OnExtremeHeatModeON()
        {
            IsExtremeHeat = true;
            BrawlerBattleManager.OnPlayerEnterEXHeat();

            Character playerChara = BrawlerBattleManager.PlayerCharacter;

            if (!IsDragon())
            {
                BrawlerBattleManager.PlayerFighter.Character.GetRender().BattleTransformationOn();
                //EquipJobWeapons(Player.GetCurrentJob(playerChara.Attributes.player_id));

                switch (Player.GetCurrentJob(CurrentPlayer))
                {
                    default:
                        SetupWeapon(BrawlerBattleManager.PlayerFighter._ptr);
                        break;
                    case RPGJobID.man_western:
                        EquipJobWeapons(RPGJobID.man_western);
                        break;

                }

                BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().Arms.SetFromPocket(true);
                BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.left_weapon).Unit.Get().Arms.SetFromPocket(true);


                //Commandset hackery to allow other players to use someone elses jobs
                //Example: Saeko on adachi
                RPGJobID currentJob = Player.GetCurrentJob(CurrentPlayer);
                string currentJobName = currentJob.ToString();
                Player.ID targetPlayerID = CurrentPlayer;

                if (currentJobName.StartsWith("woman", StringComparison.OrdinalIgnoreCase))
                    targetPlayerID = Player.ID.saeko;
                else if (currentJobName.StartsWith("man"))
                    targetPlayerID = Player.ID.kasuga;
                else
                {
                    switch (currentJob)
                    {
                        case RPGJobID.adachi_01:
                            targetPlayerID = Player.ID.adachi;
                            break;
                        case RPGJobID.saeko_01:
                            targetPlayerID = Player.ID.saeko;
                            break;
                        case RPGJobID.sonhi_01:
                            targetPlayerID = Player.ID.sonhi;
                            break;
                        case RPGJobID.kiryu_01:
                            targetPlayerID = Player.ID.kiryu;
                            break;
                        case RPGJobID.chou_01:
                            targetPlayerID = Player.ID.chou;
                            break;
                        case RPGJobID.chitose_01:
                            targetPlayerID = Player.ID.chitose;
                            break;
                        case RPGJobID.tomizawa_01:
                            targetPlayerID = Player.ID.tomizawa;
                            break;
                        case RPGJobID.jyungi_01:
                            targetPlayerID = Player.ID.jyungi;
                            break;
                    }
                }

                uint commandSet = (uint)GetCommandSetForJob(targetPlayerID, Player.GetCurrentJob(CurrentPlayer));
                playerChara.HumanModeManager.CommandsetModel.SetCommandSet(0, (BattleCommandSetID)commandSet);
                playerChara.Components.EffectEvent.Get().PlayEventOverride(EffectEventCharaID.YZ_Chara_Cange01);
            }
            else
                OnStyleSwitch(PlayerStyle.Legend);
        }

        public static void OnExtremeHeatModeOFF()
        {
            if (!IsExtremeHeat)
                return;

            IsExtremeHeat = false;
            BrawlerBattleManager.OnPlayerExitEXHeat();


            if (!IsDragon())
            {
                Fighter playerFighter = BrawlerBattleManager.PlayerFighter;
                Character playerChara = playerFighter.Character;

                BrawlerBattleManager.PlayerFighter.Character.GetRender().Reload(CharacterID.m_dummy, 7, true);
                //BrawlerBattleManager.PlayerFighter.Character.GetRender().ReadyBattleTransform(false);

                var wep1 = playerFighter.GetWeapon(AttachmentCombinationID.right_weapon);
                var wep2 = playerFighter.GetWeapon(AttachmentCombinationID.left_weapon);

                BrawlerBattleManager.PlayerFighter.DropWeapon(new DropWeaponOption(AttachmentCombinationID.left_weapon, false));
                BrawlerBattleManager.PlayerFighter.DropWeapon(new DropWeaponOption(AttachmentCombinationID.right_weapon, false));

                wep1.Unit.Get().DestroyEntity();
                wep2.Unit.Get().DestroyEntity();
                ToNormalMoveset();

                playerChara.Components.EffectEvent.Get().PlayEventOverride(EffectEventCharaID.YZ_Chara_Cange01);
            }
            else
            {
                OnStyleSwitch(PlayerStyle.Default);
            }

            BrawlerBattleManager.PlayerFighter.GetStatus().SetSuperArmor(false);
        }

        public static void OnStyleSwitch(PlayerStyle newStyle, bool quick = false)
        {
            if (CurrentStyle == newStyle)
                return;

            PlayerStyle overrideStyle = newStyle;

            RPGSkillID styleAnim = 0;
            ushort styleSoundCue = 0;
            ushort styleSoundIdx = 0;

            uint styleCommandSet = 0;

            switch (newStyle)
            {
                case PlayerStyle.Default:
                    styleAnim = DBManager.GetSkill("kiryu_to_legend");
                    styleSoundCue = DBManager.GetSoundCuesheet("bbg_b");
                    styleSoundIdx = 1;
                    styleCommandSet = FighterCommandManager.FindSetID("p_kiryu_legend_brawler");
                    break;

                case PlayerStyle.Legend:
                    styleAnim = DBManager.GetSkill("kiryu_to_legend");
                    styleCommandSet = FighterCommandManager.FindSetID("p_kiryu_legend_1988_brawler");
                    overrideStyle = PlayerStyle.Default;
                    BrawlerBattleManager.PlaySpecialMusic(DBManager.GetSoundCuesheet("bbg_k"), 1);

                    //BrawlerBattleManager.PlayerCharacter.HumanModeManager.ToAttackMode(new FighterCommandID((ushort)styleCommandSet, (ushort)FighterCommandManager.GetCommandID(styleCommandSet, "StyleStart")));
                    break;

                case PlayerStyle.Rush:
                    styleAnim = DBManager.GetSkill("kiryu_to_rush");
                    styleSoundCue = DBManager.GetSoundCuesheet("bbg_b");
                    styleSoundIdx = 4;
                    styleCommandSet = FighterCommandManager.FindSetID("p_kiryu_rush_brawler");
                    break;

                case PlayerStyle.Beast:
                    styleAnim = DBManager.GetSkill("kiryu_to_crash");
                    styleSoundCue = DBManager.GetSoundCuesheet("bbg_b");
                    styleSoundIdx = 6;
                    styleCommandSet = FighterCommandManager.FindSetID("p_kiryu_crash_brawler");
                    break;

                case PlayerStyle.Resurgence:
                    styleCommandSet = FighterCommandManager.FindSetID("p_kiryu_legend");
                    break;
            }

            if (IsExtremeHeat && newStyle != PlayerStyle.Legend)
                OnExtremeHeatModeOFF();

            if (!quick)
            {
                short command = FighterCommandManager.GetSet(styleCommandSet).FindCommandInfo("StyleStart");
                if (command >= 0)
                {
                    BrawlerBattleManager.PlayerCharacter.HumanModeManager.ToEndReady();
                    BrawlerBattleManager.PlayerCharacter.HumanModeManager.ToAttackMode(new FighterCommandID((ushort)styleCommandSet, command));
                }
            }

            BrawlerBattleManager.PlayerFighter.DropWeapon(new DropWeaponOption(AttachmentCombinationID.left_weapon, false));
            BrawlerBattleManager.PlayerFighter.DropWeapon(new DropWeaponOption(AttachmentCombinationID.right_weapon, false));

            CurrentStyle = newStyle;
        }

        public static Fighter GetLockOnTarget(Fighter kasugaFighter)
        {
            try
            {
                if (BrawlerBattleManager.DisableTargetingOnce)
                    return new Fighter(IntPtr.Zero);

                if (BrawlerBattleManager.AllEnemiesNearest.Length <= 0)
                    return new Fighter(IntPtr.Zero);


                Fighter[] nearestEnemies = BrawlerBattleManager.AllEnemiesNearest;

                float dot = -0.6f;

                if (nearestEnemies.Length == 1)
                {
                    dot = -0.8f;

                    if (kasugaFighter.Character.IsFacingEntity(nearestEnemies[0].Character, dot))
                        return nearestEnemies[0];
                    else
                        return new Fighter(IntPtr.Zero);
                }

                foreach (Fighter enemy in nearestEnemies)
                {
                    if (enemy.IsDead() || !kasugaFighter.Character.IsFacingEntity(enemy.Character, dot))
                        continue;

                    float dist = Vector3.Distance(kasugaFighter.Character.GetPosCenter(), enemy.Character.GetPosCenter());

                    if (dist >= 4.5)
                        continue;

                    return enemy;
                }

                return new Fighter(IntPtr.Zero);
            }
            catch
            {
                return new Fighter(IntPtr.Zero);
            }
        }

        public static void UpdateTargeting(Fighter playerFighter)
        {
            ECBattleTargetDecide targetDecide = playerFighter.Character.TargetDecide;
            targetDecide.SetTarget(new FighterID() { Handle = GetLockOnTarget(playerFighter).Character.UID });
        }

        public static bool TransitDamage(BattleDamageInfoSafe safeDmg)
        {
            BaseEnemyAI ai = EnemyManager.GetAI(safeDmg.Attacker.UID);

            if (ai != null && ai.IsMortalAttack())
            {
                if (BattleManager.PadInfo.IsTimingPush(BattleButtonID.guard, 2500))
                {
                    MortalReversalManager.Transit = true;
                    MortalReversalManager.Attacker = EnemyManager.GetAI(safeDmg.Attacker.UID).Fighter;

                    return false;
                }
            }

            return true;
        }

        public unsafe static void OnGetHit(BattleDamageInfoSafe dmg)
        {
            bool isJustGuard = *((bool*)(dmg._ptr.ToInt64() + 0x109));

            if (isJustGuard)
            {
                *(int*)(dmg._ptr.ToInt64() + 0x120) = 0;
                *(int*)(dmg._ptr.ToInt64() + 0x124) = 0;

                OnPerfectGuard?.Invoke();
            }

            int maxHeat = Player.GetHeatMax(Player.ID.kasuga);

            float reductionAmount = !BrawlerBattleManager.PlayerCharacter.HumanModeManager.IsGuarding() ? 0.1f : 0f;
            int reducedHeat = Player.GetHeatNow(Player.ID.kasuga) - (int)(maxHeat * reductionAmount);

            if (reducedHeat < 0)
                reducedHeat = 0;

            //Player will lose 9.5% heat for each hit they take
            Player.SetHeatNow(Player.ID.kasuga, reducedHeat);

            BaseEnemyAI attacker = EnemyManager.GetAI(dmg.Attacker.UID);

            if (attacker != null && attacker.IsMortalAttack())
            {
                EffectEventManager.PlayScreen(28);
                DragonEngine.Log("YEOWCH! MORTAL ATTACK");
            }
        }

        public static void EquipJobWeapons(RPGJobID id)
        {
            ItemID itemId = Party.GetEquipItemID(BrawlerBattleManager.PlayerCharacter.Attributes.player_id, PartyEquipSlotID.weapon);

            switch (id)
            {
                default:
                    BrawlerBattleManager.PlayerFighter.Equip(Item.GetAssetID(itemId), AttachmentCombinationID.right_weapon, itemId, RPGSkillID.invalid);
                    break;
                case RPGJobID.kasuga_freeter:
                    break;
                case RPGJobID.man_western:
                    BrawlerBattleManager.PlayerFighter.Equip(Item.GetAssetID(itemId), AttachmentCombinationID.right_weapon, itemId, DBManager.GetSkill("job_western_skill_01"));
                    // BrawlerBattleManager.PlayerFighter.Equip((AssetID)1353, AttachmentCombinationID.right_weapon, ItemID.invalid, DBManager.GetSkill("western_test"));
                    break;
            }
        }

        public unsafe static void CalculateTameDamage(IntPtr battleDamageInfo)
        {
            if (!AuthNodeBattleTame.ShouldApplyTame())
                return;

            int damage = (*(int*)(battleDamageInfo.ToInt64() + 0x120));
            damage = damage + (int)(damage * 0.15f); //15% damage boost increase
            *(int*)(battleDamageInfo.ToInt64() + 0x120) = damage;
            *(int*)(battleDamageInfo.ToInt64() + 0x124) = damage;

            //consume the tame
            AuthNodeBattleTame.Reset();
        }
    }
}
