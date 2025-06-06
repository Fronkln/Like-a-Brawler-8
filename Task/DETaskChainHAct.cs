﻿using System;
using System.Collections.Generic;
using System.Linq;
using DragonEngineLibrary;

namespace LikeABrawler2
{
    public class DETaskChainHAct : DETask
    {
        private bool m_awaitingPlay = false;
        private bool m_done = false;

        List<TalkParamID> ids = new List<TalkParamID>();

        public DETaskChainHAct(Action onFinish, bool autoStart, params TalkParamID[] ids) : base(null, onFinish, autoStart)
        {
            m_Func = delegate { return m_done; };
            this.ids = ids.ToList();
        }

        public override void Run()
        {
            if (!BrawlerBattleManager.IsHAct && !HeatActionManager.AwaitingHAct)
            {
                if (m_awaitingPlay)
                    return;
                else
                {
                    if (ids.Count > 0)
                    {
                        m_awaitingPlay = true;
                        HActRequestOptions opts = new HActRequestOptions();
                        opts.id = ids[0];
                        opts.is_force_play = true;
                        opts.base_mtx.matrix = BrawlerBattleManager.PlayerCharacter.GetMatrix();

                        HeatActionManager.IsY8BHact = false;
                        HeatActionManager.ShowEnemyGaugeDoOnce = false;
                        opts.Register(HActReplaceID.hu_player1, BrawlerBattleManager.PlayerCharacter);

                        DragonEngine.Log("hact " + ids[0]);

                        ids.RemoveAt(0);
                    }
                    else
                    {
                        m_done = true;
                        m_FinishFunc?.Invoke();
                        Success = true;
                        return;
                    }
                }
            }
            else
                m_awaitingPlay = false;

            base.Run();
        }
    }
}