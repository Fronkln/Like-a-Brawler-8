using DragonEngineLibrary;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    public class BrawlerFighterInfo
    {
        public Fighter Fighter = new Fighter();

        public static BrawlerFighterInfo Player { 
            get 
            {
                if (!Infos.ContainsKey(BrawlerBattleManager.PlayerCharacter.UID))
                    return new BrawlerFighterInfo();

                return Infos[BrawlerBattleManager.PlayerCharacter.UID];
            } 
        }
        public static Dictionary<uint, BrawlerFighterInfo> Infos = new Dictionary<uint, BrawlerFighterInfo>();

        public static BrawlerFighterInfo Get(uint characterUID)
        {
            if (Infos.ContainsKey(characterUID))
                return Infos[characterUID];
            else
                return new BrawlerFighterInfo();
        }

        public bool IsDead;
        public bool IsFlinching;
        public bool IsSync;
        public bool IsDown;
        public bool IsFaceDown;
        public bool IsGettingUp;
        public bool IsRagdoll;
        public bool IsMove;
        public bool IsAttack;

        public float MoveTime;
        public float DownTime;

        public ECAssetArms RightWeapon = new ECAssetArms();
        public ECAssetArms LeftWeapon = new ECAssetArms();

        public HumanMode CurrentMode = new HumanMode();

        //Purpose: Cache fighter variables
        //Reduces PInvoke(probably) and eliminates several crashes
        //Related to accesing those vars in input loop
        public void Update(Fighter fighter)
        {
            Fighter = fighter;

            if (fighter == null || fighter._ptr == IntPtr.Zero || fighter.IsDead() || !fighter.IsValid() || !fighter.Character.IsValid())
            {
                Infos.Remove(fighter.Character.UID);
                return;
            }

            BattleFighterInfo inf = fighter.GetInfo();


            IsDead = fighter.IsDead();
            IsFlinching = fighter.Character.HumanModeManager.IsDamage();
            IsSync = fighter.Character.HumanModeManager.IsSync();
            IsDown = fighter.Character.HumanModeManager.IsDown();
            IsFaceDown = fighter.IsFaceDown();
            IsGettingUp = fighter.Character.HumanModeManager.IsStandup();
            IsRagdoll = inf.is_ragdoll_;//fighter.Character.IsRagdoll();
            IsMove = fighter.Character.HumanModeManager.IsMove();
            RightWeapon = fighter.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().Arms;
            LeftWeapon = fighter.GetWeapon(AttachmentCombinationID.left_weapon).Unit.Get().Arms;
            IsAttack = fighter.Character.HumanModeManager.IsAttack();

            CurrentMode = fighter.Character.HumanModeManager.CurrentMode;

            if (IsDown)
                DownTime += DragonEngine.deltaTime;
            else
                DownTime = 0;

            if (IsMove)
                MoveTime += DragonEngine.deltaTime;
            else
                MoveTime = 0;

            Infos[fighter.Character.UID] = this;
        }

        /// <summary>
        /// We are either down, dead, ragdolled, in sync, swaying, getting up or flinching in pain.
        /// </summary>
        /// <returns></returns>
        public bool CantAttackOverall()
        {
            return IsDead || IsFlinching || IsSync || IsDown || IsGettingUp || IsRagdoll || Fighter.Character.HumanModeManager.CurrentMode.ModeName == "Sway" || MortalReversalManager.Procedure;
        }
    }
}
