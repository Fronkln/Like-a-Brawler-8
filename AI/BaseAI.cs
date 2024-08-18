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

        public BrawlerFighterInfo BrawlerInfo { get { return BrawlerFighterInfo.Get(Character.UID); } }

        public float ComboExtendChance = COMBO_EXTEND_BASE_CHANCE;
        public bool MyTurn { get; private set; }
        public float TimeSinceMyTurn { get; protected set; } = 999;

        private bool m_attacking = false;
        protected bool m_extendAttack = false;
        protected float m_hactCd = 0;

        //Constants
        protected const float COMBO_EXTEND_BASE_CHANCE = 45;

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

            if(!m_attacking)
            {
                if(BrawlerInfo.IsAttack)
                {
                    m_attacking = true;
                    OnStartAttack();
                }    
            }
            else
            {
                if(!BrawlerInfo.IsAttack)
                    m_attacking = false;
            }
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

        private void OnStartAttack()
        {
            m_extendAttack = new System.Random().Next(0, 101) <= ComboExtendChance;
            OnStartAttackEvent();
        }

        protected virtual void OnStartAttackEvent()
        {

        }


        public bool CheckParam(BaseAIParams param)
        {
            switch(param)
            {
                default: 
                    return false;
                case BaseAIParams.ExtendCombo:
                    return m_extendAttack;
            }
        }
    }
}
