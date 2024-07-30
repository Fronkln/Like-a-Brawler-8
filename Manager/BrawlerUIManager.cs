using DragonEngineLibrary;
using ElvisCommand;
using System.Linq;

namespace LikeABrawler2
{
    internal static class BrawlerUIManager
    {
        private static UIHandleBase m_hactPrompt;
        private static UIHandleBase m_wepPickup;

        private static UIHandleBase m_gaugeRoot;
        private static UIHandleBase m_playerGauge;
        private static UIHandleBase m_playerGauge_NextLabel;
        private static UIHandleBase m_playerGauge_HealthGauge;
        private static UIHandleBase m_playerGauge_HealthGaugeLabel;
        private static UIHandleBase m_playerGauge_HeatGauge;
        private static UIHandleBase m_playerGauge_HeatGaugeLabel;

        public static UIHandleBase Minimap;

        private static bool m_uiCreated = false;

        public static void Init()
        {
            BrawlerBattleManager.OnBattleStartEvent += OnBattleStart;
            BrawlerBattleManager.OnBattleEndEvent += OnBattleEnd;
        }

        private static void OnBattleStart()
        {
            if(!m_uiCreated)
            {
                m_uiCreated = true;
                CreateUI();
            }

            m_playerGauge = GetPlayerGauge();
            m_playerGauge_NextLabel = m_playerGauge.GetChild(4);
            m_playerGauge_HealthGauge = m_playerGauge.GetChild(5);
            m_playerGauge_HealthGaugeLabel = m_playerGauge.GetChild(8);
            m_playerGauge_HeatGauge = m_playerGauge.GetChild(6);
            m_playerGauge_HeatGaugeLabel = m_playerGauge.GetChild(7);

            DragonEngine.Log("Player Gauge: " + m_playerGauge);
        }

        private static void OnBattleEnd()
        {
            m_hactPrompt.SetVisible(false);
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
            ulong* handle = (ulong*)(DragonEngine.GetHumanPlayer().GetSceneEntity((SceneEntity)0X9F).Get().Pointer.ToInt64() + 0xC8);
            UIHandleBase uiHandle = new UIHandleBase(*handle);

            UIHandleBase gauge = new UIHandleBase();

            UIHandleBase gaugesRoot = uiHandle.GetChild(0).GetChild(0);
            m_gaugeRoot = gaugesRoot;

            for (int i = 0; i < gaugesRoot.GetChildCount(); i++)
            {
                if (!gaugesRoot.GetChild(i).IsVisible())
                    break;

                gauge = gaugesRoot.GetChild(i).GetChild(0).GetChild(0);
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
            }

            if (!Mod.IsRealtime())
                return;

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
                m_gaugeRoot.SetVisible(!Debug.NoUI);
                m_playerGauge_HealthGauge.SetValue((float)playerHp / (float)Player.GetHPMax(BrawlerPlayer.CurrentPlayer));
                m_playerGauge_HealthGaugeLabel.SetText(playerHp.ToString());
                m_playerGauge_HeatGauge.SetValue((float)Player.GetHeatNow(BrawlerPlayer.CurrentPlayer) / (float)Player.GetHeatMax(BrawlerPlayer.CurrentPlayer));
                m_playerGauge_HeatGaugeLabel.SetText(Player.GetHeatNow(BrawlerPlayer.CurrentPlayer).ToString());
                m_playerGauge_NextLabel.SetVisible(false);

                if(BrawlerPlayer.IsKiryu())
                {
                    //Kiryu style icon
                    m_playerGauge.GetChild(0).SetVisible(false);
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
