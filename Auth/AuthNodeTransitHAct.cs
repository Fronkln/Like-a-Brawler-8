using System;
using System.Runtime.InteropServices;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    internal static class AuthNodeTransitHAct
    {
        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, IntPtr parent)
        {
            IntPtr hactNamePtr = (IntPtr)(thisObj.ToInt64() + 48);
            string hactName = Marshal.PtrToStringAnsi(hactNamePtr);

            HActRequestOptions opts = new HActRequestOptions();
            opts.id = DBManager.GetTalkParam(hactName);

            if (opts.id <= 0)
                return;

            opts.Register(HActReplaceID.hu_player, BrawlerBattleManager.PlayerCharacter);

            if (BrawlerBattleManager.AllEnemiesNearest.Length > 0)
                opts.Register(HActReplaceID.hu_enemy_00, BrawlerBattleManager.AllEnemiesNearest[0].Character);

            opts.is_force_play = true;
            opts.base_mtx.matrix = BrawlerBattleManager.PlayerCharacter.GetMatrix();


            if(HeatActionManager.RequestTalk(opts))
            {
                HeatActionInformation inf = new HeatActionInformation();
                inf.Performer = BrawlerBattleManager.PlayerFighter;
                inf.Map = new System.Collections.Generic.Dictionary<ElvisCommand.HeatActionActorType, Fighter>();

                if (BrawlerBattleManager.AllEnemiesNearest.Length > 0)
                    inf.Map[ElvisCommand.HeatActionActorType.Enemy1] = BrawlerBattleManager.AllEnemiesNearest[0];

                HeatActionManager.PerformingHAct = inf;
            }

            DragonEngine.Log(hactName);
            DragonEngine.Log("transit");
        }
    }
}
