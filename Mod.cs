using DragonEngineLibrary;
using ElvisCommand;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

namespace LikeABrawler2
{
    public class Mod : DragonEngineMod
    {
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr h, string m, string c, int type);

        public static string ModPath;

        public static bool IsGameFocused = false;
        public static bool IsGamePaused = false;
        public static int Gamemode = 0; //0 = turn based, 1 = realtime

        public const float CriticalHPRatio = 0.3f;

        public static List<BrawlerPlayer> Players = new List<BrawlerPlayer>();
        public static event Action<EntityHandle<Character>, uint> OnPlayerCreatedEvent = null;

        public const int COOP_PLAYER_COUNT = 0;

        public static Character MainPlayerCharacter
        {
            get
            {
                if (Players.Count > 0)
                    return Players[0].Character;
                else
                    return new Character();
            }
        }

        public static Fighter MainPlayerFighter
        {
            get
            {
                if (Players.Count > 0)
                    return Players[0].Fighter;
                else
                    return FighterManager.GetFighter(0);
            }
        }

        public static BrawlerPlayer MainPlayer
        {
            get
            {
                if (Players.Count > 0)
                    return Players[0];
                else
                    return null;
            }
        }

        public static bool DoesPlayersExist()
        {
            return Players.Count > 0;
        }

        public static BrawlerPlayer GetPlayer(int idx)
        {
            return Players[idx];
        }

        //TODO: Make these linear time with UID dicts for better performance when u work on co-op
        public static BrawlerPlayer GetPlayerByUID(uint uid)
        {
            return Players.FirstOrDefault(x => x.Character.UID == uid);
        }

        public static BrawlerPlayer GetPlayerByPartyIndex(uint id)
        {
            return Players.FirstOrDefault(x => x.PartyMemberIndex == id);
        }

        public static void RegisterPlayer(EntityHandle<Character> player, uint nakamaIndex)
        {
            BrawlerPlayer playerInstance = new BrawlerPlayer();
            playerInstance.CharacterHandle = player;
            playerInstance.PlayerID = player.Get().Attributes.player_id;
            playerInstance.PartyMemberIndex = nakamaIndex;
            playerInstance.Update();
            playerInstance.OnSpawn();

            Players.Add(playerInstance);

            OnPlayerCreatedEvent?.Invoke(player, nakamaIndex);

            DragonEngine.Log("Added brawler player with nakama index: " + nakamaIndex);
        }

        public static bool IsTurnBased()
        {
            return Gamemode == 0;
        }

        public static bool IsRealtime()
        {
            return Gamemode == 1;
        }

        //14.11.2023, the journey begins.
        public override void OnModInit()
        {
            base.OnModInit();

            DragonEngine.Log("Like A Brawler Start");

            try
            {
                Assembly assmb = Assembly.GetExecutingAssembly();
                ModPath = Path.GetDirectoryName(assmb.Location);

                IniSettings.Read();

                DBManager.Init();

                AuthCustomNodeManager.Init();
                AuthConditionManager.Init();

                BrawlerPatches.Init();

                BrawlerBattleManager.Init();
                BrawlerUIManager.Init();
                TutorialManager.Init();
                HeatActionManager.Init();
                InputState.Init();
                WeaponManager.Init();
                RevelationManager.Init();
                TownsfolkManager.Init();
                HActLifeGaugeManager.Init();
                NativeFuncs.Init();

                new Thread(InputThread).Start();

                DragonEngine.RegisterJob(GamePreUpdate, DEJob.Update, true);
                DragonEngine.RegisterJob(GameUpdate, DEJob.Update);


                if (!File.Exists(IniSettings.IniPath()))
                    IniSettings.Write();

                DragonEngine.Log("Like A Brawler Init Complete");
            }
            catch (Exception ex)
            {
                DragonEngine.MessageBox(IntPtr.Zero, $"Error initializing Like A Brawler!\n{ex.ToString()}", "Like A Brawler Error", 0x00000010);
            }

        }


        private static void InputThread()
        {
            while (true)
            {
                if (!IsGameFocused)
                    continue;

                BrawlerBattleManager.InputUpdate();
#if DEBUG
                Debug.InputUpdate();
#endif
            }
        }

        public static bool IsDemo()
        {
#if DEMO
            return true;
#else
            return false;
#endif
        }

        private static void GamePreUpdate()
        {
            BrawlerBattleManager.PreUpdate();
        }
        private static void GameUpdate()
        {
            IsGameFocused = ApplicationIsActivated();
            IsGamePaused = GameVarManager.GetValueBool(GameVarID.is_pause);

            for (uint i = 0; i <= COOP_PLAYER_COUNT; i++)
            {
                EntityHandle<Character> partyMember = NakamaManager.GetCharacterHandle(i);

                if (partyMember.IsValid())
                {
                    if (GetPlayerByPartyIndex(i) == null)
                        RegisterPlayer(partyMember, i);
                }
            }

#if DEBUG
            Debug.GameUpdate();
#endif
            DETaskManager.Update();

            Players = Players.Where(x => x.CharacterHandle.IsValid()).ToList();

            foreach (var brawlerPlayer in Players)
                brawlerPlayer.Update();

            BrawlerBattleManager.Update();
        }

        public static YFC ReadYFC(string name)
        {
            return YazawaCommandManager.LoadYFC(name);
        }

        public static EHC ReadYHC(string name)
        {
            return YazawaCommandManager.LoadYHC(name);
        }

        public static void ReloadContent()
        {
            DragonEngine.Log("Reloading content");

            BrawlerPlayer.LoadContent();
            HeatActionManager.LoadContent();
            WeaponManager.Init();

            EnemyManager.ReloadContent();
            SupporterManager.ReloadContent();
        }



        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        //https://stackoverflow.com/a/7162873/14569631
        private static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();

            if (activatedHandle == IntPtr.Zero)
                return false;       // No window is currently activated

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        public static IntPtr FindPatternAssert(string pattern)
        {
            IntPtr func = DragonEngineLibrary.Unsafe.CPP.PatternSearch(pattern);
            System.Diagnostics.Debug.Assert(func != IntPtr.Zero, "A pattern could not be found.");

            return func;
        }
    }
}
