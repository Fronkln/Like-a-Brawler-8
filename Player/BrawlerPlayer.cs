using DragonEngineLibrary;
using DragonEngineLibrary.Service;
using ElvisCommand;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace LikeABrawler2
{
#warning TODO: Slowly make this not a static class
    public class BrawlerPlayer
    {
        public Character Character;
        public EntityHandle<Character> CharacterHandle;
        public Fighter Fighter;

        public uint PartyMemberIndex;
        public Player.ID PlayerID;

        public static PlayerStyle CurrentStyle = PlayerStyle.NotApplicable;

        public BrawlerFighterInfo FighterInfo { get { return BrawlerFighterInfo.Get(CharacterHandle.UID); } }

        public HActRangeInfo LastPlainRange;
        public bool IsInPlain;

        public RPGJobID CurrentJob { get { return Player.GetCurrentJob(PlayerID); } }

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

        public delegate void FighterSetupWeapon(IntPtr fighter);
        public static FighterSetupWeapon SetupWeapon;

        public static Fighter LastLockedInEnemy;
        public static Fighter LastBehindEnemy;

        static BrawlerPlayer()
        {
            LoadContent();
            SetupWeapon = Marshal.GetDelegateForFunctionPointer<FighterSetupWeapon>(DragonEngineLibrary.Unsafe.CPP.PatternSearch("40 53 56 41 55 41 56 48 83 EC ? 4C 8B 09"));
        }

        public void Update()
        {
            if (CharacterHandle.IsValid())
            {
                Character = CharacterHandle.Get();
                Fighter = Character.GetFighter();
            }
            else
            {
                Character = new Character();
                Fighter = new Fighter();
            }

            if (!Fighter.IsValid() || Mod.IsTurnBased())
                return;

            CombatUpdate();
        }

        public void OnSpawn()
        {
            //Ichiban animations are too silly on others.
            if (IsOtherPlayer())
                Character.HumanModeManager.CommandsetModel.SetCommandSet(1, (BattleCommandSetID)DBManager.GetCommandSet("p_kiryu_legend"));
        }

        private void CombatUpdate()
        {
            HActRangeInfo rangeTemp = new HActRangeInfo();

            //plain range is no longer reliable in yakuza 8
            //can be "true" on instances where it certainly shouldnt be
            //so we have to do it in a less optimal manner...
            var hactModule = Fighter.GetStatus().HAct;

            IsInPlain =
                !hactModule.GetPlayInfo(ref rangeTemp, HActRangeType.hit_wall) &&
                !hactModule.GetPlayInfo(ref rangeTemp, HActRangeType.dropped_throw) &&
                !hactModule.GetPlayInfo(ref rangeTemp, HActRangeType.guardrail) &&
                !hactModule.GetPlayInfo(ref rangeTemp, HActRangeType.pole) &&
                !hactModule.GetPlayInfo(ref rangeTemp, HActRangeType.stand);

            if (IsInPlain)
            {
                hactModule.GetPlayInfo(ref LastPlainRange, HActRangeType.plain);
            }

            unsafe
            {
                bool* free_movement_mode = (bool*)(Character.Pointer.ToInt64() + 0x11A3);
                *free_movement_mode = AllowPlayerMovement();
            }

            HeatModule.Update();

            for (uint i = 1; i < 13; i++)
                Fighter.GetStatus().RemoveExEffect(i, true, true);

            Fighter.GetStatus().RemoveExEffect(266, true, true);

            if (BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action)
                CombatActionUpdate();
        }

        private void CombatActionUpdate()
        {
            if (CanAct() && !Mod.IsTurnBased())
            {
                if (IsInputDeliveryHelp())
                {
                    BrawlerBattleManager.IsDeliveryHelping = true;
                    BrawlerBattleManager.OnForceDeliveryHelpON();
                    BrawlerBattleManager.ChangeToTurnBased();
                    BattleTurnManager.OverrideAttackerSelection2(BrawlerBattleManager.DecideTurnAttacker);
                    BrawlerBattleManager.UsedDeliveryOnce = true;
                }
            }

            if (GodMode)
            {
                Player.SetHeatNow(PlayerID, Player.GetHeatMax(PlayerID));
                Mod.MainPlayerFighter.GetStatus().SetHPCurrent(Player.GetHPMax(PlayerID));
            }

            if (IsExtremeHeat)
            {
                if (CurrentJob != RPGJobID.kasuga_freeter && CurrentJob != RPGJobID.kiryu_01)
                {
                    ItemID wep = Party.GetEquipItemID(PlayerID, PartyEquipSlotID.weapon);
                    AssetID wepAssetID = Item.GetAssetID(wep);

                    //something went wrong with weapons, lets fix it
                    if (Fighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().AssetID != wepAssetID)
                        SetupJobWeapons();
                }
            }
            else
            {
                if (IsOtherPlayer())
                {
                    if (CurrentJob != RPGJobID.kasuga_freeter && CurrentJob != RPGJobID.kiryu_01)
                    {
                        ItemID wep = Party.GetEquipItemID(PlayerID, PartyEquipSlotID.weapon);
                        AssetID wepAssetID = Item.GetAssetID(wep);

                        Weapon leftWeapon = Fighter.GetWeapon(AttachmentCombinationID.left_weapon);
                        Weapon rightWeapon = Fighter.GetWeapon(AttachmentCombinationID.right_weapon);

                        //something went wrong with weapons, lets fix it
                        //have to check left weapon too, or else adachi will break
                        if (DoesJobHaveWeapons(CurrentJob))
                            if (leftWeapon.Unit.Get().AssetID == AssetID.invalid && rightWeapon.Unit.Get().AssetID == AssetID.invalid)
                                SetupJobWeapons();
                    }
                }
            }

            EXModule.Update();
            UpdateTargeting(Fighter);

            GameInputUpdate();

            if (BattleManager.PadInfo.IsJustPush(BattleButtonID.down) && !BattleManager.PadInfo.IsHold(BattleButtonID.guard))
            {
                RPGJobID curJob = CurrentJob;

                if (curJob == RPGJobID.kasuga_freeter || curJob == RPGJobID.kiryu_01)
                {
                    if (FighterInfo.RightWeapon.IsValid() && FighterInfo.RightWeapon.IsFromPocket())
                    {
                        string skillName = $"player_wp{Asset.GetArmsCategory(Fighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().AssetID).ToString().ToLowerInvariant()}_noutou";
                        RPGSkillID noutouSkill = DBManager.GetSkill(skillName);

                        if (noutouSkill == RPGSkillID.invalid)
                            noutouSkill = DBManager.GetSkill("player_wpa_noutou");

                        Character.HumanModeManager.ToEndReady();
                        BattleTurnManager.ForceCounterCommand(Fighter, BrawlerBattleManager.AllEnemiesNearest[0], noutouSkill);
                    }
                    else
                    {
                        DragonEngine.Log("Player equips job weapon");
                        ItemID weapon = Party.GetEquipItemID(PlayerID, PartyEquipSlotID.weapon);

                        if (weapon != 0)
                            PullOutWeapon(weapon);
                    }
                }

            }

            if (IsExtremeHeat)
                Fighter.GetStatus().SetSuperArmor(true);

            if (!m_getupHyperArmorDoOnce)
            {
                if (FighterInfo.IsGettingUp)
                {
                    if (!IsExtremeHeat)
                        Fighter.GetStatus().SetSuperArmor(true);

                    //OnPlayerStartGettingUp?.Invoke();

                    m_getupHyperArmorDoOnce = true;
                }
            }
            else
            {
                if (!FighterInfo.IsGettingUp)
                {
                    if (!IsExtremeHeat)
                        Fighter.GetStatus().SetSuperArmor(false);

                    m_getupHyperArmorDoOnce = false;
                }
            }

            MortalReversalManager.Update();

            var fighterInf = FighterInfo;


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

        public bool IsKiryu()
        {
            if (!BrawlerBattleManager.Battling)
                return Mod.MainPlayerCharacter.Attributes.player_id == Player.ID.kiryu;

            return PlayerID == Player.ID.kiryu;
        }

        public bool IsKasuga()
        {
            if (!BrawlerBattleManager.Battling)
                return Mod.MainPlayerCharacter.Attributes.player_id == Player.ID.kasuga;

            return PlayerID == Player.ID.kasuga;
        }

        public bool IsDragon()
        {
            return CurrentJob == RPGJobID.kiryu_01;
        }

        public bool IsOtherPlayer()
        {
            return !IsKiryu() && !IsKasuga();
        }

        public bool IsOtherPlayerLeader()
        {
            return IsOtherPlayer() && DragonEngine.GetHumanPlayer().UID == Mod.MainPlayerCharacter.UID;
        }

        public static bool AllowStyleChange()
        {
            if (SpecialBattle.IsDreamSequence())
                return false;

            if (!TutorialManager.Active)
                return true;

            return !TutorialManager.CurrentGoal.Modifier.HasFlag(TutorialModifier.DontAllowStyleChange);
        }

        public bool CanExtremeHeat()
        {
            if (BrawlerBattleManager.IsHAct || HeatActionManager.IsY8BHact)
                return false;

            if (FighterInfo.IsDown || FighterInfo.IsFaceDown)
                return false;

            if (FighterInfo.IsSync)
                return false;

            if (SpecialBattle.IsDreamSequence())
                return false;

            //Adachi
            if (IsOtherPlayer() && GetNormalMovesetForPlayer(PlayerID) == RPG.GetJobCommandSetID(PlayerID, CurrentJob))
                return false;

            if (TutorialManager.Active)
                if (TutorialManager.CurrentGoal.Modifier.HasFlag(TutorialModifier.DontAllowStyleChange))
                    return false;

            if (IsKasuga())
                return TimelineManager.CheckClockAchievement(1, 78, 47) || CurrentJob == RPGJobID.kasuga_braver;
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

        public EHC GetCurrentPlayerHActSet()
        {
            Weapon weapon = Mod.MainPlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon);

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
                    case PlayerStyle.Dragon:
                        return DODHActs[0];
                    case PlayerStyle.Rush:
                        return DODHActs[1];
                    case PlayerStyle.Beast:
                        return DODHActs[2];
                }
            }

            if (PlayerHActs.ContainsKey(PlayerID))
                return PlayerHActs[PlayerID];

            //Not DOD job, but still default style hact for him
            if (isKiryu)
                return DODHActs[0];


            return null;
        }

        public void OnBattleStart()
        {
            DragonEngine.Log("Brawler Battle Start");

            Character player = Character;

            IsExtremeHeat = false;

            //Recursively enumerate and load all combat animations
            //Using commandset 235 as reference, which is kiryu krh
            BattleResourceManager.DoPrepareRefAll(235);


            if (!IsOtherPlayer())
            {
                ToNormalMoveset();
            }
            else
                SetupWeapon(Fighter._ptr);

            if (IsKiryu() || Player.GetCurrentJob(PlayerID) == RPGJobID.kiryu_01)
                CurrentStyle = PlayerStyle.Default;
            else
                CurrentStyle = PlayerStyle.NotApplicable;

            BrawlerFighterInfo.Infos[CharacterHandle.UID] = new BrawlerFighterInfo() { Fighter = Fighter};
        }

        public void OnBattleEnd()
        {
            DragonEngine.Log("Brawler Battle End");

            m_isAttackingDoOnce = false;
            IsExtremeHeat = false;
            CurrentStyle = PlayerStyle.Default;

            ToNormalMoveset();
        }

        public unsafe static void Update_STATIC_TEMP()
        {
            Fighter player = Mod.MainPlayerFighter;

            if (!player.IsValid() || Mod.IsTurnBased())
                return;
        }

        //OnAttackHit/OnAttackLand
        public void OnHitEnemy(Fighter enemy, BattleDamageInfoSafe dmg)
        {
            if (!IsExtremeHeat)
            {
                int curHeat = Player.GetHeatNow(PlayerID);
                int maxHeat = Player.GetHeatMax(PlayerID);

                //player will recover 8% heat for each hit
                //TODO: make this fair for both early and late game by adding heat gain upgrade?
                if (curHeat < maxHeat)
                    Player.SetHeatNow(PlayerID, curHeat + (int)(maxHeat * 0.08f));

                if (Player.GetHeatNow(PlayerID) > maxHeat)
                    Player.SetHeatNow(PlayerID, maxHeat);
            }
        }


        public unsafe static void PullOutWeapon(ItemID weapon)
        {
            AssetID assetId = Item.GetAssetID(weapon);

            if (assetId <= 0)
                return;

            ECAssetArms rightWep = BrawlerFighterInfo.Player.RightWeapon;
            ECAssetArms leftWep = BrawlerFighterInfo.Player.RightWeapon;

            if (rightWep.IsValid() && !rightWep.IsFromPocket())
                Mod.MainPlayerFighter.DropWeapon(new DropWeaponOption(AttachmentCombinationID.right_weapon, false));

            if (leftWep.IsValid() && !leftWep.IsFromPocket())
                Mod.MainPlayerFighter.DropWeapon(new DropWeaponOption(AttachmentCombinationID.left_weapon, false));


            AssetArmsCategoryID category = Asset.GetArmsCategory(assetId);
            DragonEngine.Log("Equipping inventory weapon, category: " + category + " Asset ID: " + (uint)assetId);

            switch (category)
            {
                default:
                    Mod.MainPlayerFighter.Equip(weapon, AttachmentCombinationID.right_weapon);
                    break;
                case AssetArmsCategoryID.X:
                    Mod.MainPlayerFighter.Equip(weapon, AttachmentCombinationID.right_weapon);
                    Mod.MainPlayerFighter.Equip(weapon, AttachmentCombinationID.left_weapon);
                    break;
                case AssetArmsCategoryID.M:
                    Mod.MainPlayerFighter.Equip(weapon, AttachmentCombinationID.right_weapon);
                    Mod.MainPlayerFighter.Equip(weapon, AttachmentCombinationID.left_weapon);
                    break;
            }

            PocketWeapon = Mod.MainPlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit;

            if (PocketWeapon.IsValid())
                PocketWeapon.Get().Arms.SetFromPocket(true);

            BattleTurnManager.ForceCounterCommand(Mod.MainPlayerFighter, BrawlerBattleManager.AllEnemies[0], DBManager.GetSkill($"player_wp{category.ToString().ToLowerInvariant()}_battou"));
        }

        public static BattleCommandSetID GetCommandSetForJob(Player.ID playerID, RPGJobID id)
        {
            switch (id)
            {
                default:
                    return RPG.GetJobCommandSetID(playerID, id);
                case RPGJobID.chitose_01:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_chitose_01");
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
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_chitose_01");
                case Player.ID.chou:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_chou_job_01");
                case Player.ID.tomizawa:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_tomizawa_01");
                case Player.ID.jyungi:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_jyungi_job_01");
            }
        }

        public void ToNormalMoveset()
        {
            BattleCommandSetID moveset = GetNormalMovesetForPlayer(PlayerID);

            //else they will be stuck on their job motinoset for some reason
            if (IsOtherPlayer() && !Mod.MainPlayerFighter.IsValid())
            {
                moveset = (BattleCommandSetID)DBManager.GetCommandSet("p_kiryu_legend");
            }

            Mod.MainPlayerCharacter.HumanModeManager.CommandsetModel.SetCommandSet(0, moveset);

            if (!Mod.MainPlayerFighter.IsValid())
            {
                Mod.MainPlayerCharacter.HumanModeManager.CommandsetModel.SetCommandSet(1, moveset);
            }

            //Yakuza 8 limitation: we cannot get the equipped weapon for another job, it has to be another one
            if (DoesJobHaveWeapons(Player.GetCurrentJob(PlayerID)))
                if (IsOtherPlayer())
                    if (Mod.MainPlayerFighter.IsValid())
                    {
                        SetupWeapon(Mod.MainPlayerFighter._ptr);
                        Mod.MainPlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().Arms.SetFromPocket(true);
                        Mod.MainPlayerFighter.GetWeapon(AttachmentCombinationID.left_weapon).Unit.Get().Arms.SetFromPocket(true);
                    }

            //otherwise, dont do shit, if this is a party member that is the main playable character right now for some reason
            //its modder's responsibility to edit the player cfc.
        }

        public bool CanAct()
        {
            BrawlerFighterInfo inf = FighterInfo;
            return !inf.IsDown && !inf.IsGettingUp && !inf.IsAttack && !Character.HumanModeManager.IsDamage();
        }

        public static bool AllowPlayerMovement()
        {
            if (SpecialBattle.IsDreamSequenceStart())
                return false;

            return true;
        }

        private void GameInputUpdate()
        {
            if (BattleManager.PadInfo.IsJustPush(BattleButtonID.run))
            {
                if (CanExtremeHeat())
                {
                    if (!IsExtremeHeat)
                    {
                        if (Player.GetHeatNow(PlayerID) > 0)
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
                    // Mod.MainPlayerCharacter.HumanModeManager.ToStyleChange(1);
                    OnStyleSwitch(PlayerStyle.Rush);
                }

                if (BattleManager.PadInfo.IsJustPush(BattleButtonID.up))
                {
                    //Mod.MainPlayerCharacter.HumanModeManager.ToStyleChange(2);
                    OnStyleSwitch(PlayerStyle.Default);
                }

                if (BattleManager.PadInfo.IsJustPush(BattleButtonID.right))
                {
                    // Mod.MainPlayerCharacter.HumanModeManager.ToStyleChange(3);
                    OnStyleSwitch(PlayerStyle.Beast);
                }
            }
        }

        public bool CanStyleSwitch()
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
        
        private void SetupJobWeapons()
        {
            switch (Player.GetCurrentJob(PlayerID))
            {
                default:
                    SetupWeapon(Mod.MainPlayerFighter._ptr);
                    break;
                case RPGJobID.man_western:
                    EquipJobWeapons(RPGJobID.man_western);
                    break;
                case RPGJobID.kasuga_freeter:
                    break;
                case RPGJobID.kiryu_01:
                    break;

            }

            Mod.MainPlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().Arms.SetFromPocket(true);
            Mod.MainPlayerFighter.GetWeapon(AttachmentCombinationID.left_weapon).Unit.Get().Arms.SetFromPocket(true);
        }


        public void OnExtremeHeatModeON()
        {
            IsExtremeHeat = true;
            BrawlerBattleManager.OnPlayerEnterEXHeat();

            Character playerChara = Character;

            if (!IsDragon())
            {
                Mod.MainPlayerFighter.Character.GetRender().BattleTransformationOn();
                //EquipJobWeapons(Player.GetCurrentJob(playerChara.Attributes.player_id));

                SetupJobWeapons();

                //Commandset hackery to allow other players to use someone elses jobs
                //Example: Saeko on adachi
                RPGJobID currentJob = Player.GetCurrentJob(PlayerID);
                string currentJobName = currentJob.ToString();
                Player.ID targetPlayerID = PlayerID;

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

                uint commandSet = (uint)GetCommandSetForJob(targetPlayerID, Player.GetCurrentJob(PlayerID));
                playerChara.HumanModeManager.CommandsetModel.SetCommandSet(0, (BattleCommandSetID)commandSet);
                playerChara.Components.EffectEvent.Get().PlayEventOverride(EffectEventCharaID.YZ_Chara_Cange01);
            }
            else
                OnStyleSwitch(PlayerStyle.Dragon);
        }

        public void OnExtremeHeatModeOFF()
        {
            if (!IsExtremeHeat)
                return;

            IsExtremeHeat = false;
            BrawlerBattleManager.OnPlayerExitEXHeat();

            if (!IsDragon())
            {

                Character.GetRender().Reload(CharacterID.m_dummy, 7, true);
                //Mod.MainPlayerFighter.Character.GetRender().ReadyBattleTransform(false);

                var wep1 = Fighter.GetWeapon(AttachmentCombinationID.right_weapon);
                var wep2 = Fighter.GetWeapon(AttachmentCombinationID.left_weapon);

                Fighter.DropWeapon(new DropWeaponOption(AttachmentCombinationID.left_weapon, false));
                Fighter.DropWeapon(new DropWeaponOption(AttachmentCombinationID.right_weapon, false));

                wep1.Unit.Get().DestroyEntity();
                wep2.Unit.Get().DestroyEntity();
                ToNormalMoveset();

                Character.Components.EffectEvent.Get().PlayEventOverride(EffectEventCharaID.YZ_Chara_Cange01);
            }
            else
            {
                OnStyleSwitch(PlayerStyle.Default);
            }

            Fighter.GetStatus().SetSuperArmor(false);
        }

        public void OnStyleSwitch(PlayerStyle newStyle, bool quick = false)
        {
            if (CurrentStyle == newStyle)
                return;

            Character.HumanModeManager.ToEndReady();

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

                case PlayerStyle.Dragon:
                    styleAnim = DBManager.GetSkill("kiryu_to_legend");
                    styleCommandSet = FighterCommandManager.FindSetID("p_kiryu_legend_1988_brawler");
                    overrideStyle = PlayerStyle.Default;

                    if (IniSettings.AllowResurgenceMusic)
                        BrawlerBattleManager.PlaySpecialMusic(DBManager.GetSoundCuesheet("bbg_k"), 1);

                    //Mod.MainPlayerCharacter.HumanModeManager.ToAttackMode(new FighterCommandID((ushort)styleCommandSet, (ushort)FighterCommandManager.GetCommandID(styleCommandSet, "StyleStart")));
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

            if (IsExtremeHeat && newStyle != PlayerStyle.Dragon)
                OnExtremeHeatModeOFF();

            if (!quick)
            {
                short command = FighterCommandManager.GetSet(styleCommandSet).FindCommandInfo("StyleStart");
                if (command >= 0)
                {
                    Character.HumanModeManager.ToEndReady();
                    Character.HumanModeManager.ToAttackMode(new FighterCommandID((ushort)styleCommandSet, command));
                }
            }

            Fighter.DropWeapon(new DropWeaponOption(AttachmentCombinationID.left_weapon, false));
            Fighter.DropWeapon(new DropWeaponOption(AttachmentCombinationID.right_weapon, false));

            CurrentStyle = newStyle;
        }

        public static Fighter GetLockOnTarget(Fighter kasugaFighter)
        {
            try
            {
                if (BrawlerBattleManager.DisableTargetingOnce)
                    return new Fighter(IntPtr.Zero);

                if (BrawlerBattleManager.DisableTargetingThisFrame)
                {
                    BrawlerBattleManager.DisableTargetingThisFrame = false;
                    return new Fighter(IntPtr.Zero);  //LastBehindEnemy; // new Fighter(IntPtr.Zero);
                }

                if (BrawlerBattleManager.AllEnemiesNearest.Length <= 0)
                    return new Fighter(IntPtr.Zero);


                Fighter[] nearestEnemies = null;


                /*
                if (Mod.MainPlayerCharacter.Pad.LeverWorldAng < 0 
                    && BrawlerBattleManager.NearestEnemyBehindPlayer.IsValid()
                    && Vector3.Distance(Mod.MainPlayerCharacter.Transform.Position, BrawlerBattleManager.NearestEnemyBehindPlayer.Character.Transform.Position) <= 3.5f)
                {
                        return BrawlerBattleManager.NearestEnemyBehindPlayer;
                }
                else
               */
                nearestEnemies = BrawlerBattleManager.AllEnemiesNearest;


                if (LastLockedInEnemy.IsValid() && !LastLockedInEnemy.IsDead() && BrawlerFighterInfo.Player.IsAttack)
                    return LastLockedInEnemy;


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

            Fighter target = GetLockOnTarget(playerFighter);

            bool disableTargetingThisFrame = BrawlerBattleManager.DisableTargetingOnce == true || BrawlerBattleManager.DisableTargetingThisFrame;

            targetDecide.SetTarget(new FighterID() { Handle = target.Character.UID });

            if (!disableTargetingThisFrame)
            {
                LastBehindEnemy = BrawlerBattleManager.NearestEnemyBehindPlayer;
                LastLockedInEnemy = target;
            }
            else
                targetDecide.SetTarget(new FighterID() { Handle = 0 });
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
            bool isGuard = *((bool*)(dmg._ptr.ToInt64() + 0x108));
            bool isJustGuard = *((bool*)(dmg._ptr.ToInt64() + 0x109));

            if (isJustGuard)
            {
                *(int*)(dmg._ptr.ToInt64() + 0x120) = 0;
                *(int*)(dmg._ptr.ToInt64() + 0x124) = 0;

                OnPerfectGuard?.Invoke();
            }

            if (isGuard && !isJustGuard)
            {
                uint attr = *(uint*)(dmg._ptr + 0x8C);

                if ((attr & 32) != 0 || (attr & 0x10000000) != 0)
                {
                    if (Mod.MainPlayerCharacter.HumanModeManager.IsGuarding())
                        HumanModePatches.GuardBreak.Invoke(Mod.MainPlayerCharacter.HumanModeManager.Pointer, dmg._ptr);
                }
            }

            int maxHeat = Player.GetHeatMax(Player.ID.kasuga);

            float reductionAmount = !Mod.MainPlayerCharacter.HumanModeManager.IsGuarding() ? 0.1f : 0f;
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

        public unsafe static void OnPostGetHit(BattleDamageInfoSafe dmg)
        {
            if (dmg._ptr == null)
                return;

            bool isGuard = *((bool*)(dmg._ptr.ToInt64() + 0x108));
            bool isJustGuard = *((bool*)(dmg._ptr.ToInt64() + 0x109));

            if (isGuard && !isJustGuard)
            {
                uint attr = *(uint*)(dmg._ptr + 0x8C);

                if ((attr & 32) != 0 || (attr & 0x10000000) != 0)
                {
                    if (Mod.MainPlayerCharacter.HumanModeManager.IsGuarding())
                    {
                        HumanModePatches.GuardBreak.Invoke(Mod.MainPlayerCharacter.HumanModeManager.Pointer, dmg._ptr);

                        //new DETaskList(new DETaskNextFrame(delegate { HumanModePatches.GuardBreak.Invoke(Mod.MainPlayerCharacter.HumanModeManager.Pointer, dmg._ptr); }));


                    }
                }
            }
        }

        public static void EquipJobWeapons(RPGJobID id)
        {
            ItemID itemId = Party.GetEquipItemID(Mod.MainPlayerCharacter.Attributes.player_id, PartyEquipSlotID.weapon);

            switch (id)
            {
                default:
                    Mod.MainPlayerFighter.Equip(Item.GetAssetID(itemId), AttachmentCombinationID.right_weapon, itemId, RPGSkillID.invalid);
                    break;
                case RPGJobID.kasuga_freeter:
                    break;
                case RPGJobID.man_western:
                    Mod.MainPlayerFighter.Equip(Item.GetAssetID(itemId), AttachmentCombinationID.right_weapon, itemId, DBManager.GetSkill("job_western_skill_01"));
                    // Mod.MainPlayerFighter.Equip((AssetID)1353, AttachmentCombinationID.right_weapon, ItemID.invalid, DBManager.GetSkill("western_test"));
                    break;
            }
        }

        public unsafe static void CalculateTameDamage(IntPtr battleDamageInfo)
        {
            if (!AuthNodeBattleTame.ShouldApplyTame())
                return;

            int damage = (*(int*)(battleDamageInfo.ToInt64() + 0x120));
            damage = damage + (int)(damage * 0.25f); //25% damage boost increase
            *(int*)(battleDamageInfo.ToInt64() + 0x120) = damage;
            *(int*)(battleDamageInfo.ToInt64() + 0x124) = damage;

            //consume the tame
            AuthNodeBattleTame.Reset();
        }
    }
}
