using DragonEngineLibrary;
using System;


namespace LikeABrawler2
{
    //Gasmasked shotgun enemy
    internal class EnemyAIBossWPJ_BTL11_0060 : EnemyAIBossWPJ
    {
        private static bool m_hactOnce = false;

        public override void CombatUpdate()
        {
            base.CombatUpdate();

            /*
            if (BrawlerBattleManager.BattleConfigID == 190)
            {
                if (!m_hactOnce)
                {
                    if (!BrawlerBattleManager.IsHActOrWaiting)
                    {
                        if (Fighter.IsHPBelowRatio(0.15f))
                        {
                            DOHact();
                            m_hactOnce = true;
                        }
                    }
                }
            }
            */
        }

        private void DOHact()
        {
            HActRequestOptions opts = new HActRequestOptions();
            opts.id = DBManager.GetTalkParam("eb1510_boss_power_1vs1");

            opts.Register(HActReplaceID.hu_player, BrawlerBattleManager.PlayerCharacter);
            opts.Register(HActReplaceID.hu_enemy_00, Character);

            opts.base_mtx.matrix = Character.GetMatrix();

            opts.base_mtx.matrix.Position = new Vector3(-357.29f, 20.11f, 220.53f);
            opts.base_mtx.matrix.ForwardDirection = new Vector3(0.93f, 0, 0.37f);
            opts.base_mtx.matrix.UpDirection = new Vector3(0, 1f, 0);
            opts.base_mtx.matrix.LeftDirection = new Vector3(0.37f, 0, -0.93f);

            HeatActionManager.RequestTalk(opts);
        }
    }
}
