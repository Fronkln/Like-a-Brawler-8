using DragonEngineLibrary;
using DragonEngineLibrary.Service;
using ElvisCommand;

namespace LikeABrawler2
{
    public class BaseAI
    {
        public Fighter Fighter;
        public Character Character;

        public EHC HActList;

        protected float m_hactCd = 0;

        public BrawlerFighterInfo BrawlerInfo { get { return BrawlerFighterInfo.Infos[Character.UID]; } }

        public bool MyTurn { get; private set; }
        public float TimeSinceMyTurn { get; protected set; } = 999;

        public virtual void Awake()
        {
            LoadContent();
            
            uint soldierID = (uint)Character.Attributes.soldier_data_id;

            ECBattleStatus status = Fighter.GetStatus();

            ApplyBalanceChange();
 
            DragonEngine.Log(GetType().ToString() +
                "\nSoldier ID: " + DBManager.GetSoldier(soldierID) +
                "\nHealth: " + status.CurrentHP +
                "\nAttack: " + status.AttackPower +
                "\nDefense: " + status.DefensePower +
                "\nCtrlType: " + (uint)Character.Attributes.ctrl_type +
                "\nAssetL: " + Fighter.GetWeapon(AttachmentCombinationID.left_weapon).ToString() +
                "\nAssetR: " + Fighter.GetWeapon(AttachmentCombinationID.right_weapon).ToString());
        }

        private void ApplyBalanceChange()
        {
            uint soldierID = (uint)Character.Attributes.soldier_data_id;
            EnemyRebalanceEntry rebalancedDat = DBManager.GetSoldierRebalance(soldierID);

            if (rebalancedDat == null)
                return;

            ECBattleStatus status = Fighter.GetStatus();

            Fighter.GetStatus().SetHPMax(rebalancedDat.Health);
            Fighter.GetStatus().SetHPCurrent(rebalancedDat.Health);
            Fighter.GetStatus().AttackPower = rebalancedDat.Attack;
            Fighter.GetStatus().DefensePower = rebalancedDat.Defense;
        }

        public virtual void LoadContent()
        {

        }

        public virtual void Update()
        {
        }

        public virtual bool AllowDamage()
        {
            return true;
        }

        public virtual bool CanBeHActed()
        {
            return true;
        }

        public virtual void CombatUpdate()
        {
            if (m_hactCd > 0)
                m_hactCd -= DragonEngine.deltaTime;

            if (HActList != null && m_hactCd <= 0)
                HActUpdate();

            bool myTurnNow = IsMyTurn();

            if (myTurnNow && !MyTurn)
                OnStartTurn();

            MyTurn = myTurnNow;

            if (!MyTurn)
                TimeSinceMyTurn += DragonEngine.deltaTime;
            else
                TimeSinceMyTurn = 0;
        }

        public virtual void HActUpdate()
        {
            HeatActionInformation performableHact = HeatActionSimulator.Check(Fighter, HActList);

            if(performableHact != null)
                HeatActionManager.ExecHeatAction(performableHact);
        }

        public virtual bool CanPerformHAct()
        {
            return true;
        }

        public bool IsBeingJuggled()
        {
            uint attrib = Fighter.GetReactionType();
            return attrib == 54 || attrib == 6;
        }
        
        public virtual void OnStartTurn()
        {

        }

        public bool IsMyTurn()
        {
            return BattleTurnManager.SelectedFighter.UID == Character.UID;
        }

        public bool CanAttackCancel()
        {
            if (!MyTurn)
                return false;

            if (!Character.HumanModeManager.IsAttack())
                return false;

            int battleStartFrame = MotionService.SearchTimingDetail(0, MotionService.GetBepID(Character.GetMotion().GmtID), 24).Start;

            if (battleStartFrame >= 0 && Character.GetMotion().Frame * 100 >= battleStartFrame)
                return true;

            return false;
        }
    }
}
