using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using DragonEngineLibrary;
using ElvisCommand;

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

        public static bool IsTurnBased()
        {
            return Gamemode == 0;
        }

        public static bool IsRealtime()
        {
            return Gamemode == 1;
        }


        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        public static Vector2 GetControlSize(IntPtr hWnd)
        {
            RECT pRect;
            Vector2 cSize;
            // get coordinates relative to window
            GetWindowRect(hWnd, out pRect);

            cSize.x = pRect.Right - pRect.Left;
            cSize.y = pRect.Bottom - pRect.Top;

            return cSize;
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

                IniSettings.Read();
                DragonEngine.Log("Like A Brawler Init Complete");
            }
            catch(Exception ex)
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

                Debug.InputUpdate();
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

#if DEBUG
            Debug.GameUpdate();
#endif
            DETaskManager.Update();
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
