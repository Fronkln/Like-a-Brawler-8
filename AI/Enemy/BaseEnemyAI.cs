using DragonEngineLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LikeABrawler2
{
    internal class BaseEnemyAI : BaseAI
    {
        public List<RPGSkillID> CounterAttacks = new List<RPGSkillID>();
        public List<RPGSkillID> SwayAttacks = new List<RPGSkillID>();

        public EnemyEvasionModule EvasionModule;

        public event Action OnGetUp;

        //Constants
        protected const float RECENT_HIT_TIME = 2.5f;

        //Gameplay variables
        public int RecentDefensiveAttacks = 0;
        //We can determine if RNG has failed us and force things as needed
        //Primarily meant for bosses
        public int RecentHitsWithoutDefensiveMove = 0;

        /// <summary>
        /// Recent total hits we ate without attacking, this includes evasion/guarding
        /// </summary>
        public int RecentHitsWithoutAttack = 0;

        //Player is spamming same attacks 0 iq
        public int RecentHits = 0;

        public int TurnsInARow = 0;
        public float LastTurnTime = 0;
        public float LastHitTime = 10000;
        public float LastGuardTime = 10000;

        public float HeatActionDamageResist = 0; //0.0-1.0f

        protected bool m_hasAntiSpamArmor = false;
        protected float m_antiSpamArmorCooldown = 0;
        protected float m_antiSpamArmorDuration = 0;

        private bool m_gettingUp;
        private bool m_getupHyperArmorDoOnce = false;

        protected bool m_isMortalAttackDoOnce = false;
        protected int m_numMortalAttacks = 0;
        protected RPGSkillID m_mortalSkill = 0;

        protected bool m_transitCounterAttack = false;
        private float m_counterAttackTime = 0;
        private bool m_counterAttacking = false;

        public List<Fighter> PlayersNearest = new List<Fighter>();
        public float DistToPlayer { get { return Vector3.Distance(Character.Transform.Position, BrawlerBattleManager.PlayerCharacter.Transform.Position); } }

        public override void Awake()
        {
            base.Awake();

            EvasionModule = new EnemyEvasionModule();
            EvasionModule.AI = this;

            BrawlerPlayer.OnStartAttack += OnPlayerStartAttack;
        }

        public virtual bool IsBoss()
        {
            return false;
        }

        public override bool AllowDamage()
        {
            if (TutorialManager.Active && TutorialManager.CurrentGoal.Modifier.HasFlag(TutorialModifier.DontAllowEnemyDamage))
                return false;

            return base.AllowDamage();
        }

        public unsafe virtual bool DamageExecValid(IntPtr battleDamageInfo)
        {
            if(!AllowDamage())
            {
                *(int*)(battleDamageInfo.ToInt64() + 0x120) = 0;
                *(int*)(battleDamageInfo.ToInt64() + 0x124) = 0;
            }

            if (HeatActionManager.IsHAct())
                return true;

            if (TransitSway(battleDamageInfo))
            {
                EvasionModule.DoEvasion();
                return false;
            }

            if (CanCounterAttack())
                if (TransitCounterAttack())
                    OnCounterAttacked();

            return true;
        }

        public unsafe virtual void PreTakeDamage(IntPtr battleDamageInfo)
        {
            //3.11.2024: THIS FUCKING SUCKS! THIS FUCKING SUCKS! THIS FUCKING

            if (!BrawlerPlayer.IsExtremeHeat)
                return;

            if(BrawlerPlayer.CurrentJob == RPGJobID.man_western)
            {
                int attr = Marshal.ReadByte(battleDamageInfo + 0x64);

                if (attr == 4)
                {
                    //placeholder damage calculation: TODO, use player base damage, AND weapon stats.
                    int damage = (int)(BrawlerBattleManager.PlayerFighter.GetStatus().AttackPower * 0.45f);
                    *(int*)(battleDamageInfo.ToInt64() + 0x120) = damage;
                    *(int*)(battleDamageInfo.ToInt64() + 0x124) = damage;
                    DragonEngine.Log("PEW PEW PEW!");
                }


            }

        }

        public void OnTakeDamage(BattleDamageInfoSafe dmg)
        {
            LastHitTime = 0;
            RecentHits++;
            RecentHitsWithoutAttack++;
            RecentHitsWithoutDefensiveMove++;
            OnTakeDamageEvent(dmg);
        }

        protected virtual void OnTakeDamageEvent(BattleDamageInfoSafe dmg)
        {
            Character attacker = dmg.Attacker;
            bool isBackAttack = false;

            if(attacker.IsValid())
                isBackAttack = Vector3.Distance(attacker.Transform.Position, Character.Transform.Position) <= 3f && !Character.IsFacingEntity(attacker, 0.1f);

            if (!IsBoss() && !isBackAttack)
            {
                int hitLimit = 0;

                if (IsBeingJuggled())
                    hitLimit = 12;
                else
                    hitLimit = 6;

                //Hyperarmor for goonies
                if (hitLimit> 6)
                    if (!m_hasAntiSpamArmor && m_antiSpamArmorCooldown <= 0)
                    {
                        m_antiSpamArmorCooldown = 20f;
                        ToggleAntiSpamArmor(true);

                        DragonEngine.Log("HYPERARMOR TIME!");
                    }
            }
        }

        public bool IsMortalAttack()
        {
            return Character.HumanModeManager.CurrentMode.GetCommandID().GetInfo().Id.StartsWith("MortalAttack", StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IsMortalAttackOrPreparing()
        {
            return IsMortalAttack() || m_mortalSkill != 0 || m_isMortalAttackDoOnce == true;
        }
        public virtual bool CanCounterAttack()
        {
            return !BrawlerInfo.IsSync && !BrawlerInfo.IsDown && !IsBeingJuggled() && !IsMortalAttackOrPreparing();
        }

        public virtual bool TransitSway(IntPtr battleDamageInfo)
        {
            if (EvasionModule.ShouldEvade(new BattleDamageInfoSafe(battleDamageInfo)))
                return true;

            return false;
        }


        public virtual bool TransitCounterAttack()
        {
            if (RecentHitsWithoutAttack > 4 && CounterAttacks.Count > 0)
                return true;

            return false;
        }


        public virtual void OnCounterAttacked()
        {
            RecentHitsWithoutAttack = 0;
            m_transitCounterAttack = true;
            m_counterAttackTime = 0;
        }

        public void ToggleAntiSpamArmor(bool toggle)
        {
            m_hasAntiSpamArmor = toggle;

            Fighter.GetStatus().SetSuperArmor(toggle, false);

            if (!toggle)
                m_antiSpamArmorDuration = 0;
        }

        public virtual long ProcessHActDamage(TalkParamID hact, long damage)
        {
            return damage;
        }

        public virtual bool CanDieOnHAct()
        {
            return true;
        }

        public virtual bool TransitMortalAttack()
        {
            return false;
        }

        public bool IsMyTurn()
        {
            return BattleTurnManager.SelectedFighter.Get().UID == Character.UID || BrawlerBattleManager.AllEnemies.Length == 1;
        }

        public virtual EntityHandle<Character> OverrideMarkTarget(EntityHandle<Character> original)
        {
            if (!Debug.AttackFirstMember)
            {
                if (original.Get().IsPartyMember())
                    return BrawlerBattleManager.PlayerCharacter;
            }
            else
                return FighterManager.GetFighter(1).Character.UID;

            return BrawlerBattleManager.PlayerCharacter;
        }

        public virtual bool AllowCanGetTurn()
        {
            return true;
        }
        public unsafe override void CombatUpdate()
        {
            base.CombatUpdate();

            //helps with forcing AI to target kasuga
            //TODO: does this mess up targeting for support skills?
            uint* tagFighter = (uint*)(Fighter.GetStatus().Pointer.ToInt64() + 0x2014);
            *tagFighter = BrawlerBattleManager.PlayerCharacter.UID;

            PlayersNearest.Clear();


            if (BrawlerBattleManager.PlayerFighter.IsValid())
                PlayersNearest.Add(BrawlerBattleManager.PlayerFighter);

            Fighter p1 = FighterManager.GetFighter(1);
            Fighter p2 = FighterManager.GetFighter(2);
            Fighter p3 = FighterManager.GetFighter(3);

            if (p1.IsValid())
                PlayersNearest.Add(p1);

            if (p2.IsValid())
                PlayersNearest.Add(p2);

            if (p3.IsValid())
                PlayersNearest.Add(p3);

            PlayersNearest = PlayersNearest.OrderBy(x => Vector3.Distance(Character.Transform.Position, x.Character.Transform.Position)).ToList();

            bool gettingUp = BrawlerInfo.IsGettingUp;

            if (gettingUp && !m_gettingUp)
                OnGetUp?.Invoke();

            m_gettingUp = gettingUp;

            float dt = DragonEngine.deltaTime;

            LastHitTime += dt;

            if(!IsMyTurn())
                LastTurnTime += dt;

            m_antiSpamArmorCooldown -= dt;

            if (m_hasAntiSpamArmor && !Character.HumanModeManager.IsDamage())
                m_antiSpamArmorDuration += dt;

            if (LastHitTime >= RECENT_HIT_TIME)
            {
                RecentHits = 0;
                RecentHitsWithoutAttack = 0;
                RecentHitsWithoutDefensiveMove = 0;
            }
            
            //TODO: Format this code better
            if(!IsBoss())
            {
                if (m_antiSpamArmorDuration > 3f)
                {
                    ToggleAntiSpamArmor(false);
                    DragonEngine.Log("BaseEnemyAI: Relinquish spam armor");
                }
            }
            else
            {
                if (m_antiSpamArmorDuration > 4.5f && LastHitTime > 1.5f)
                {
                    ToggleAntiSpamArmor(false);
                    DragonEngine.Log("BaseEnemyAIBoss: Relinquish spam armor");
                }
            }

            if(m_hasAntiSpamArmor || m_transitCounterAttack)
                Fighter.GetStatus().SetSuperArmor(true, false);

            if (m_getupHyperArmorDoOnce && !m_gettingUp)
            {
                m_getupHyperArmorDoOnce = false;
                Fighter.GetStatus().SetSuperArmor(false);
            }

            if (m_gettingUp)
            {
                m_getupHyperArmorDoOnce = true;
                Fighter.GetStatus().SetSuperArmor(true);

                OnStartGettingUp();
            }


            if(!m_isMortalAttackDoOnce)
            {
                if (IsMortalAttack())
                    m_isMortalAttackDoOnce = true;
            }
            else
            {
                if (!IsMortalAttack())
                {
                    m_isMortalAttackDoOnce = false;
                    Character.Components.EffectEvent.Get().StopEvent((EffectEventCharaID)1501, true);
                }
            }


            if (m_mortalSkill != RPGSkillID.invalid || IsMortalAttack())
            {
                Fighter.GetStatus().SetSuperArmor(true);
            }

            
            if (m_mortalSkill != RPGSkillID.invalid)
            {
                BattleTurnManager.ChangeActionStep(BattleTurnManager.ActionStep.ActionStart);
                BattleTurnManager.ForceCounterCommand(Fighter, BrawlerBattleManager.PlayerFighter, m_mortalSkill);

                m_mortalSkill = RPGSkillID.invalid;
                m_numMortalAttacks++;

                Fighter.Character.Components.EffectEvent.Get().PlayEventOverride((EffectEventCharaID)1501);
                SoundManager.PlayCue(DBManager.GetSoundCuesheet("y8b_common"), 1, 0);

                DragonEngine.Log("TRANSIT MORTAL ATTACK!");
            }
            else
            {
                if (m_transitCounterAttack)
                {
                    m_counterAttackTime += DragonEngine.deltaTime;
                    PerformGenericCounterAttack();
                    m_transitCounterAttack = false;
                }
            }
        }


        public void ExecuteCounterAttack(RPGSkillID id, bool showEffect)
        {
          //  m_allowCounterEffectDoOnce = showEffect;
            BattleTurnManager.ForceCounterCommand(Fighter, BrawlerBattleManager.PlayerFighter, id);
            SoundManager.PlayCue((SoundCuesheetID)7, 4, 0);

            //Grant hyperarmor for 1.5 secs
            //TODO: Make this based on animation time.
            m_counterAttacking = true;
            DETaskTime hpyerArmor = new DETaskTime(1.5f, delegate { m_counterAttacking = false; Character.GetBattleStatus().SetSuperArmor(false); }, true, delegate { Character.GetBattleStatus().SetSuperArmor(true); return false; }, true);

            // OnCounterAttack?.Invoke();

            RecentHitsWithoutAttack = 0;
        }

        public bool IsCounterAttacking()
        {
            return m_counterAttacking;
        }

        /// <summary>
        /// Reliably can play skills, however original skill effect may play as well.
        /// </summary>
        public void OnMyTurnStart()
        {
            LastTurnTime = 0;
            OnMyTurnStartEvent();
        }

        private void OnPlayerStartAttack()
        {
            OnPlayerStartAttackingEvent();
        }

        protected void PerformGenericCounterAttack()
        {
            ExecuteCounterAttack(CounterAttacks[new Random().Next(0, CounterAttacks.Count)], true);
            Character.Components.EffectEvent.Get().PlayEvent((EffectEventCharaID)206);
            SoundManager.PlayCue(SoundCuesheetID.battle_common, 5, 0);
        }

        protected virtual void OnMyTurnStartEvent()
        {

        }

        protected virtual void OnPlayerStartAttackingEvent()
        {

        }

        public virtual void MyTurnUpdate()
        {
            if(m_mortalSkill != 0)
            {
                if(!IsMortalAttack())
                {
                    BattleTurnManager.ForceCounterCommand(Fighter, BrawlerBattleManager.PlayerFighter, m_mortalSkill);
                    Fighter.Character.Components.EffectEvent.Get().PlayEventOverride((EffectEventCharaID)1501);
                    SoundManager.PlayCue(DBManager.GetSoundCuesheet("y8b_common"), 1, 0);
                    DragonEngine.Log("Proc mortal attack");
                   // m_numMortalAttacks++;
                   // m_mortalSkill = 0;

                    return;
                }
            }
        }

        protected virtual void OnStartGettingUp()
        {

        }

        public void ApplyFear(float seconds)
        {
            ExEffectInfo str = new ExEffectInfo();
            str.effID = 11;
            str.idk = str.effID;
            str.effSetID = 1479;
            str.idk2 = str.effSetID;
            str.category = 0x11;
            str.bKeepInfinity = true;
            str.nKeepDamage = 255;

            var ptr = str.ToIntPtr();
            Fighter.GetStatus().AddExEffect(str.ToIntPtr(), false, false);
            Marshal.FreeHGlobal(ptr);

            new DETaskTime(seconds, delegate
            {
                Fighter.GetStatus().RemoveExEffect(11, false, false);
            });
        }
    }
}
