using System;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal class EnemyAIAmonLose : EnemyAIBoss
    {
        private bool m_hactOnce = false;

        public override void CombatUpdate()
        {
            base.CombatUpdate();

            if (!m_hactOnce && BrawlerBattleManager.BattleTime >= 35f)
            {
                if (Vector3.Distance(Character.Transform.Position, BrawlerBattleManager.PlayerCharacter.Transform.Position) <= 3f)
                {
                    HActRequestOptions opts = new HActRequestOptions();
                    opts.base_mtx.matrix = Character.GetMatrix();
                    opts.base_mtx.matrix.Position = new Vector3(130f, 0.80f, 144.90f);
                    opts.id = DBManager.GetTalkParam("y8bb1480_amn_binta");
                    opts.is_force_play = true;
                    opts.Register(HActReplaceID.hu_player1, BrawlerBattleManager.PlayerCharacter);
                    opts.Register(HActReplaceID.hu_enemy_00, Character);
                    HeatActionManager.RequestTalk(opts);
                    m_hactOnce = true;
                }
            }
        }
    }
}
