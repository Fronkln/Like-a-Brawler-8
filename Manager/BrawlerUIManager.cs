using DragonEngineLibrary;
using DragonEngineLibrary.Service;
using ElvisCommand;
using System.Linq;

namespace LikeABrawler2
{
    internal static class BrawlerUIManager
    {
        private static UIHandleBase m_hactPrompt;
        private static UIHandleBase m_wepPickup;

        private const bool UseClassicGauge = false;

        public static UIHandleBase RealtimeGauge;
        public static UIHandleBase GaugeRoot;

        private static UIHandleBase m_realtimeGaugeHealth;
        private static UIHandleBase m_realtimeGaugeHeat;

        private static UIHandleBase m_playerGaugeRoot;
        private static UIHandleBase m_playerGauge;
        private static UIHandleBase m_playerGauge_NextLabel;
        private static UIHandleBase m_playerGauge_HealthGauge;
        private static UIHandleBase m_playerGauge_HealthGaugeLabel;
        private static UIHandleBase m_playerGauge_HeatGauge;
        private static UIHandleBase m_playerGauge_HeatGaugeLabel;

        public static UIHandleBase Minimap;

        private static bool m_uiCreated = false;
        private static bool m_playerSpawnedDoOnce = false;

        public static void Init()
        {
            BrawlerBattleManager.OnActionStartEvent += OnBattleStart;
            BrawlerBattleManager.OnBattleEndEvent += OnBattleEnd;

            HeatActionManager.OnHActStartEvent += OnHActStart;
            HeatActionManager.OnHActEndEvent += OnHActEnd;
        }

        private static void OnBattleStart()
        {
            if(!m_uiCreated)
            {
                m_uiCreated = true;
                CreateUI();
            }

            ProcessPlayerGauge();

            DragonEngine.Log("Player Gauge: " + m_playerGauge);
        }

        private static uint GetRealtimeGaugeID()
        {
            uint characterID = (uint)BrawlerBattleManager.PlayerCharacter.Attributes.chara_id;

            //Reborn Kiryu
            if (characterID == 28267 ||
                characterID == 26144 ||
                characterID == 26143 ||
                characterID == 17305 ||
                characterID == 17304 ||
                characterID == 17196)
                return 1172;

            if (BrawlerPlayer.IsKasuga())
                return 394;
            else if (BrawlerPlayer.IsKiryu())
                return 1171;
            else
                return 394; //temp;
        }

        private static void ProcessPlayerGauge()
        {
            m_playerGauge = GetPlayerGauge();
            m_playerGauge_NextLabel = m_playerGauge.GetChild(4);
            m_playerGauge_HealthGauge = m_playerGauge.GetChild(5);
            m_playerGauge_HealthGaugeLabel = m_playerGauge.GetChild(8);
            m_playerGauge_HeatGauge = m_playerGauge.GetChild(6);
            m_playerGauge_HeatGaugeLabel = m_playerGauge.GetChild(7);

            if (!UseClassicGauge)
            {
                if (RealtimeGauge.Handle == 0)
                {
                    RealtimeGauge = UI.Play(GetRealtimeGaugeID(), 0);
                    m_realtimeGaugeHealth = RealtimeGauge.GetChild(0).GetChild(0).GetChild(1);
                    m_realtimeGaugeHeat = RealtimeGauge.GetChild(0).GetChild(0).GetChild(2);
                    m_realtimeGaugeHeat.GetChild(3).SetVisible(false);
                }
            }
        }

        public static void DoSoloFight()
        {
            if (GaugeRoot.GetChildCount() <= 1)
                return;

            //release UI until we are at player
            var ch1 = GaugeRoot.GetChild(0);
            var ch2 = GaugeRoot.GetChild(1);
            var ch3 = GaugeRoot.GetChild(2);

            ch1.Release();
            ch2.Release();
            ch3.Release();
        }

        private static void OnBattleEnd()
        {
            m_hactPrompt.SetVisible(false);
            
            if(RealtimeGauge.Handle != 0)
            {
                RealtimeGauge.Release();
                RealtimeGauge.Handle = 0;
            }
        }


        private static void OnHActStart()
        {
            GaugeRoot.SetVisible(false);
        }

        private static void OnHActEnd()
        {
            GaugeRoot.SetVisible(true);
        }
        public static void OnSwitchToTurnBased()
        {
            m_hactPrompt.SetVisible(false);
            BattleTurnManager.UIRoot.SetVisible(true);
        }

        private static void CreateUI()
        {
            CreateHActPromptWorkaround();
            m_wepPickup = UI.Play(846, 0);
        }


        private unsafe static UIHandleBase GetPlayerGauge()
        {

            EntityBase sceneEnt = SceneService.CurrentScene.Get().GetSceneEntity((SceneEntity)0X9F);

            if (!sceneEnt.IsValid())
                return new UIHandleBase();

            ulong* handle = (ulong*)(sceneEnt.Pointer.ToInt64() + 0xC8);
            UIHandleBase uiHandle = new UIHandleBase(*handle);

            UIHandleBase gauge = new UIHandleBase();

            UIHandleBase gaugesRoot = uiHandle.GetChild(0).GetChild(0);
            GaugeRoot = gaugesRoot;

            if (!BrawlerPlayer.IsOtherPlayer() || BrawlerPlayer.IsOtherPlayerLeader())
            {
                uint numGauges = gaugesRoot.GetChildCount();
                uint idx = 0;

                m_playerGaugeRoot = gaugesRoot.GetChild((int)idx);

                int highestIndex = 0;

                for(int i = 0; i < (int)Player.ID.num; i++)
                {
                    int ideex = NakamaManager.FindIndex((Player.ID)i);

                    if (ideex > highestIndex)
                        highestIndex = ideex;
                }

                idx = (uint)highestIndex;
                /*
                
                if (numGauges > 4)
                    idx = 3;
                else
                    idx = numGauges - 1;
                */

                gauge = gaugesRoot.GetChild((int)idx).GetChild(0).GetChild(0);
                m_playerGaugeRoot = gaugesRoot.GetChild((int)idx);
            }
            else
            {
                int idx = NakamaManager.FindIndex(BrawlerPlayer.CurrentPlayer);
                idx = idx + 2;

                gauge = gaugesRoot.GetChild(idx).GetChild(0).GetChild(0);
            }

            return gauge;
        }

        public static bool ShouldSeeRPGUI()
        {
            return BrawlerBattleManager.IsDeliveryHelping;
        }

        public static void Update()
        {
            if (Mod.IsRealtime() &&
                BrawlerBattleManager.Battling &&
                !BrawlerBattleManager.IsHAct
                && BrawlerBattleManager.CurrentPhase == BattleTurnManager.TurnPhase.Action && !Debug.NoUI)
                Minimap.SetVisibilityID(28); //minimap_combat
            else
            {
                Minimap.SetVisibilityID(1);
                m_hactPrompt.SetVisible(false);
                m_wepPickup.SetVisible(false);
            }

            if (!Mod.IsRealtime())
                return;

            if(BrawlerBattleManager.IsHAct)
                GaugeRoot.SetVisible(false);
            else
            {
                if(!UseClassicGauge)
                {
                    m_playerGaugeRoot.SetVisible(!BrawlerBattleManager.Battling);
                    RealtimeGauge.SetVisible(BrawlerBattleManager.Battling && !BrawlerBattleManager.PlayerCharacter.IsDead() && BattleTurnManager.CurrentPhase > BattleTurnManager.TurnPhase.Start && !(BrawlerBattleManager.IsHAct && !HeatActionManager.IsY8BHact));
                }
            }

            //ProcessPlayerGauge();
            Minimap.SetVisible(!Debug.NoUI);

            if (BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action || BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Cleanup)
                BattleTurnManager.UIRoot.SetVisible(ShouldSeeRPGUI());

            m_hactPrompt.SetVisible(HeatActionManager.CanHAct());
            m_wepPickup.SetVisible(WeaponManager.NearestAsset != null);

            if (WeaponManager.NearestAsset != null)
                m_wepPickup.SetPosition(WeaponManager.NearestAsset.GetPosCenter());


            long playerHp = BrawlerBattleManager.PlayerFighter.GetStatus().CurrentHP;

            //Game only updates these values *sometimes* when they are changed during combat. and we dont want that, we always want constant update
            //To account for things like game script changing health, or player using healing on menu

            if (BrawlerBattleManager.Battling)
            {
                GaugeRoot.SetVisible(!Debug.NoUI);

                if (UseClassicGauge)
                {
                    m_playerGauge_HealthGauge.SetValue((float)playerHp / (float)Player.GetHPMax(BrawlerPlayer.CurrentPlayer));
                    m_playerGauge_HealthGaugeLabel.SetText(playerHp.ToString());
                    m_playerGauge_HeatGauge.SetValue((float)Player.GetHeatNow(BrawlerPlayer.CurrentPlayer) / (float)Player.GetHeatMax(BrawlerPlayer.CurrentPlayer));
                    m_playerGauge_HeatGaugeLabel.SetText(Player.GetHeatNow(BrawlerPlayer.CurrentPlayer).ToString());
                    m_playerGauge_NextLabel.SetVisible(false);

                    if (BrawlerPlayer.IsKiryu())
                    {
                        //Kiryu style icon
                        m_playerGauge.GetChild(0).SetVisible(false);
                    }
                }
                else
                {
                    m_realtimeGaugeHealth.SetValue((float)playerHp / (float)Player.GetHPMax(BrawlerPlayer.CurrentPlayer));
                    m_realtimeGaugeHeat.SetValue((float)Player.GetHeatNow(BrawlerPlayer.CurrentPlayer) / (float)Player.GetHeatMax(BrawlerPlayer.CurrentPlayer));
                }
            }

            bool canHAct = HeatActionManager.AllowHAct();

            m_playerGauge_HeatGauge.SetVisible(canHAct);
            m_playerGauge_HeatGaugeLabel.SetVisible(canHAct);
        }

        //TODO: Only do this on battle start
        //Create Kiwami action UI (texture has been made so small it looks like a prompt)
        //Set to 2.5 seconds
        //Pause
        //Profit
        private static void CreateHActPromptWorkaround()
        {
            m_hactPrompt = UI.Play(1170, 0); //y8b_can_hact
            m_hactPrompt.SetVisible(false);
            m_hactPrompt.SetFrame(85);

            m_hactPrompt.Pause();
        }
    }
}
