using System;
using DragonEngineLibrary;
namespace LikeABrawler2
{
    internal static class TownsfolkManager
    {
        public static void Init()
        {
            BrawlerBattleManager.OnActionStartEvent += Procedure;
        }

        public static void Procedure()
        {
            if (RevelationManager.IsQueue() || !BrawlerBattleManager.IsEncounter)
                return;

            return;

            PlayNakama();
        }

        public static void PlayNakama()
        {
            Matrix4x4 mtx = new Matrix4x4();
            mtx.Position = new Vector4(731.67f, 0.10f, 24.28f);
            mtx.ForwardDirection = new Vector4(0.16f, 0f, 0.99f, 0);
            mtx.LeftDirection = new Vector4(0.99f, 0, -0.16f, 0);

            HActRequestOptions opts = new HActRequestOptions();
            opts.is_force_play = true;
            opts.base_mtx.matrix = mtx;
            opts.id = DBManager.GetTalkParam("y8bn1010_ric");
            opts.Register(HActReplaceID.hu_enemy_00, BrawlerBattleManager.AllEnemiesNearest[0].Character.UID);
           

            new DETaskTime(0.1f, delegate
            {
                new DETask(delegate { return !BrawlerBattleManager.IsHAct; }, delegate { HeatActionManager.RequestTalk(opts); });
            });
        }
    }
}
