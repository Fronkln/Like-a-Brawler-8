using System;
using DragonEngineLibrary;
using ElvisCommand;

namespace LikeABrawler2
{
    internal class SupporterPartyMemberSoldier : SupporterPartyMember
    {
        public ItemID EquipWeapon;

        public override void Awake()
        {
            base.Awake();

            EquipWeapon = Party.GetEquipItemID(PlayerID, PartyEquipSlotID.weapon);
            DragonEngine.Log("Party member: " + PlayerID + " EquipWeapon: " + (uint)EquipWeapon + " AssetID: " + (uint)Item.GetAssetID(EquipWeapon));

            if(SupporterManager.PartyStats.ContainsKey(PlayerID))
            {
                PartyMemberTempStatStore stat = SupporterManager.PartyStats[PlayerID];
                Fighter.GetStatus().AttackPower = stat.AttackPower;
            }
        }

        private void EquipWep()
        {
            if(EquipWeapon > 0)
                Fighter.Equip(EquipWeapon, AttachmentCombinationID.right_weapon);
        }
    }
}
