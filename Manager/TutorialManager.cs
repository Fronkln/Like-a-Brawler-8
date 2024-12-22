using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal static class TutorialManager
    {
        public static bool Active { get; private set; }
        private static UIHandleBase m_instructionsRoot;

        private static List<TutorialGoal> m_currentGoals;
        private static int m_goalIdx = 0;
        private static bool m_startNextGoal = false;
        private static float m_goalTime = 0;

        public static TutorialGoal CurrentGoal { get { return m_currentGoals[m_goalIdx]; } }

        public static void Init()
        {
            BrawlerBattleManager.OnBattleStartEvent += OnBattleStart;
            BrawlerBattleManager.OnBattleEndEvent += OnBattleEnd;
            BrawlerBattleManager.OnActionStartEvent += OnActionStart;
        }

        private static void Reset()
        {
            Active = false;
            m_instructionsRoot.SetVisible(false);
            m_instructionsRoot.PlayAnimationSet(248);
            m_startNextGoal = false;
            m_goalTime = 0;
        }

        public static void OnBattleStart()
        {
            if (Mod.IsTurnBased())
                return;

            Reset();
        }
        
        public static void OnBattleEnd()
        {
            Reset();
        }

        public static void OnActionStart()
        {
            if (Mod.IsTurnBased())
                return;

            CheckTutorialBattle();
        }

        public static void Update()
        {
            if(!BrawlerBattleManager.Battling || Mod.IsGamePaused)
                return;

            if (!Active)
                return;

            if(BrawlerBattleManager.CurrentPhase >= BattleTurnManager.TurnPhase.Cleanup)
            {
                Active = false;
                m_instructionsRoot.PlayAnimationSet(248);

                return;
            }

            if(m_startNextGoal)
            {
                if (BrawlerBattleManager.CurrentPhase != BattleTurnManager.TurnPhase.Action || BrawlerBattleManager.CurrentPhaseTime < 0.1f)
                    return;

                TutorialGoal nextGoal = m_currentGoals[m_goalIdx];

                if (nextGoal.TalkID > 0)
                    if (HeatActionManager.IsHAct() || HeatActionManager.AwaitingHAct)
                        return;

                StartGoal(nextGoal);
                m_startNextGoal = false;
            }


            if (m_startNextGoal)
                return;

            TutorialGoal currentGoal = m_currentGoals[m_goalIdx];

            m_goalTime += DragonEngine.deltaTime;

            if(currentGoal.Modifier.HasFlag(TutorialModifier.DontAllowPlayerDamage))
                BrawlerBattleManager.PlayerFighter.GetStatus().SetHPCurrent(Player.GetHPMax(BrawlerPlayer.CurrentPlayer));

           if(currentGoal.Modifier.HasFlag(TutorialModifier.DontAllowEnemyDamage))
                foreach(Fighter enemy in BrawlerBattleManager.AllEnemies)
                {
                    ECBattleStatus stat = enemy.GetStatus();
                    stat.SetHPCurrent(stat.MaxHP);
                }

            if(currentGoal.Modifier.HasFlag(TutorialModifier.FullHeat))
                Player.SetHeatNow(BrawlerPlayer.CurrentPlayer, Player.GetHeatMax(BrawlerPlayer.CurrentPlayer));

            if(currentGoal.Modifier.HasFlag(TutorialModifier.NoHeat))
                Player.SetHeatNow(BrawlerPlayer.CurrentPlayer, 0);

            if (currentGoal.CheckDelegate?.Invoke() == true)
            {
                OnGoalFinish(true);
                return;
            }
            else if (currentGoal.TimeToComplete > 0 && m_goalTime >= currentGoal.TimeToComplete)
                OnGoalFinish(false);

        }

        private static void OnGoalFinish(bool success)
        {
            if (success && CurrentGoal.TimeToComplete > 0)
                SoundManager.PlayCue(1627, 31, 0);

            //close, play_success doesnt seem to work
            //close will also destroy the UI element when its done playing.
            m_instructionsRoot.PlayAnimationSet(248); 
            m_goalIdx++;

            if (m_goalIdx >= m_currentGoals.Count)
            {
                Active = false;
                return;
            }
            else
                StartGoal(m_currentGoals[m_goalIdx]);
        }

        private static void CheckTutorialBattle()
        {
            uint tutEnemy = DBManager.GetSoldier("elvis_btl01_0100_000_1");
            foreach(Fighter fighter in FighterManager.GetAllEnemies())
                if((uint)fighter.Character.Attributes.soldier_data_id == tutEnemy)
                {
                    StartTutorial(Tutorial01_Kasuga());
                    return;
                }

            if (BattleTurnManager.BattleConfigID == 249)
                StartTutorial(Tutorial02_Kasuga());
            if (BattleTurnManager.BattleConfigID == 4)
                StartTutorial(Tutorial03_Kasuga());
            if (BattleTurnManager.BattleConfigID == 6)
                StartTutorial(Tutorial04_Kasuga());
            if (BattleTurnManager.BattleConfigID == 44)
                StartTutorial(Tutorial01_Kiryu());
            if(BattleTurnManager.BattleConfigID == 248)
                StartTutorial(Tutorial02_Kiryu());
        }


        public static void StartTutorial(List<TutorialGoal> tutorials)
        {
            m_goalIdx = 0;
            m_currentGoals = tutorials;

            //StartGoal(tutorials[0]);
            m_startNextGoal = true;

            Active = true;
        }

        private static void StartGoal(TutorialGoal goal)
        {
            m_goalTime = 0;

            if(goal.TalkID != 0)
            {
                //HAct goal
                HActRequestOptions opts = new HActRequestOptions();
                opts.id = goal.TalkID;
                opts.is_force_play = true;
                opts.base_mtx.matrix = BrawlerBattleManager.PlayerCharacter.GetMatrix();
                HeatActionManager.RequestTalk(opts);
            }
            else
            {
                //Instructions goal
                if (!string.IsNullOrEmpty(goal.Instructions))
                {
                    m_instructionsRoot = UI.Play(55, 0);
                    m_instructionsRoot.GetChild(0).SetText(goal.Instructions);
                }
            }

            goal.OnStart?.Invoke();
        }

        private static List<TutorialGoal> Tutorial01_Kasuga()
        {
            bool m_rushAttackOnce = false;
            bool m_finisherAttackOnce = false;
            float m_stanceTime = 0;
            float m_guardTime = 0;

            List<TutorialGoal> goals = new List<TutorialGoal>();

            TutorialGoal hactGoalTest = new TutorialGoal();
            hactGoalTest.SetTalkID(DBManager.GetTalkParam("y8b_tutorial_btl01_0100_01"));
            hactGoalTest.CheckDelegate = delegate { return !HeatActionManager.AwaitingHAct && !HeatActionManager.IsHAct() && m_goalTime > 0.1f; };

            TutorialGoal rushCombo = new TutorialGoal();
            rushCombo.SetInstructions(
                "<color=batting_pitch_light_blue>Rush Combo</color>" +
                "\n<symbol=button_shikaku>" +
                "\n<symbol=button_shikaku> <symbol=button_shikaku>" +
                "\n<symbol=button_shikaku> <symbol=button_shikaku> <symbol=button_shikaku>");
            rushCombo.CheckDelegate = delegate 
            {
                string command = BrawlerBattleManager.PlayerCharacter.HumanModeManager.GetCommandName().ToLowerInvariant();

                if (!m_rushAttackOnce)
                {
                    if (command.StartsWith("light"))
                        m_rushAttackOnce = true;

                    return false;
                }
                else if (!BrawlerBattleManager.PlayerCharacter.HumanModeManager.IsAttack())
                    return true;

                return false;
            };


            rushCombo.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.NoHeat;

            TutorialGoal finisherCombo = new TutorialGoal();
            finisherCombo.SetInstructions(
                 "<color=batting_pitch_light_blue>Finisher</color>" +
                 "\n<symbol=button_sankaku> during a Rush Combo"
                );
            finisherCombo.CheckDelegate = delegate
            {
                string command = BrawlerBattleManager.PlayerCharacter.HumanModeManager.GetCommandName().ToLowerInvariant();
                return command.StartsWith("attackfinish");
            };

            finisherCombo.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.NoHeat;


            TutorialGoal grab = new TutorialGoal();
            grab.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.NoHeat;
            grab.SetInstructions(
                 "<color=batting_pitch_light_blue>Grabbing</color>" +
                 "\n<symbol=button_maru>"
                );
            grab.CheckDelegate = delegate { return BrawlerFighterInfo.Infos[BrawlerBattleManager.PlayerCharacter.UID].IsSync; };


            TutorialGoal battleStance = new TutorialGoal();
            battleStance.SetInstructions(
                 "<color=batting_pitch_light_blue>Battle Stance</color>" +
                 "\nHold <symbol=button_r1>"
                );
            battleStance.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.NoHeat;
            battleStance.CheckDelegate = delegate
            {
                if (CombatPlayerPatches.HumanModeManager_IsInputKamae(BrawlerBattleManager.PlayerCharacter.HumanModeManager.Pointer))
                    m_stanceTime += DragonEngine.deltaTime;

                if (m_stanceTime >= 3)
                    return true;

                return false;
            };


            TutorialGoal guard = new TutorialGoal();
            guard.SetInstructions(
                 "<color=batting_pitch_light_blue>Guarding</color>" +
                 "\nPress or Hold <symbol=button_l1>"
                );

            guard.CheckDelegate = delegate
            {
                if (BrawlerBattleManager.PlayerCharacter.HumanModeManager.IsGuarding())
                    m_guardTime += DragonEngine.deltaTime;

                if (m_guardTime >= 3)
                    return true;

                return false;
            };

            guard.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.NoHeat;

            TutorialGoal pguard = new TutorialGoal();
            pguard.SetTalkID(DBManager.GetTalkParam("y8b_tutorial_btl01_0100_02"));
            pguard.CheckDelegate = delegate { return !HeatActionManager.AwaitingHAct && !HeatActionManager.IsHAct() && m_goalTime > 0.1f; };


            int numPerfectGuard = 0;

            TutorialGoal pguardInstruction = new TutorialGoal();
            pguardInstruction.OnStart += delegate { BrawlerPlayer.OnPerfectGuard += delegate { numPerfectGuard++; }; };
            pguardInstruction.SetInstructions("Perform <color=sanpo>Perfect Guard</color> three times.");
            pguardInstruction.CheckDelegate = delegate { return numPerfectGuard == 3; };
            pguardInstruction.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.NoHeat;

            TutorialGoal fin = new TutorialGoal();
            fin.SetTalkID(DBManager.GetTalkParam("y8b_tutorial_btl01_0100_03"));
            fin.CheckDelegate = delegate { return !HeatActionManager.AwaitingHAct && !HeatActionManager.IsHAct() && m_goalTime > 0.1f; };


            goals.Add(hactGoalTest);
            goals.Add(rushCombo);
            goals.Add(finisherCombo);
            goals.Add(grab);
            goals.Add(battleStance);
            goals.Add(guard);
            goals.Add(pguard);
            goals.Add(pguardInstruction);
            goals.Add(fin);

            return goals;
        }
        private static List<TutorialGoal> Tutorial02_Kasuga()
        {
            List<TutorialGoal> goals = new List<TutorialGoal>();

            TutorialGoal wepPickup = new TutorialGoal();
            wepPickup.SetInstructions(
                "<color=batting_pitch_light_blue>Picking Up Weapons</color>" +
                "\n<symbol=button_maru> facing a nearby object");
            wepPickup.CheckDelegate = delegate
            {
                return BrawlerFighterInfo.Infos[BrawlerBattleManager.PlayerCharacter.UID].RightWeapon.IsValid();
            };
            wepPickup.TimeToComplete = 12f;
            wepPickup.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.NoHeat;


            TutorialGoal hact = new TutorialGoal();
            hact.SetInstructions(
                "<color=batting_pitch_light_blue>Heat Actions</color>" +
                "\n<symbol=button_sankaku> nearby a downed enemy or while holding a weapon");
            hact.CheckDelegate = delegate
            {
                return BrawlerBattleManager.IsHAct;
            };
            hact.TimeToComplete = 15f;
            hact.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.FullHeat;

            TutorialGoal disableHactCenterCam = new TutorialGoal();
            disableHactCenterCam.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.NoHeat;
            disableHactCenterCam.SetInstructions(
                 "<color=batting_pitch_light_blue>Prevent Heat Action/Center Camera</color>" +
                 "\nHold down <symbol=button_l2> to disable heat actions, press to re-center camera"
                );

            float disableHactTime = 0;

            disableHactCenterCam.CheckDelegate = delegate
            {
                if (BrawlerPlayer.IsInputDisableHAct())
                    disableHactTime += DragonEngine.deltaTime;

                if (disableHactTime >= 2)
                    return true;

                return false;
            };


            goals.Add(wepPickup);
            goals.Add(hact);
            goals.Add(disableHactCenterCam);

            return goals;
        }

        //Fighting as a team
        private static List<TutorialGoal> Tutorial03_Kasuga()
        {
            List<TutorialGoal> goals = new List<TutorialGoal>();

            TutorialGoal extremeHeatTut = new TutorialGoal();
            extremeHeatTut.TalkID = DBManager.GetTalkParam("y8b_tutorial_btl01_0300_01");
            extremeHeatTut.CheckDelegate = delegate { return !HeatActionManager.AwaitingHAct && !HeatActionManager.IsHAct() && m_goalTime > 0.1f; };

            goals.Add(extremeHeatTut);

            return goals;
        }


        //Extreme heat
        private static List<TutorialGoal> Tutorial04_Kasuga()
        {
            List<TutorialGoal> goals = new List<TutorialGoal>();

            TutorialGoal extremeHeatTut = new TutorialGoal();
            extremeHeatTut.TalkID = DBManager.GetTalkParam("y8b_tutorial_btl01_0500_01");
            extremeHeatTut.CheckDelegate = delegate { return !HeatActionManager.AwaitingHAct && !HeatActionManager.IsHAct() && m_goalTime > 0.1f; };
            TutorialGoal extremeHeat = new TutorialGoal();
            extremeHeat.SetInstructions(
                "<color=batting_pitch_light_blue>Extreme Heat Mode</color>" +
                "\n<symbol=button_r2> to enter Extreme Heat Mode");
            extremeHeat.CheckDelegate = delegate
            {
                return BrawlerPlayer.IsExtremeHeat;
            };
            extremeHeat.TimeToComplete = -1;
            extremeHeat.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.FullHeat;

            goals.Add(extremeHeatTut);
            goals.Add(extremeHeat);

            return goals;
        }

        //Poundmates
        public static List<TutorialGoal> Tutorial05_Kasuga()
        {
            List<TutorialGoal> goals = new List<TutorialGoal>();

            TutorialGoal poundmate = new TutorialGoal();
            poundmate.SetInstructions(
                "<color=batting_pitch_light_blue>Poundmates</color>" +
                "\nHold <symbol=button_l1> + <symbol=button_shikaku> + <symbol=button_sankaku> to activate.");
            poundmate.CheckDelegate = delegate
            {
                return BrawlerBattleManager.IsDeliveryHelping;
            };
            poundmate.TimeToComplete = -1;
            poundmate.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.FullHeat;

            goals.Add(poundmate);

            return goals;
        }

        //Kiryu introduction
        private static List<TutorialGoal> Tutorial01_Kiryu()
        {
            List<TutorialGoal> goals = new List<TutorialGoal>();

            TutorialGoal rushTutorial = new TutorialGoal();
            rushTutorial.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage;
            rushTutorial.SetInstructions(
                 "<color=batting_pitch_light_blue>Style Change</color>" +
                 "\n<symbol=button_left> Rush Style"
                );

            rushTutorial.CheckDelegate = delegate
            {
                return BrawlerPlayer.CurrentStyle == PlayerStyle.Rush;
            };


            TutorialGoal rushWeave = new TutorialGoal();
            rushWeave.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage;
            rushWeave.SetInstructions(
                 "<color=batting_pitch_light_blue>Weaving (Rush Style)</color>" +
                 "\n<symbol=button_l1> right before an attack lands"
                );

            rushWeave.CheckDelegate = delegate
            {
                return BrawlerPlayer.CurrentStyle == PlayerStyle.Rush && BrawlerBattleManager.PlayerCharacter.HumanModeManager.IsGuarding();
            };

            TutorialGoal beastTutorial = new TutorialGoal();
            beastTutorial.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage;
            beastTutorial.SetInstructions(
                 "<color=batting_pitch_light_blue>Style Change</color>" +
                 "\n<symbol=button_right> Beast Style"
                );

            beastTutorial.CheckDelegate = delegate
            {
                return BrawlerPlayer.CurrentStyle == PlayerStyle.Beast;
            };

            TutorialGoal legendTutorial = new TutorialGoal();
            legendTutorial.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage;
            legendTutorial.SetInstructions(
                 "<color=batting_pitch_light_blue>Style Change</color>" +
                 "\n<symbol=button_up> Legend Style"
                );

            legendTutorial.CheckDelegate = delegate
            {
                return BrawlerPlayer.CurrentStyle == PlayerStyle.Default;
            };


            goals.Add(rushTutorial);
            goals.Add(rushWeave);
            goals.Add(beastTutorial);
            goals.Add(legendTutorial);

            return goals;
        }

        //Dragon's Resurgence
        private static List<TutorialGoal> Tutorial02_Kiryu()
        {
            List<TutorialGoal> goals = new List<TutorialGoal>();

            TutorialGoal extremeHeatTutWait = new TutorialGoal();
            extremeHeatTutWait.CheckDelegate = delegate { return BrawlerBattleManager.BattleTime > 1f; };
            extremeHeatTutWait.TimeToComplete = -1;
            TutorialGoal extremeHeat = new TutorialGoal();
            extremeHeat.SetInstructions(
                "<color=batting_pitch_light_blue>1988 Dragon of Dojima Style</color>" +
                "\n<symbol=button_r2> to activate");
            extremeHeat.CheckDelegate = delegate
            {
                return BrawlerPlayer.IsExtremeHeat;
            };
            extremeHeat.TimeToComplete = -1;
            extremeHeat.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.FullHeat;

            TutorialGoal swayAttack = new TutorialGoal();
            swayAttack.SetInstructions(
                "<color=batting_pitch_light_blue>Komaki Evade & Strike</color>" +
                "\n<symbol=button_sankaku> while dodging");
            swayAttack.CheckDelegate = delegate { return BrawlerBattleManager.PlayerCharacter.HumanModeManager.GetCommandName().StartsWith("SwayAttack"); };
            swayAttack.TimeToComplete = 30;
            swayAttack.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.FullHeat | TutorialModifier.DontAllowStyleChange;

            TutorialGoal finishingHold = new TutorialGoal();
            finishingHold.SetInstructions(
                "<color=batting_pitch_light_blue>Finishing Hold</color>" +
                "\n<symbol=button_maru> during a finisher");
            finishingHold.CheckDelegate = delegate { return BrawlerBattleManager.PlayerCharacter.HumanModeManager.GetCommandName().StartsWith("FinishingHold") || m_goalTime >= 30; };
            finishingHold.TimeToComplete = 30;
            finishingHold.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.FullHeat | TutorialModifier.DontAllowStyleChange;

            TutorialGoal parry = new TutorialGoal();
            parry.SetInstructions(
                "<color=batting_pitch_light_blue>Komaki Parry</color>" +
                "\n<symbol=button_maru> before enemy attack lands");
            parry.CheckDelegate = delegate { return BrawlerBattleManager.PlayerCharacter.HumanModeManager.GetCommandName() == "CounterParry" || m_goalTime >= 30; };
            parry.TimeToComplete =30;
            parry.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.FullHeat | TutorialModifier.DontAllowStyleChange;

            TutorialGoal tigerDrop = new TutorialGoal();
            tigerDrop.SetInstructions(
                "<color=batting_pitch_light_blue>Komaki Tiger Drop</color>" +
                "\n<symbol=button_r1> + <symbol=button_sankaku> before enemy attack lands");
            tigerDrop.CheckDelegate = delegate { return BrawlerBattleManager.PlayerCharacter.HumanModeManager.GetCommandName() == "Counter" || m_goalTime >= 30; };
            tigerDrop.TimeToComplete = 30;
            tigerDrop.Modifier = TutorialModifier.DontAllowPlayerDamage | TutorialModifier.DontAllowEnemyDamage | TutorialModifier.FullHeat | TutorialModifier.DontAllowStyleChange;

            TutorialGoal extremeHeatBeatdown = new TutorialGoal();
            extremeHeatBeatdown.TimeToComplete = -1;
            extremeHeatBeatdown.Modifier = TutorialModifier.FullHeat | TutorialModifier.DontAllowStyleChange;

            goals.Add(extremeHeatTutWait);
            goals.Add(extremeHeat);
            goals.Add(swayAttack);
            goals.Add(finishingHold);
            goals.Add(parry);
            goals.Add(tigerDrop);
            goals.Add(extremeHeatBeatdown);

            return goals;
        }
    }
}
