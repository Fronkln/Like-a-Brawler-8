using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal class InventoryPatches : BrawlerPatch
    {
        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool PlayerCanEquipItem(uint armsID, PartyEquipSlotID equipSlot, Player.ID playerID, IntPtr boolean, RPGJobID job);

        private IntPtr m_invCanEquipFunc;

        public override void Init()
        {
            base.Init();

            m_invCanEquipFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("40 55 56 41 54 41 55 41 57 48 8B EC");
        }

        protected override void SetActive()
        {
            base.SetActive();

            if(m_canEquipTrampoline == null)
               m_canEquipTrampoline = BrawlerPatches.HookEngine.CreateHook<PlayerCanEquipItem>(m_invCanEquipFunc, Player_CanEquipItem);

            BrawlerPatches.HookEngine.EnableHook(m_canEquipTrampoline);
        }

        private static PlayerCanEquipItem m_canEquipTrampoline = null;
        private static bool Player_CanEquipItem(uint armsID, PartyEquipSlotID equipSlot, Player.ID playerID, IntPtr boolean, RPGJobID job)
        {
            if(Mod.IsTurnBased())
                return m_canEquipTrampoline(armsID, equipSlot, playerID, boolean, job);

            if (Player.GetCurrentJob(Player.ID.kasuga) != RPGJobID.kasuga_freeter)
                return m_canEquipTrampoline(armsID, equipSlot, playerID, boolean, job);


            if (equipSlot != PartyEquipSlotID.weapon || playerID != Player.ID.kasuga)
                return m_canEquipTrampoline(armsID, equipSlot, playerID, boolean, job);

            string name = ((ArmsID)armsID).ToString().ToLowerInvariant();

            //elvis_weapon_job_xxx
            if (name.StartsWith("elvis_weapon", StringComparison.Ordinal))
                return true;

            return false;
        }
    }
}
