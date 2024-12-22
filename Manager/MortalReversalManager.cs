using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    public static class MortalReversalManager
    {
        public static bool Transit = false;
        public static bool CounterFlag = false; //for candamsync
        public static Fighter Attacker;


        public static bool Procedure { get; private set; } = false;

        private static FighterCommandInfo m_mortalCommand;

        public static void Update()
        {
            if(Transit)
            {
                CounterFlag = true;

                FighterCommandID command = BrawlerBattleManager.PlayerCharacter.HumanModeManager.CurrentMode.GetCommandID();
                string targetSkillName = DBManager.GetCommandSet(command.set_) + "_mortal_counter";
                RPGSkillID mrSkill = DBManager.GetSkill(targetSkillName);

                if (mrSkill <= 0)
                    mrSkill = DBManager.GetSkill("mortal_counter_test");

                BattleTurnManager.ForceCounterCommand(BrawlerBattleManager.PlayerFighter, Attacker, mrSkill);

               // BattleTurnManager.ForceCounterCommand(BrawlerBattleManager.PlayerFighter, Attacker, DBManager.GetSkill("mortal_counter_test"));
                EffectEventManager.PlayScreen(3); //just to add a tiny bit of flair
                DragonEngine.Log("GET. REVERSED!");
                Transit = false;

                SoundManager.PlayCue((SoundCuesheetID)7, 4, 0);

                new DETaskTime(0.3f, delegate
                {
                    FighterCommandID commandId = BrawlerBattleManager.PlayerCharacter.HumanModeManager.CurrentMode.GetCommandID();
                    FighterCommandInfo commandInf = commandId.GetInfo();

                    if (commandInf.Id == "MortalCounter")
                    {
                        m_mortalCommand = commandInf;
                        Procedure = true;
                    }

                    CounterFlag = false;
                });
            }

            if (Procedure)
            {
                if(BrawlerBattleManager.IsHActOrWaiting)
                {
                    Procedure = false;
                    return;
                }    

                if(!Attacker.IsValid() || Attacker.IsDead())
                {
                    Procedure = false;
                    return;
                }

                DragonEngine.SetSpeed(DESpeedType.General, 0.1f);
                DragonEngine.SetSpeed(DESpeedType.Character, 0.1f);
                DragonEngine.SetSpeed(DESpeedType.Player, 0.1f);

                FighterCommandID commandId = BrawlerBattleManager.PlayerCharacter.HumanModeManager.CurrentMode.GetCommandID();
                FighterCommandInfo commandInf = commandId.GetInfo();

                if (commandInf.Id != m_mortalCommand.Id)
                    Procedure = false;
            }
        }
    }
}
