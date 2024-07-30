using System;
using MinHook;
using DragonEngineLibrary;
using Brawler;

namespace LikeABrawler2
{
    internal static class BrawlerPatches
    {
        public static HookEngine HookEngine = new HookEngine();

        public static HActDamage HActDamage = new HActDamage();
        public static CombatPatches CombatPatches = new CombatPatches();
        public static CombatPlayerPatches CombatPlayerPatches = new CombatPlayerPatches();
        public static HActPatches HActPatches = new HActPatches();
        public static SupporterPatches SupporterPatches = new SupporterPatches();
        public static HumanModePatches HumanModePatches = new HumanModePatches();
        public static InventoryPatches InventoryPatches = new InventoryPatches();
        public static CFCPatches CFCPatches = new CFCPatches();
        public static ParticlePatches ParticlePatches = new ParticlePatches();
        public static PausePatches PausePatches = new PausePatches();
        public static BattleTurnManagerPatches BattleTurnManagerPatches = new BattleTurnManagerPatches();
        public static UIPatches UIPatches = new UIPatches();
        public static CameraPatches CameraPatches = new CameraPatches();
        public static AuthPatches AuthPatches = new AuthPatches();

        public static bool Enabled = false;

        public static void Init()
        {
            //Goes without saying, although LAD8 makes it very easy
            //it's impossible to avoid some hooking and memory altering
            //to make Like A Brawler 8 possible.
            CombatPatches.Init();
            CombatPlayerPatches.Init();
            SupporterPatches.Init();
            HActPatches.Init();
            HActDamage.Init();
            HumanModePatches.Init();
            InventoryPatches.Init();
            InventoryPatches.Activate(); //need this to be always ready
            CFCPatches.Init();
            ParticlePatches.Init();
            PausePatches.Init();
            BattleTurnManagerPatches.Init();
            UIPatches.Init();
            CameraPatches.Init();
            AuthPatches.Init();

            //DRAGON ENGINE: Allow us to set speed for "unprocessed"
            DragonEngineLibrary.Unsafe.CPP.NopMemory(DragonEngineLibrary.Unsafe.CPP.PatternSearch("74 ? 0F B6 C1 48 8D 0D ? ? ? ? C5 FA 11 0C 81"), 2);
        }

        public static void Enable()
        {
            if (Enabled)
                return;

            Enabled = true;

            CombatPatches.Activate();
            CombatPlayerPatches.Activate();
            HActPatches.Activate();
            HActDamage.Activate();
            SupporterPatches.Activate();
            CombatPlayerPatches.Activate();
            HumanModePatches.Activate();
            InventoryPatches.Activate();
            CFCPatches.Activate();
            ParticlePatches.Activate();
            PausePatches.Activate();
            BattleTurnManagerPatches.Activate();
            CameraPatches.Activate();
            AuthPatches.Activate();
            

            if (BrawlerBattleManager.PlayerFighter.IsValid())
                CombatPatches.DisableAssignment();
        }

        public static void Disable()
        {
            if (!Enabled)
                return;

            Enabled = false;

            CombatPatches.Deactivate();
            CombatPlayerPatches.Deactivate();
            HActPatches.Deactivate();
            HActDamage.Deactivate();
            SupporterPatches.Deactivate();
            CombatPlayerPatches.Deactivate();
            HumanModePatches.Deactivate();
            ParticlePatches.Deactivate();
            PausePatches.Deactivate();
            CameraPatches.Deactivate();
            AuthPatches.Deactivate();

            CombatPatches.EnableAssignment();
        }
    }
}
