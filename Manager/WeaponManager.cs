using DragonEngineLibrary;
using ElvisCommand;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal unsafe static class WeaponManager
    {
        public static AssetUnit NearestAsset = null;
        private static AssetUnit m_targetNearestAsset = null;

        public static Dictionary<Player.ID, Dictionary<JobWeaponType, Dictionary<AssetArmsCategoryID, EHC>>> WeaponEHCs = new Dictionary<Player.ID, Dictionary<JobWeaponType, Dictionary<AssetArmsCategoryID, EHC>>>();
        public static Dictionary<Player.ID, Dictionary<AssetArmsCategoryID, string>> WeaponCommandsets = new Dictionary<Player.ID, Dictionary<AssetArmsCategoryID, string>>();

        private static Dictionary<AssetArmsCategoryID, int> m_wepUseCounts = new Dictionary<AssetArmsCategoryID, int>();

        public static EntityHandle<AssetUnit> PickedUpWeapon = new EntityHandle<AssetUnit>(0);
        public static int PickedUpWeaponUseCount = 0;

        private static bool m_initDoOnce = false;
        private static bool m_hasWeaponDoOnce = false;

        private static float m_reduceCountCooldown = 0;
        private static FighterCommandID m_lastHitCommand;

        public static void Init()
        {
            //TODO: Atleast seperate EHCs based on Kiryu or Kasooga
            /*
            WeaponCommandsets = new Dictionary<AssetArmsCategoryID, BattleCommandSetID>()
            {
                [AssetArmsCategoryID.A] = (BattleCommandSetID)DBManager.GetCommandSet("p_com_wpa"),
                [AssetArmsCategoryID.B] = (BattleCommandSetID)DBManager.GetCommandSet("p_com_wpb"),
                [AssetArmsCategoryID.C] = (BattleCommandSetID)DBManager.GetCommandSet("p_com_wpc"),
                [AssetArmsCategoryID.F] = (BattleCommandSetID)DBManager.GetCommandSet("p_com_wpc"),
                [AssetArmsCategoryID.D] = (BattleCommandSetID)DBManager.GetCommandSet("p_com_wpd"),
                [AssetArmsCategoryID.H] = (BattleCommandSetID)DBManager.GetCommandSet("p_kasuga_wph"),
                [AssetArmsCategoryID.N] = (BattleCommandSetID)DBManager.GetCommandSet("p_com_wpa"), // temp
                [AssetArmsCategoryID.Y] = (BattleCommandSetID)DBManager.GetCommandSet("p_com_wpy"), // temp
            };
            */

            WeaponEHCs = new  Dictionary<Player.ID, Dictionary<JobWeaponType, Dictionary<AssetArmsCategoryID, EHC>>>
            {
                [Player.ID.invalid] = new Dictionary<JobWeaponType, Dictionary<AssetArmsCategoryID, EHC>>
                {
                    [JobWeaponType.Unknown] = new Dictionary<AssetArmsCategoryID, EHC>()
                    {
                        [AssetArmsCategoryID.A] = YazawaCommandManager.LoadYHC("player/player_wpa.ehc"),
                        [AssetArmsCategoryID.B] = YazawaCommandManager.LoadYHC("player/player_wpb.ehc"),
                        [AssetArmsCategoryID.C] = YazawaCommandManager.LoadYHC("player/player_wpc.ehc"),
                        [AssetArmsCategoryID.F] = YazawaCommandManager.LoadYHC("player/player_wpf.ehc"),
                        [AssetArmsCategoryID.D] = YazawaCommandManager.LoadYHC("player/player_wpd.ehc"),
                        [AssetArmsCategoryID.E] = YazawaCommandManager.LoadYHC("player/player_wpe.ehc"),
                        [AssetArmsCategoryID.H] = YazawaCommandManager.LoadYHC("player/kasuga_wph.ehc"),
                        [AssetArmsCategoryID.N] = YazawaCommandManager.LoadYHC("player/player_wpn.ehc"),
                        [AssetArmsCategoryID.M] = YazawaCommandManager.LoadYHC("player/player_wpm.ehc"),
                        [AssetArmsCategoryID.V] = YazawaCommandManager.LoadYHC("player/player_wpv.ehc"),
                        [AssetArmsCategoryID.Y] = YazawaCommandManager.LoadYHC("player/player_wpy.ehc"),
                    }
                },

                [Player.ID.kasuga] = new Dictionary<JobWeaponType, Dictionary<AssetArmsCategoryID, EHC>>
                {
                    [JobWeaponType.Unknown] = new Dictionary<AssetArmsCategoryID, EHC>()
                    {
                        [AssetArmsCategoryID.A] = YazawaCommandManager.LoadYHC("player/player_wpa.ehc"),
                        [AssetArmsCategoryID.B] = YazawaCommandManager.LoadYHC("player/player_wpb.ehc"),
                        [AssetArmsCategoryID.C] = YazawaCommandManager.LoadYHC("player/player_wpc.ehc"),
                        [AssetArmsCategoryID.F] = YazawaCommandManager.LoadYHC("player/player_wpf.ehc"),
                        [AssetArmsCategoryID.D] = YazawaCommandManager.LoadYHC("player/player_wpd.ehc"),
                        [AssetArmsCategoryID.E] = YazawaCommandManager.LoadYHC("player/player_wpe.ehc"),
                        [AssetArmsCategoryID.H] = YazawaCommandManager.LoadYHC("player/kasuga_wph.ehc"),
                        [AssetArmsCategoryID.N] = YazawaCommandManager.LoadYHC("player/player_wpn.ehc"),
                        [AssetArmsCategoryID.M] = YazawaCommandManager.LoadYHC("player/player_wpm.ehc"),
                        [AssetArmsCategoryID.V] = YazawaCommandManager.LoadYHC("player/player_wpv.ehc"),
                        [AssetArmsCategoryID.Y] = YazawaCommandManager.LoadYHC("player/kasuga_wpy.ehc"),
                    }
                },
                [Player.ID.kiryu] = new Dictionary<JobWeaponType, Dictionary<AssetArmsCategoryID, EHC>>
                {
                    [JobWeaponType.Unknown] = new Dictionary<AssetArmsCategoryID, EHC>()
                    {
                        [AssetArmsCategoryID.A] = YazawaCommandManager.LoadYHC("player/kiryu_wpa.ehc"),
                        [AssetArmsCategoryID.B] = YazawaCommandManager.LoadYHC("player/player_wpb.ehc"),
                        [AssetArmsCategoryID.C] = YazawaCommandManager.LoadYHC("player/kiryu_wpc.ehc"),
                        [AssetArmsCategoryID.F] = YazawaCommandManager.LoadYHC("player/player_wpf.ehc"),
                        [AssetArmsCategoryID.D] = YazawaCommandManager.LoadYHC("player/kiryu_wpd.ehc"),
                        [AssetArmsCategoryID.E] = YazawaCommandManager.LoadYHC("player/player_wpe.ehc"),
                        [AssetArmsCategoryID.H] = YazawaCommandManager.LoadYHC("player/kiryu_wph.ehc"),
                        [AssetArmsCategoryID.N] = YazawaCommandManager.LoadYHC("player/player_wpn.ehc"),
                        [AssetArmsCategoryID.M] = YazawaCommandManager.LoadYHC("player/player_wpm.ehc"),
                        [AssetArmsCategoryID.V] = YazawaCommandManager.LoadYHC("player/player_wpv.ehc"),
                        [AssetArmsCategoryID.Y] = YazawaCommandManager.LoadYHC("player/player_wpy.ehc"),
                    }
                }
            };

            foreach (string str in File.ReadAllLines(Path.Combine(Mod.ModPath, "mdb.brawler/weapon_use_count.txt")))
            {
                string[] split = str.Split(' ');

                m_wepUseCounts[(AssetArmsCategoryID)Enum.Parse(typeof(AssetArmsCategoryID), split[0])] = int.Parse(split[1]);
            }

            if (!m_initDoOnce)
                BrawlerBattleManager.OnBattleEndEvent += OnBattleEnd;

            m_initDoOnce = true;
        }
        
        private static void OnBattleEnd()
        {
            NearestAsset = null;
            m_hasWeaponDoOnce = false;
            m_lastHitCommand = new FighterCommandID();
        }


        public static JobWeaponType GetTypeForJobWeapon(ItemID id)
        {
            string name = id.ToString();

            if (!name.StartsWith("elvis_weapon", StringComparison.OrdinalIgnoreCase))
                return JobWeaponType.Unknown;

            string[] split = name.Split('_');
            string type = split[2];

            switch (type)
            {
                case "kasuga":
                    if (split.Length > 4)
                        return JobWeaponType.Bat;
                    break;
            }

            return JobWeaponType.Unknown;
        }

        public static EHC GetEHCSetForWeapon(AssetArmsCategoryID category, JobWeaponType specialType = JobWeaponType.Unknown)
        {
            Player.ID playerID = BrawlerPlayer.CurrentPlayer;

            if (!WeaponEHCs.ContainsKey(playerID))
                playerID = Player.ID.invalid;

            if (!WeaponEHCs[playerID].ContainsKey(specialType))
                specialType = JobWeaponType.Unknown;

            if (!WeaponEHCs[playerID].ContainsKey(specialType))
                return null;

            if (!WeaponEHCs[playerID][specialType].ContainsKey(category))
                return null;

            return WeaponEHCs[playerID][specialType][category];
        }


        public static void Update()
        {
            
        }
        public static void RealtimeCombatUpdate()
        {
            /*
            //Weapon broke
            if (PickedUpWeapon.UID != 0 && (!PickedUpWeapon.IsValid() || !BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.IsValid()))
            {
                PickedUpWeapon.UID = 0;

                //Means player switched to ex heat from holding wep
                if(!BrawlerPlayer.IsExtremeHeat)
                    BrawlerPlayer.ToNormalMoveset();
            }
            */

            if (m_reduceCountCooldown >= 0)
                m_reduceCountCooldown -= DragonEngine.deltaTime;

            if(!BrawlerFighterInfo.Player.IsAttack)
            {
                m_lastHitCommand.set_ = 0;
                m_lastHitCommand.cmd = 0;
            }

            NearestAsset = AssetManager.FindNearestAssetFromAll(DragonEngine.GetHumanPlayer().GetPosCenter(), 2);

            if (!NearestAsset.IsValid())
            {
                NearestAsset = null;
                return;
            }

            if (Vector3.Distance(NearestAsset.GetPosCenter(), BrawlerBattleManager.PlayerCharacter.GetPosCenter()) > 2f)
            {
                NearestAsset = null;
                return;
            }

            if(!BrawlerBattleManager.PlayerCharacter.IsFacingEntity(NearestAsset, 0))
            {
                NearestAsset = null;
                return;
            }

            Fighter player = BrawlerBattleManager.PlayerFighter;


            if (NearestAsset != null)
            {
                if (!player.GetWeapon(AttachmentCombinationID.right_weapon).Unit.IsValid() && !player.GetWeapon(AttachmentCombinationID.left_weapon).Unit.IsValid())
                {
                    if (!player.GetBrawlerInfo().CantAttackOverall())
                    {
                        if (BattleManager.PadInfo.IsJustPush(BattleButtonID.action))
                        {
                            AssetArmsCategoryID cat = Asset.GetArmsCategory(NearestAsset.AssetID);
                            RPGSkillID pickupSkill = DBManager.GetSkill($"player_wp{cat.ToString().ToLowerInvariant()}_pickup_floor");
                            m_targetNearestAsset = NearestAsset;

                            if (pickupSkill != RPGSkillID.invalid)
                                BattleTurnManager.ForceCounterCommand(player, BrawlerBattleManager.AllEnemies[0], pickupSkill);
                            else
                                throw new Exception("No pickup animation for Asset Category" + cat.ToString());
                           // else
                               // PickupNearestWeapon();
                        }
                        else if(BattleManager.PadInfo.IsJustPush(BattleButtonID.light))
                        {
                            //07.01.2025
                            //Infinite Wealth breaks "Move to play in moveset" forcing me to hardcode this behavior
                            ushort setID = BrawlerBattleManager.PlayerCharacter.HumanModeManager.CurrentMode.GetCommandID().set_;


                            if(DBManager.CommandsetWepAttacks.ContainsKey(setID))
                            {
                                AssetArmsCategoryID cat = Asset.GetArmsCategory(NearestAsset.AssetID);

                                if (DBManager.CommandsetWepAttacks[setID].ContainsKey(cat))
                                {
                                    ushort setID2 = (ushort)FighterCommandManager.FindSetID(DBManager.CommandsetWepAttacks[setID][cat]);
                                    BrawlerBattleManager.PlayerCharacter.HumanModeManager.ToAttackMode(new FighterCommandID(setID2, FighterCommandManager.GetCommandID(setID2, "PickupAttack")));
                                }
                            }
                        }
                    }
                }
            }

            BrawlerFighterInfo inf = BrawlerFighterInfo.Player;


            if (inf.Fighter != null && inf.Fighter.IsValid())
            {
                if (!m_hasWeaponDoOnce)
                {
                    if (BrawlerFighterInfo.Player.RightWeapon.IsValid())
                    {
                        if (!PickedUpWeapon.IsValid() || PickedUpWeapon == null)
                        {
                            PickedUpWeapon = BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit;
                            OnWeaponPickedUp(Asset.GetArmsCategory(PickedUpWeapon.Get().AssetID), false);
                        }

                        m_hasWeaponDoOnce = true;
                    }

                }
                else
                {

                    if (!BrawlerFighterInfo.Player.RightWeapon.IsValid())
                        m_hasWeaponDoOnce = false;
                }
            }
        }


        public static void PickupWeapon(AssetUnit unit)
        {
            if (unit == null)
                return;

            AssetArmsCategoryID cat = Asset.GetArmsCategory(unit.AssetID);

            BrawlerBattleManager.PlayerFighter.Equip(unit.AssetID, AttachmentCombinationID.right_weapon, 0, RPGSkillID.invalid);
            DragonEngine.Log("Picking up " + unit.AssetID + ", Category: " + cat);

            unit.DestroyEntity();

            PickedUpWeapon = BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.UID;
            OnWeaponPickedUp(cat, true);
        }

        private static void OnWeaponPickedUp(AssetArmsCategoryID cat, bool fromGround)
        {
            if (m_wepUseCounts.ContainsKey(cat))
                PickedUpWeaponUseCount = m_wepUseCounts[cat];
            else
                PickedUpWeaponUseCount = 3;

            if (fromGround)
            {
                string wepCommandset = "";
                Player.ID playerID = BrawlerPlayer.CurrentPlayer;


                /*
                if (!WeaponCommandsets.ContainsKey(playerID))
                    playerID = Player.ID.invalid;

                if (WeaponCommandsets[playerID].ContainsKey(cat))
                    wepCommandset = WeaponCommandsets[playerID][cat];

                if (wepCommandset != "")
                    BrawlerBattleManager.PlayerCharacter.HumanModeManager.CommandsetModel.SetCommandSet(0, (BattleCommandSetID)FighterCommandManager.FindSetID(wepCommandset));
                */
            }
        }

        public static void PickupNearestWeapon()
        {
            if (BrawlerFighterInfo.Player.RightWeapon.IsValid())
                return;

            PickupWeapon(m_targetNearestAsset);
            m_targetNearestAsset = null;
        }

        public static void PickupNearestWeapon2()
        {
            if (BrawlerFighterInfo.Player.RightWeapon.IsValid())
                return;

            PickupWeapon(AssetManager.FindNearestAssetFromAll(DragonEngine.GetHumanPlayer().GetPosCenter(), 2));
        }

        //AuthNodeLABPlayerAssetUseReduce
        public static void OnHitWeapon()
        {
            if (!PickedUpWeapon.IsValid() || !BrawlerFighterInfo.Player.IsAttack)
                return;

            FighterCommandID currentCommand = BrawlerBattleManager.PlayerCharacter.HumanModeManager.CurrentMode.GetCommandID();

            if ((currentCommand.cmd == m_lastHitCommand.cmd && currentCommand.set_ == m_lastHitCommand.set_) || m_reduceCountCooldown > 0)
                return;

            PickedUpWeaponUseCount--;

            if (PickedUpWeaponUseCount <= 0)
            {
                PickedUpWeapon.Get().DestroyEntity();

                new DETaskTime(0.1f, delegate { BrawlerBattleManager.PlayerCharacter.HumanModeManager.ToEndReady(); });
            }

            m_lastHitCommand = currentCommand;

            m_reduceCountCooldown = 0.1f;
        }
    }
}
