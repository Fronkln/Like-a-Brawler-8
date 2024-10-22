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

        protected CharacterAttributes m_attributes;

        //Constants
        protected const float COMBO_EXTEND_BASE_CHANCE = 45;
        protected const float HACT_COOLDOWN = 25f;

        public virtual void Awake()
        {
            LoadContent();

            m_attributes = Character.Attributes;
            uint soldierID = (uint)m_attributes.soldier_data_id;

            ECBattleStatus status = Fighter.GetStatus();

            DragonEngine.Log(GetType().ToString() +
                "\nSoldier ID: " + DBManager.GetSoldier(soldierID) +
                "\nHealth: " + status.CurrentHP +
                "\nAttack: " + status.AttackPower +
                "\nDefense: " + status.DefensePower +
                "\nCtrlType: " + (uint)Character.Attributes.ctrl_type +
                "\nAssetL: " + Fighter.GetWeapon(AttachmentCombinationID.left_weapon).ToString() +
                "\nAssetR: " + Fighter.GetWeapon(AttachmentCombinationID.right_weapon).ToString() +
                "\nCharacter ID: " + (uint)m_attributes.chara_id);
        }

        public static void ApplyBalanceChange(Fighter character)
        {

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
            return m_attributes.animal_kind == CharacterAnimalKind.human;
        }

        public virtual bool CanHAct()
        {
            return true;
        }

        public virtual void CombatUpdate()
        {
            if (m_hactCd > 0)
                m_hactCd -= DragonEngine.deltaTime;

            if (HActList != null && CanHAct() && m_hactCd <= 0 && !BrawlerBattleManager.IsHActOrWaiting)
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

            if (performableHact != null)
            {
                performableHact.UseHeat = false;
                HeatActionManager.ExecHeatAction(performableHact);
                m_hactCd = HACT_COOLDOWN;
            }
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
