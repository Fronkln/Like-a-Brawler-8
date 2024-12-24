using DragonEngineLibrary;
using DragonEngineLibrary.Service;
using ElvisCommand;
using System.Security.Permissions;

namespace LikeABrawler2
{
    public class BaseAI
    {
        public Fighter Fighter;
        public Character Character;

        public EHC HActList;

        public BrawlerFighterInfo BrawlerInfo { get { return BrawlerFighterInfo.Get(Character.UID); } }

        public float ComboExtendChance = COMBO_EXTEND_BASE_CHANCE;
        public bool MyTurn { get; private set; } = false;
        public float TimeSinceLastAttack { get; private set; } = 999;
        public float TimeSinceMyTurn { get; protected set; } = 999;

        private bool m_attacking = false;
        
        //AI FLAGS
        protected bool m_extendAttack = false;
        protected bool m_swayAttack = false;

        protected float m_hactCd = 0;

        protected CharacterAttributes m_attributes;

        //Constants
        protected const float COMBO_EXTEND_BASE_CHANCE = 45;
        protected const float HACT_COOLDOWN = 25f;

        private bool m_performingNonTurnAttackDoOnce = false;
        protected float m_nonTurnAttackPatience = 5f;

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
                TimeSinceLastAttack += DragonEngine.deltaTime;

                if(BrawlerInfo.IsAttack)
                {
                    m_attacking = true;
                    TimeSinceLastAttack = 0;
                    OnStartAttack();
                }    
            }
            else
            {
                if(!BrawlerInfo.IsAttack)
                    m_attacking = false;
            }


            if(!m_performingNonTurnAttackDoOnce)
            {
                if(IsPerformingNonTurnAttack())
                {
                    m_performingNonTurnAttackDoOnce = true;
                    OnPerformNonTurnAttack();
                }
            }
            else
            {
                if (!IsPerformingNonTurnAttack())
                    m_performingNonTurnAttackDoOnce = false;
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

        public virtual bool CanDoNonTurnAttack()
        {
            return !IsMyTurn() && TimeSinceLastAttack >= m_nonTurnAttackPatience;
        }

        public bool IsPerformingNonTurnAttack()
        {
            return Character.HumanModeManager.GetCommandName().StartsWith("NonTurnAttack", System.StringComparison.OrdinalIgnoreCase);
        }

        public virtual void OnPerformNonTurnAttack()
        {
            m_nonTurnAttackPatience = new System.Random().Next(4.5f, 7f);
        }

        public virtual bool ShouldGuard()
        {
            return false;
        }


        public bool CheckParam(BaseAIParams param)
        {
            switch(param)
            {
                default: 
                    return false;
                case BaseAIParams.ExtendCombo:
                    return m_extendAttack;
                case BaseAIParams.SwayAttack:
                    return m_swayAttack;
                case BaseAIParams.CanDoNonTurnNearbyAttack:
                    return CanDoNonTurnAttack();
            }
        }
    }
}
