using DragonEngineLibrary;
using System;
using System.Collections.Generic;

namespace LikeABrawler2
{
    //TODO: Also maybe use this for bosses?
    public static class HActLifeGaugeManager
    {

        private class GaugeInf
        {
            public Character Owner;
            public long CurrentHP;
            public long MaxHP;

            public UIHandleBase m_uiRoot;
            public UIHandleBase m_nameRoot;
            public UIHandleBase m_gaugeRoot;
            public UIHandleBase m_lifeRoot;

            public void SetValue(long curHp)
            {
                if(m_activeGauge != this)
                {
                    HideAllGauge();
                    SetGaugeVisible(Owner.UID);
                }

                CurrentHP = curHp;

                float fillRatio = (float)curHp / (float)MaxHP;
                m_gaugeRoot.SetValue(fillRatio);
            }
        }

        private static Fighter m_lastInitedFighter = new Fighter();
        private static Dictionary<uint, GaugeInf> m_gauges = new Dictionary<uint, GaugeInf>();
        private static GaugeInf m_activeGauge = null;

        public static void Init()
        {
            HActDamage.OnDamageDealt += OnDamageDealt;
            HeatActionManager.OnHActStartEvent += OnHActStart;
            HeatActionManager.OnHActEndEvent += OnHActEnd;
        }

        private unsafe static GaugeInf InitGauge()
        {
            GaugeInf newGauge = new GaugeInf();

            /*
            //TODO VERY IMPORTANT: RELEASE OLD HANDLE
            if (m_uiRoot.Handle != 0)
            {
                //release
                m_uiRoot.Release();
                m_uiRoot.Handle = 0;
            }
            */

            newGauge.m_uiRoot = UI.Play(151, 0); //boss_life_gauge leftover from JE
            //m_uiRoot.PlayAnimationSet(693);
            newGauge.m_nameRoot = newGauge.m_uiRoot.GetChild(0);
            newGauge.m_gaugeRoot = newGauge.m_uiRoot.GetChild(0).GetChild(0);
            newGauge.m_lifeRoot = newGauge.m_gaugeRoot.GetChild(3);
            newGauge.m_uiRoot.SetVisible(false);

            return newGauge;
        }

        private static void InitFighter(Fighter fighter)
        {
            ECBattleStatus status = fighter.GetStatus();
            //SetText(fighter.Character.GetConstructor().GetAgentComponent().SoldierInfo.Get().Name);
            SetValue(status.CurrentHP, status.MaxHP);
        }

        public static void SetText(string text)
        {
            //m_nameRoot.SetText(text);
        }

        public static void SetValue(long curHp, long maxHp)
        {
            //m_gaugeRoot.SetValue((float)curHp / (float)maxHp);
        }

        public static void SetVisible(bool visible)
        {
            //m_uiRoot.SetVisible(visible);
        }


        private static void HideAllGauge()
        {
            foreach (var kv in m_gauges)
                kv.Value.m_uiRoot.SetVisible(false);
        }


        public static void Update()
        {
            /*
            if (m_gaugeRoot.Handle != 0)
                if (!BrawlerBattleManager.PlayerFighter.IsValid() || BrawlerBattleManager.PlayerFighter.IsDead())
                {
                    m_uiRoot.Release();
                    m_uiRoot.Handle = 0;
                }
            */

            foreach(var gauge in m_gauges)
            {
                long newHP = gauge.Value.Owner.GetBattleStatus().CurrentHP;

                if(newHP < gauge.Value.CurrentHP)
                {
                    if(newHP > 0)
                         gauge.Value.m_uiRoot.PlayAnimationSet(664); //boss_life_gauge_judge/play_damage
                    else
                        gauge.Value.m_uiRoot.PlayAnimationSet(663); //boss_life_gauge_judge/play_dead

                    gauge.Value.SetValue(newHP);
                }
            }
        }

        private static void OnHActStart()
        {
            BattleTurnManager.TurnPhase phase = BattleTurnManager.CurrentPhase;

            if (BrawlerBattleManager.AllEnemiesNearest.Length <= 0 || phase <= BattleTurnManager.TurnPhase.Start || !HeatActionManager.IsY8BHact)
            {
                SetVisible(false);
                return;
            }

            if(HeatActionManager.PerformingHAct == null)
            {
                SetVisible(false);
                return;
            }

            Fighter firstEnem = new Fighter();

            foreach(var kv in HeatActionManager.PerformingHAct.Map)
            {
                if(kv.Key.ToString().StartsWith("Enemy") && !kv.Value.IsPlayer())
                {
                    if(firstEnem._ptr == IntPtr.Zero)
                        firstEnem = kv.Value;

                    GaugeInf inf = InitGauge();
                    inf.Owner = kv.Value.Character;

                    var status = kv.Value.Character.GetBattleStatus();
                    inf.MaxHP = status.MaxHP;
                    inf.SetValue(status.CurrentHP);

                    var constructor = kv.Value.Character.GetConstructor();
                    var agent = constructor.GetAgentComponent();
                    var soldierInfo = constructor.SoldierInfo.Get();

                    string name = soldierInfo.Name;
                    inf.m_nameRoot.SetText(name);

                    m_gauges[kv.Value.Character.UID] = inf;
                }
            }


            if(firstEnem.IsValid())
                SetGaugeVisible(firstEnem.Character.UID);

            SetVisible(true);

            //if (m_lastInitedFighter != BrawlerBattleManager.AllEnemiesNearest[0])
                //InitFighter((BrawlerBattleManager.AllEnemiesNearest[0])); ;
        }

        private static void SetGaugeVisible(uint fighter)
        {
            if (!m_gauges.ContainsKey(fighter))
                return;

            var gauge = m_gauges[fighter];
            gauge.m_uiRoot.SetVisible(true);
        }

        private static void OnHActEnd()
        {
            foreach(var kv in m_gauges)
            {
                kv.Value.m_uiRoot.Release();
            }

            m_gauges.Clear();
        }

        private static void OnDamageDealt(Character fighter, long oldHp, long newHp)
        {
            if (fighter.Attributes.is_player)
                return;

            ECBattleStatus fighterStatus = fighter.GetBattleStatus();
            long maxHp = fighterStatus.MaxHP;

            SetValue(oldHp, maxHp);
            SetValue(newHp, maxHp);

            /*
            if (newHp == 0)
                m_uiRoot.PlayAnimationSet(695); //boss_life_gauge_judge/play_dead
            else
                m_uiRoot.PlayAnimationSet(696); //boss_life_gauge_judge/play_damage
            */
        }
    }
}