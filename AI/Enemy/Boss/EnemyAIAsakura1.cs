using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIAsakura1 : EnemyAIBoss
    {
        private bool m_hactDoneOnce = false;
        private bool m_mortalDoneOnce = false;

        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("boss_asakura_punch"));
        }

        //Asakura (Chapter 1): Huge emphasis on avoiding the player until he is alone.
        public override bool TransitSway(IntPtr battleDamageInfo)
        {
            if (!IsMyTurn())
                return true;

            return base.TransitSway(battleDamageInfo);
        }

        public override void CombatUpdate()
        {
            base.CombatUpdate();

            if (!m_hactDoneOnce)
                if (Fighter.IsHPBelowRatio(0.45f))
                    PerformMortalTutorial();
        }


        public override bool CanBeHActed()
        {
            if (BrawlerBattleManager.AllEnemies.Length > 1)
                return false;

            return base.CanBeHActed();
        }


        private void PerformMortalTutorial()
        {
            HActRequestOptions opts = new HActRequestOptions();
            opts.id = DBManager.GetTalkParam("y8bb1430_ask_rush");

            opts.Register(HActReplaceID.hu_player, BrawlerBattleManager.PlayerCharacter);
            opts.Register(HActReplaceID.hu_enemy_00, Character);

            opts.base_mtx.matrix = Character.GetMatrix();

            opts.base_mtx.matrix.Position = new Vector3(93.52f, 0f, 304.11f);
            opts.base_mtx.matrix.ForwardDirection = new Vector3(0.12f, 0, -0.99f);
            opts.base_mtx.matrix.UpDirection = new Vector3(0, 1f, 0);
            opts.base_mtx.matrix.LeftDirection = new Vector3(-0.99f, 0, -0.12f);

            HeatActionManager.RequestTalk(opts);
            HeatActionManager.OnHActEndEvent += PerformMortal;

            m_hactDoneOnce = true;
        }

        private void PerformMortal()
        {
            if (m_mortalDoneOnce)
                return;

            HeatActionManager.OnHActEndEvent -= PerformMortal;

            m_mortalDoneOnce = true;

            HActRequestOptions opts = HActRequestOptions.Simple(DBManager.GetTalkParam("y8b_tutorial_btl01_0400_01"));
            opts.base_mtx.matrix = BrawlerBattleManager.PlayerCharacter.GetMatrix();

            new DETaskTime(0.1f, 
                delegate 
                {
                    m_mortalSkill = DBManager.GetSkill("e_asakura_mortal_attack");
                    HeatActionManager.RequestTalk(opts);
                }
            );
        }
    }
}
