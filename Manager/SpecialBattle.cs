using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace LikeABrawler2
{
    public static class SpecialBattle
    {
        private static bool m_adachiSwapped = false;


        public static void OnBattleEnd()
        {
            m_adachiSwapped = false;
            m_dreamSequenceStart = false;
            m_dreamSequenceFighting = false;
            m_dreamSequenceFinish = false;
            m_dreamSequenceEnemy = new Fighter();
            m_warpedOnce = false;
        }

        /// <summary>
        /// elvis_lng04_btl11_0030 theater split fight. Adachi becomes protag momentarily
        /// </summary>
        public unsafe static void SplitFight()
        {
            if (m_adachiSwapped)
                return;

            int adachiIdx = NakamaManager.FindIndex(Player.ID.adachi);
            BrawlerBattleManager.MakeNakamaMain((uint)adachiIdx);

            m_adachiSwapped = true;


            int tomizawaIdx = NakamaManager.FindIndex(Player.ID.tomizawa);

            if (tomizawaIdx > 0)
            {
                Fighter tomizawaFighter = FighterManager.GetFighter((uint)tomizawaIdx);
                BaseAI tomizawaAI = tomizawaFighter.TryGetAI();

                if (tomizawaAI is BaseSupporterAI)
                    (tomizawaAI as BaseSupporterAI).TakeTurn();
            }

            DragonEngine.Log("Adachi Files");
        }

        public static void Update()
        {
            switch (BrawlerBattleManager.BattleConfigID)
            {
                case 161:
                    UpdateTriosFight();
                    break;
                case 189:
                    if (m_adachiSwapped)
                    {
                        if (BrawlerPlayer.IsOtherPlayer())
                        {
                            if (BrawlerBattleManager.CurrentPhase == BattleTurnManager.TurnPhase.Event)
                                if (!HeatActionManager.IsY8BHact)
                                    BrawlerBattleManager.MakeNakamaMain(0); // back to kasuga
                        }
                    }
                    break;
            }
        }

        public static void TriosFight()
        {
            m_dreamBattleStartPos = DragonEngine.GetHumanPlayer().Transform.Position;
        }

        private static bool m_dreamSequenceStart = false;
        public static bool m_dreamSequenceFighting = false;
        private static bool m_dreamSequenceFinish = false;
        private static bool m_warpedOnce = false;
        public static Fighter m_dreamSequenceEnemy = new Fighter();
        private static Vector3 m_dreamBattleStartPos;
        private unsafe static void UpdateTriosFight()
        {
            EnemyAIBoss daigo = EnemyAIBossDaigo.Instance;
            EnemyAIBoss majima = EnemyAIBossMajima.Instance;
            EnemyAIBoss saejima = EnemyAIBossSaejima.Instance;

            if (!m_dreamSequenceStart && !m_dreamSequenceFighting)
            {
                if (daigo != null && daigo.Fighter.IsValid())
                {
                    if (daigo.Character.GetMotion().GmtID == (MotionID)21002)
                        OnStartDreamSequence(daigo.Fighter);
                }

                if (majima != null && majima.Fighter.IsValid())
                {
                    if (majima.Character.GetMotion().GmtID == (MotionID)21003)
                        OnStartDreamSequence(majima.Fighter);
                }

                if (saejima != null && saejima.Fighter.IsValid())
                {
                    if (saejima.Character.GetMotion().GmtID == (MotionID)21004)
                        OnStartDreamSequence(saejima.Fighter);
                }
            }

            if (IsDreamSequence())
            {
                if (IsDreamSequenceStart())
                {
                    if ((uint)BrawlerBattleManager.PlayerCharacter.GetMotion().GmtID == 19778)
                    {
                        m_dreamSequenceStart = false;
                        m_dreamSequenceFighting = true;

                        BrawlerBattleManager.PlayerCharacter.HumanModeManager.CommandsetModel.SetCommandSet(0, (BattleCommandSetID)FighterCommandManager.FindSetID("p_kiryu_legend"));

                        DragonEngine.Log("START FIGHTING!");
                    }
                }
                else
                {

                    bool playHactOnce = false;

                    if (m_dreamSequenceFighting)
                    {
                        bool* free_movement_mode = (bool*)(BrawlerBattleManager.PlayerCharacter.Pointer.ToInt64() + 0x11A3);
                        *free_movement_mode = true;

                        if (!m_warpedOnce)
                        {
                            if (Vector3.Distance(m_dreamBattleStartPos, BrawlerBattleManager.PlayerCharacter.Transform.Position) >= 50)
                            {
                                m_warpedOnce = true;

                                if(m_dreamSequenceEnemy == EnemyAIBossDaigo.Instance.Fighter)
                                {
                                    BrawlerBattleManager.PlayerCharacter.GetRender().Reload((CharacterID)9416);
                                    m_dreamSequenceEnemy.Character.GetRender().Reload((CharacterID)9403);
                                    
                                    new DETask(delegate
                                    {
                                        return !BrawlerBattleManager.IsHActOrWaiting && m_dreamSequenceEnemy.IsHPBelowRatio(0.3f) & !m_dreamSequenceEnemy.Character.HumanModeManager.IsDamage();
                                    }, delegate
                                    {
                                        HActRequestOptions opts = new HActRequestOptions();
                                        opts.id = DBManager.GetTalkParam("y8bb1740_dgo_punch_lock");

                                        opts.Register(HActReplaceID.hu_player1, BrawlerBattleManager.PlayerCharacter);
                                        opts.Register(HActReplaceID.hu_enemy_00, m_dreamSequenceEnemy.Character);

                                        opts.base_mtx.matrix = m_dreamSequenceEnemy.Character.GetMatrix();

                                        opts.base_mtx.matrix.Position = new Vector3(297.36f, -49.98f, 350.54f);
                                        opts.base_mtx.matrix.ForwardDirection = new Vector3(0.93f, 0, 0.37f);
                                        opts.base_mtx.matrix.UpDirection = new Vector3(0, 1f, 0);
                                        opts.base_mtx.matrix.LeftDirection = new Vector3(0.37f, 0, -0.93f);


                                        new DETask(delegate
                                        {
                                            HeatActionManager.RequestTalk(opts);
                                            return AuthManager.PlayingScene.IsValid() && AuthManager.PlayingScene.Get().TalkParamID == opts.id;
                                        }, delegate { playHactOnce = true; });

                                    });
                                }

                                if(m_dreamSequenceEnemy == EnemyAIBossMajima.Instance.Fighter)
                                {
                                    BrawlerBattleManager.PlayerCharacter.GetRender().Reload((CharacterID)11486);
                                    m_dreamSequenceEnemy.Character.GetRender().Reload((CharacterID)103);

                                    new DETask(delegate
                                    {
                                        return !BrawlerBattleManager.IsHActOrWaiting && m_dreamSequenceEnemy.IsHPBelowRatio(0.3f) & !m_dreamSequenceEnemy.Character.HumanModeManager.IsDamage();
                                    }, delegate
                                    {
                                        HActRequestOptions opts = new HActRequestOptions();
                                        opts.id = DBManager.GetTalkParam("y8bb1750_majima_combo");

                                        opts.Register(HActReplaceID.hu_player1, BrawlerBattleManager.PlayerCharacter);
                                        opts.Register(HActReplaceID.hu_enemy_00, m_dreamSequenceEnemy.Character);

                                        opts.base_mtx.matrix = m_dreamSequenceEnemy.Character.GetMatrix();

                                        opts.base_mtx.matrix.Position = new Vector3(-0.066f, -49.96f, 349.21f);
                                        opts.base_mtx.matrix.ForwardDirection = new Vector3(-0.88f, 0, 0.48f);
                                        opts.base_mtx.matrix.UpDirection = new Vector3(0, 1f, 0);
                                        opts.base_mtx.matrix.LeftDirection = new Vector3(0.48f, 0, 0.88f);

                                        new DETask(delegate
                                        {
                                            HeatActionManager.RequestTalk(opts);
                                            return AuthManager.PlayingScene.IsValid() && AuthManager.PlayingScene.Get().TalkParamID == opts.id;
                                        }, delegate { playHactOnce = true; });
                                    });
                                }

                                if(m_dreamSequenceEnemy == EnemyAIBossSaejima.Instance.Fighter)
                                {
                                    BrawlerBattleManager.PlayerCharacter.GetRender().Reload((CharacterID)9402);
                                    m_dreamSequenceEnemy.Character.GetRender().Reload((CharacterID)16652);

                                    new DETask(delegate
                                    {
                                        return !BrawlerBattleManager.IsHActOrWaiting && m_dreamSequenceEnemy.IsHPBelowRatio(0.3f) & !m_dreamSequenceEnemy.Character.HumanModeManager.IsDamage();
                                    }, delegate
                                    {
                                        HActRequestOptions opts = new HActRequestOptions();
                                        opts.id = DBManager.GetTalkParam("y8bb1760_sae_soul");

                                        opts.Register(HActReplaceID.hu_player1, BrawlerBattleManager.PlayerCharacter);
                                        opts.Register(HActReplaceID.hu_enemy_00, m_dreamSequenceEnemy.Character);

                                        opts.base_mtx.matrix = m_dreamSequenceEnemy.Character.GetMatrix();

                                        opts.base_mtx.matrix.Position = new Vector3(147.95f, -49.96f, 349.21f);
                                        opts.base_mtx.matrix.ForwardDirection = new Vector3(-0.88f, 0, 0.48f);
                                        opts.base_mtx.matrix.UpDirection = new Vector3(0, 1f, 0);
                                        opts.base_mtx.matrix.LeftDirection = new Vector3(0.48f, 0, 0.88f);

                                        new DETask(delegate
                                        {
                                            HeatActionManager.RequestTalk(opts);
                                            return AuthManager.PlayingScene.IsValid() && AuthManager.PlayingScene.Get().TalkParamID == opts.id;
                                        }, delegate { playHactOnce = true; });
                                    });
                                }

                                DragonEngine.Log("WARPED");
                            }
                        }

                        if (!GameVarManager.GetValueBool(GameVarID.is_hact) && m_dreamSequenceEnemy.IsDead() || !m_dreamSequenceEnemy.IsValid())
                        {
                            m_dreamSequenceFighting = false;
                            m_dreamSequenceStart = false;
                            m_dreamSequenceFinish = true;

                            new DETask(delegate
                            {
                                return Vector3.Distance(m_dreamBattleStartPos, BrawlerBattleManager.PlayerCharacter.Transform.Position) <= 50;
                            }, delegate
                            {
                                //player gott warped back to the area
                                BrawlerBattleManager.PlayerCharacter.GetRender().Reload(BrawlerBattleManager.PlayerCharacter.Attributes.chara_id);

                                new DETaskTime(0.5f, delegate
                                {
                                    m_dreamSequenceFinish = false;
                                    m_warpedOnce = false;
                                    BrawlerPlayer.OnStyleSwitch(PlayerStyle.Default, true);
                                    BrawlerBattleManager.ChangeToRealtime();
                                });
                            });
                            BrawlerPlayer.OnStyleSwitch(PlayerStyle.Default, true);
                            DragonEngine.Log("Dream sequence end.");
                        }
                    }
                }
            }
        }

        private static void OnStartDreamSequence(Fighter enemy)
        {
            m_dreamSequenceStart = true;
            DragonEngine.Log("DREAM SEQUENCE!");
            BrawlerPlayer.OnExtremeHeatModeOFF();
            BrawlerPlayer.CurrentStyle = PlayerStyle.Resurgence;
            BrawlerBattleManager.ChangeToTurnBased();

            m_dreamSequenceEnemy = enemy;

            DETask task = null;
            task = new DETask(
                delegate
                {
                    UpdateTriosFight();
                    return !IsDreamSequence();
                }, null);
        }

        public static bool IsDreamSequenceStart()
        {
            return m_dreamSequenceStart == true;
        }

        public static bool IsDreamSequenceFighting()
        {
            return m_dreamSequenceFighting == true;
        }

        public static bool IsDreamSequence()
        {
            return m_dreamSequenceStart || m_dreamSequenceFighting || m_dreamSequenceFinish;
        }
    }
}
