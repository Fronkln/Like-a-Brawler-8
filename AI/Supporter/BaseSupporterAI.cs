using DragonEngineLibrary;
using DragonEngineLibrary.Service;
using System;
using ElvisCommand;
using Yarhl.FileSystem;

namespace LikeABrawler2
{
    internal class BaseSupporterAI : BaseAI
    {
        public SupporterFlags Flags = 0;

        private float m_nextAttackTime = 3.5f;

        public override void LoadContent()
        {
            HActList = Mod.ReadYHC("supporter/chitose_test.ehc");
        }

        public override void HActUpdate()
        {
            HeatActionInformation performableHact = HeatActionSimulator.Check(Fighter, HActList);

            if (performableHact != null)
            {
                performableHact.PosOverride = BrawlerBattleManager.PlayerCharacter.GetPosCenter();
                HeatActionManager.ExecHeatAction(performableHact);

                m_hactCd = HACT_COOLDOWN;
            }
        }

        public virtual bool IsPartyMember()
        {
            return false;
        }

        private float GetWaitTime()
        {
            if(EnemyManager.Enemies.Count <= 1)
                return new Random().Next(14f, 22f);

            if(EnemyManager.Enemies.Count > 3)
                return new Random().Next(12f, 18f);

            if (EnemyManager.Enemies.Count >= 5)
                return new Random().Next(7.5f, 14f);

            return new Random().Next(15f, 22f);
        }

        public virtual void BattleStartEvent()
        {

        }

        public override void CombatUpdate()
        {
            base.CombatUpdate();

            if (SupporterManager.NextSupporterAttacker == null || !SupporterManager.NextSupporterAttacker.Fighter.IsValid())
            {
                m_nextAttackTime -= DragonEngine.deltaTime;

                if (m_nextAttackTime <= 0)
                    TakeTurn();
            }
        }

        public void TakeTurn()
        {
            SupporterManager.NextSupporterAttacker = this;
            m_nextAttackTime = GetWaitTime();
        }

        public override void OnStartTurn()
        {
            base.OnStartTurn();

            foreach (var kv in SupporterManager.Supporters)
                if (kv.Value != this)
                    kv.Value.OnMyAllyStartTurn();
        }

        public void OnMyAllyStartTurn()
        {
            if(m_nextAttackTime < 1.5f)
                m_nextAttackTime += new Random().Next(0.5f, 2.6f);
        }
    }
}
