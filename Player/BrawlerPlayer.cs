using DragonEngineLibrary;
using DragonEngineLibrary.Service;
using ElvisCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    public static class BrawlerPlayer
    {
        public static Player.ID CurrentPlayer = 0;
        public static PlayerStyle CurrentStyle = PlayerStyle.Default;

        public static CharacterAttributes OriginalPlayerAttributes;

        public static Dictionary<StageID, EHC> StageEHC = new Dictionary<StageID, EHC>();
        public static EHC PlayerHActs;
        public static EHC[] KiryuHActs;
        private static EHC KasugaHAct;
        private static EHC AdachiHAct;

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
            return inf.IsHold(BattleButtonID.guard) && inf.IsHold(BattleButtonID.light) && inf.IsHold(BattleButtonID.heavy);
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

        public static bool IsOtherPlayer()
        {
            return !IsKiryu() && !IsKasuga();
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
            if (IsOtherPlayer())
                return false;

            if (IsKasuga())
                return TimelineManager.CheckClockAchievement(1, 78, 47);
            else
                return TimelineManager.CheckClockAchievement(1, 86, 53);
        }

        public static void LoadContent()
        {
            PlayerHActs = YazawaCommandManager.LoadYHC("player/shared.ehc");

            KiryuHActs = new EHC[3]
            {
                YazawaCommandManager.LoadYHC("player/kiryu_legend.ehc"),
                YazawaCommandManager.LoadYHC("player/kiryu_rush.ehc"),
                YazawaCommandManager.LoadYHC("player/kiryu_crash.ehc"),
            };

            KasugaHAct = YazawaCommandManager.LoadYHC("player/kasuga_sud.ehc");
            AdachiHAct = YazawaCommandManager.LoadYHC("player/adachi_detective.ehc");

            StageEHC = new Dictionary<StageID, EHC>()
            {
                [StageID.st_kamuro] = YazawaCommandManager.LoadYHC("stage/kamuro.ehc")
            };
        }

        public static EHC GetCurrentPlayerHActSet()
        {
            Weapon weapon = BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon);

            bool isKiryu = IsKiryu();
            bool isKasuga = IsKasuga();

            if (isKiryu || isKasuga)
            {
                if (weapon.Unit.IsValid())
                    return WeaponManager.GetEHCSetForWeapon(Asset.GetArmsCategory(weapon.Unit.Get().AssetID));

                if (IsKiryu())
                {
                    switch (CurrentStyle)
                    {
                        default:
                            return KiryuHActs[0];

                        case PlayerStyle.Default:
                            return KiryuHActs[0];
                        case PlayerStyle.Legend:
                            return KiryuHActs[0];
                        case PlayerStyle.Rush:
                            return KiryuHActs[1];
                        case PlayerStyle.Beast:
                            return KiryuHActs[2];
                    }
                }
                else
                    return KasugaHAct;
            }
            else
            {
                switch(CurrentPlayer)
                {
                    default:
                        return null;
                    case Player.ID.adachi:
                        if (Player.GetCurrentJob(Player.ID.adachi) == RPGJobID.adachi_01)
                            return AdachiHAct;
                        else
                            return null;
                }
            }
        }

        public static void OnBattleStart()
        {
            Character player = BrawlerBattleManager.PlayerCharacter;

            IsExtremeHeat = false;
            ToNormalMoveset();
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
            if(!IsExtremeHeat)
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


        public static void PullOutWeapon(ItemID weapon)
        {
            AssetID assetId = Item.GetAssetID(weapon);

            if (assetId <= 0)
                return;

            AssetArmsCategoryID category = Asset.GetArmsCategory(assetId);
            DragonEngine.Log("Equipping inventory weapon, category: " + category);

            switch(category)
            {
                default:
                    BrawlerBattleManager.PlayerFighter.Equip(weapon, AttachmentCombinationID.right_weapon);
                    break;
                case AssetArmsCategoryID.X:
                    BrawlerBattleManager.PlayerFighter.Equip(weapon, AttachmentCombinationID.right_weapon);
                    BrawlerBattleManager.PlayerFighter.Equip(weapon, AttachmentCombinationID.left_weapon);
                    break;
            }

            BattleTurnManager.ForceCounterCommand(BrawlerBattleManager.PlayerFighter, BrawlerBattleManager.AllEnemies[0], DBManager.GetSkill($"player_wp{category.ToString().ToLowerInvariant()}_battou"));
        }

        public static void ToNormalMoveset()
        {
            if(IsKasuga())
                BrawlerBattleManager.PlayerCharacter.HumanModeManager.CommandsetModel.SetCommandSet(0, GetCommandSetForJob(Player.ID.kasuga, RPGJobID.kasuga_freeter));
            else
                BrawlerBattleManager.PlayerCharacter.HumanModeManager.CommandsetModel.SetCommandSet(0, (BattleCommandSetID)FighterCommandManager.FindSetID("p_kiryu_legend_brawler"));
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
            }

            EXModule.Update();
            UpdateTargeting(BrawlerBattleManager.PlayerFighter);

            GameInputUpdate();

            if (DragonEngine.IsKeyHeld(VirtualKey.LeftShift))
            {
                if(IsKasuga() && Player.GetCurrentJob(Player.ID.kasuga) == RPGJobID.kasuga_freeter)
                {
                    if (DragonEngine.IsKeyHeld(VirtualKey.N1))
                    {
                        DragonEngine.Log("Ichi equips job weapon");
                        ItemID weapon = Party.GetEquipItemID(Player.ID.kasuga, PartyEquipSlotID.weapon);

                        if (weapon != 0)
                            PullOutWeapon(weapon);
                    }
                }
            }

            if(IsExtremeHeat)
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


            if(fighterInf.Fighter != null)
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
            return IsKiryu();
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
                if (HeatActionManager.PerformableHact != null && HeatActionManager.CanHAct())
                    HeatActionManager.ExecHeatAction(HeatActionManager.PerformableHact);
        }


        public static void OnExtremeHeatModeON()
        {
            IsExtremeHeat = true;
            BrawlerBattleManager.OnPlayerEnterEXHeat();

            Character playerChara = BrawlerBattleManager.PlayerCharacter;

            if (!IsKiryu())
            {
                BrawlerBattleManager.PlayerFighter.Character.GetRender().BattleTransformationOn();
                //EquipJobWeapons(Player.GetCurrentJob(playerChara.Attributes.player_id));
                SetupWeapon(BrawlerBattleManager.PlayerFighter._ptr);
                playerChara.HumanModeManager.CommandsetModel.SetCommandSet(0, GetCommandSetForJob(playerChara.Attributes.player_id, Player.GetCurrentJob(playerChara.Attributes.player_id)));
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


            if (!IsKiryu())
            {
                Fighter playerFighter = BrawlerBattleManager.PlayerFighter;
                Character playerChara = playerFighter.Character;

                BrawlerBattleManager.PlayerFighter.Character.GetRender().Reload(CharacterID.m_dummy, 7, true);
                //BrawlerBattleManager.PlayerFighter.Character.GetRender().ReadyBattleTransform(false);
                playerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().DestroyEntity();
                playerFighter.GetWeapon(AttachmentCombinationID.left_weapon).Unit.Get().DestroyEntity();
                ToNormalMoveset();

                playerChara.Components.EffectEvent.Get().PlayEventOverride(EffectEventCharaID.YZ_Chara_Cange01);
            }

            if(IsKiryu())
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

            switch(newStyle)
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

                    BrawlerBattleManager.PlayerCharacter.HumanModeManager.ToAttackMode(new FighterCommandID((ushort)styleCommandSet, (ushort)FighterCommandManager.GetCommandID(styleCommandSet, "StyleStart")));
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
                    BrawlerBattleManager.PlayerCharacter.HumanModeManager.ToAttackMode(new FighterCommandID((ushort)styleCommandSet, command));
            }

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

            if(isJustGuard)
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

            if(attacker != null && attacker.IsMortalAttack())
            {
                EffectEventManager.PlayScreen(28);
                DragonEngine.Log("YEOWCH! MORTAL ATTACK");
            }
        }


        public static BattleCommandSetID GetCommandSetForJob(Player.ID playerID, RPGJobID id)
        {
            switch(id)
            {
                default:
                    return RPG.GetJobCommandSetID(playerID, id);
                case RPGJobID.man_western:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_man_western_brawler");
                case RPGJobID.kasuga_freeter:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_kasuga_brawler");
                case RPGJobID.kasuga_braver:
                    return (BattleCommandSetID)DBManager.GetCommandSet("p_kasuga_hero_brawler");
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
    }
}
