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

        public static Dictionary<AssetArmsCategoryID, BattleCommandSetID> WeaponCommandsets = new Dictionary<AssetArmsCategoryID, BattleCommandSetID>();
        public static Dictionary<Player.ID, Dictionary<JobWeaponType, Dictionary<AssetArmsCategoryID, EHC>>> WeaponEHCs = new Dictionary<Player.ID, Dictionary<JobWeaponType, Dictionary<AssetArmsCategoryID, EHC>>>();


        private static Dictionary<AssetArmsCategoryID, int> m_wepUseCounts = new Dictionary<AssetArmsCategoryID, int>();

        public static EntityHandle<AssetUnit> PickedUpWeapon = new EntityHandle<AssetUnit>(0);
        public static int PickedUpWeaponUseCount = 0;

        private static bool m_initDoOnce = false;

        public static void Init()
        {
            //TODO: Atleast seperate EHCs based on Kiryu or Kasooga
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

            WeaponEHCs = new  Dictionary<Player.ID, Dictionary<JobWeaponType, Dictionary<AssetArmsCategoryID, EHC>>>
            {
                [Player.ID.kasuga] = new Dictionary<JobWeaponType, Dictionary<AssetArmsCategoryID, EHC>>
                {
                    [JobWeaponType.Unknown] = new Dictionary<AssetArmsCategoryID, EHC>()
                    {
                        [AssetArmsCategoryID.A] = YazawaCommandManager.LoadYHC("player/player_wpa.ehc"),
                        [AssetArmsCategoryID.B] = YazawaCommandManager.LoadYHC("player/player_wpb.ehc"),
                        [AssetArmsCategoryID.C] = YazawaCommandManager.LoadYHC("player/player_wpc.ehc"),
                        [AssetArmsCategoryID.F] = YazawaCommandManager.LoadYHC("player/player_wpf.ehc"),
                        [AssetArmsCategoryID.D] = YazawaCommandManager.LoadYHC("player/kasuga_wpd.ehc"),
                        [AssetArmsCategoryID.H] = YazawaCommandManager.LoadYHC("player/kasuga_wph.ehc"),
                        [AssetArmsCategoryID.N] = YazawaCommandManager.LoadYHC("player/player_wpn.ehc"),
                    }
                },
                [Player.ID.kiryu] = new Dictionary<JobWeaponType, Dictionary<AssetArmsCategoryID, EHC>>
                {
                    [JobWeaponType.Unknown] = new Dictionary<AssetArmsCategoryID, EHC>()
                    {
                        [AssetArmsCategoryID.A] = YazawaCommandManager.LoadYHC("player/player_wpa.ehc"),
                        [AssetArmsCategoryID.B] = YazawaCommandManager.LoadYHC("player/player_wpb.ehc"),
                        [AssetArmsCategoryID.C] = YazawaCommandManager.LoadYHC("player/player_wpc.ehc"),
                        [AssetArmsCategoryID.F] = YazawaCommandManager.LoadYHC("player/player_wpf.ehc"),
                        [AssetArmsCategoryID.D] = YazawaCommandManager.LoadYHC("player/kiryu_wpd.ehc"),
                        [AssetArmsCategoryID.N] = YazawaCommandManager.LoadYHC("player/player_wpn.ehc"),
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

        public static BattleCommandSetID GetCommandSetForWeapon(ItemID item)
        {
            AssetID asset = Item.GetAssetID(item);

            if (asset == AssetID.invalid)
                return GetCommandSetForWeapon(AssetArmsCategoryID.B);

            return GetCommandSetForWeapon(Asset.GetArmsCategory(asset));
        }

        public static BattleCommandSetID GetCommandSetForWeapon(AssetArmsCategoryID category)
        {
            if (!WeaponCommandsets.ContainsKey(category))
                return WeaponCommandsets[AssetArmsCategoryID.B];
            else
                return WeaponCommandsets[category];
        }

        public static EHC GetEHCSetForWeapon(AssetArmsCategoryID category, JobWeaponType specialType = JobWeaponType.Unknown)
        {
            Player.ID playerID = BrawlerPlayer.CurrentPlayer;

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
            RPGJobID job = Player.GetCurrentJob(BrawlerPlayer.CurrentPlayer);

            if(job == RPGJobID.kasuga_freeter || job  == RPGJobID.kiryu_01)
            {
                if(BrawlerBattleManager.IsHAct)
                {

                    //For snatch hacts
                    AssetUnit unit = BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get();
                    AssetArmsCategoryID cat = Asset.GetArmsCategory(unit.AssetID);

                    if (WeaponCommandsets.ContainsKey(cat))
                        BrawlerBattleManager.PlayerCharacter.HumanModeManager.CommandsetModel.SetCommandSet(0, WeaponCommandsets[cat]);
                }
            }
        }
        public static void RealtimeCombatUpdate()
        {
            //Weapon broke
            if (PickedUpWeapon.UID != 0 && (!PickedUpWeapon.IsValid() || !BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.IsValid()))
            {
                PickedUpWeapon.UID = 0;

                //Means player switched to ex heat from holding wep
                if(!BrawlerPlayer.IsExtremeHeat)
                    BrawlerPlayer.ToNormalMoveset();
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

            Fighter player = BrawlerBattleManager.PlayerFighter;


            if (NearestAsset != null)
            {
                if (!player.GetWeapon(AttachmentCombinationID.right_weapon).Unit.IsValid())
                {
                    if (!player.GetBrawlerInfo().CantAttackOverall())
                        if (BattleManager.PadInfo.IsJustPush(BattleButtonID.action))
                        {
                            AssetArmsCategoryID cat = Asset.GetArmsCategory(NearestAsset.AssetID);
                            RPGSkillID pickupSkill = DBManager.GetSkill($"player_wp{cat.ToString().ToLowerInvariant()}_pickup_floor");

                            m_targetNearestAsset = NearestAsset;

                            if (pickupSkill != RPGSkillID.invalid)
                                BattleTurnManager.ForceCounterCommand(player, BrawlerBattleManager.AllEnemies[0], pickupSkill);
                            else
                                PickupNearestWeapon();
                        }
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

            if (WeaponCommandsets.ContainsKey(cat))
                BrawlerBattleManager.PlayerCharacter.HumanModeManager.CommandsetModel.SetCommandSet(0, WeaponCommandsets[cat]);

            unit.DestroyEntity();

            PickedUpWeapon = BrawlerBattleManager.PlayerFighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.UID;

            if (m_wepUseCounts.ContainsKey(cat))
                PickedUpWeaponUseCount = m_wepUseCounts[cat];
            else
                PickedUpWeaponUseCount = 3;
        }

        public static void PickupNearestWeapon()
        {
            PickupWeapon(m_targetNearestAsset);
            m_targetNearestAsset = null;
        }

        //AuthNodeLABPlayerAssetUseReduce
        public static void OnHitWeapon()
        {
            if (!PickedUpWeapon.IsValid())
                return;

            PickedUpWeaponUseCount--;

            if (PickedUpWeaponUseCount <= 0)
                PickedUpWeapon.Get().DestroyEntity();
        }
    }
}
