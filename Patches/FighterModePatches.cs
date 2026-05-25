using DragonEngineLibrary;
using DragonEngineLibrary.Service;
using DragonEngineLibrary.Unsafe;
using System;
using System.Runtime.InteropServices;

namespace LikeABrawler2
{
    internal class FighterModePatches : BrawlerPatch
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private unsafe delegate int FighterModeSwayCalcSwayLimit(IntPtr fighterMode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private unsafe delegate uint FighterModeSwayGetMotionID(IntPtr fighterMode);


        private static IntPtr m_swayGetMotionIDFunc;
        private static IntPtr m_calcSwayLimitFunc;

        public override void Init()
        {
            base.Init();

            m_swayGetMotionIDFunc = CPP.PatternSearch("48 89 5C 24 ? 48 89 74 24 ? 48 89 7C 24 ? 4C 89 74 24 ? 55 48 89 E5 48 83 EC ? 48 89 CE 48 83 C1");
            m_calcSwayLimitFunc = CPP.PatternSearch("48 83 EC ? 48 83 C1 ? E8 ? ? ? ? 31 C0");
        }

        protected override void SetActive()
        {
            base.SetActive();

            if (_swayMotionIDTrampoline == null)
                _swayMotionIDTrampoline = BrawlerPatches.HookEngine.CreateHook<FighterModeSwayGetMotionID>(m_swayGetMotionIDFunc, FighterModeSway_GetMotionID);

            if(_calcSwayLimitTrampoline == null)
                _calcSwayLimitTrampoline = BrawlerPatches.HookEngine.CreateHook<FighterModeSwayCalcSwayLimit>(m_calcSwayLimitFunc, FighterModeSway_CalcSwayLimit);

            BrawlerPatches.HookEngine.EnableHook(_swayMotionIDTrampoline);
            BrawlerPatches.HookEngine.EnableHook(_calcSwayLimitTrampoline);
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            if (_swayMotionIDTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(_swayMotionIDTrampoline);

            if (_calcSwayLimitTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(_calcSwayLimitTrampoline);
        }

        private static FighterModeSwayCalcSwayLimit _calcSwayLimitTrampoline = null;
        //Returns maximum amount of sways a player can do
        private static unsafe int FighterModeSway_CalcSwayLimit(IntPtr fighterMode)
        {
            HumanMode hObj = new HumanMode() { Pointer = fighterMode };
            HumanModeManager hMan = hObj.Parent;
            Character human = hMan.Human;

            if (human.UID != Mod.MainPlayerCharacter.UID)
                return _calcSwayLimitTrampoline(fighterMode);

            if (BrawlerPlayer.CurrentStyle == PlayerStyle.Rush)
                return 3;

            return 2;
        }

        private static FighterModeSwayGetMotionID _swayMotionIDTrampoline = null;
        private static unsafe uint FighterModeSway_GetMotionID(IntPtr fighterMode)
        {
            HumanMode hObj = new HumanMode() { Pointer = fighterMode };
            HumanModeManager hMan = hObj.Parent;
            Character human = hMan.Human;

            /*
            var brawlerPlayer = Mod.GetPlayerByUID(human.UID);

            if (brawlerPlayer == null)
                return _swayMotionIDTrampoline(fighterMode);
            */

            int swayNum = *(int*)(fighterMode + 0xA4);

            if (swayNum <= 1)
                return _swayMotionIDTrampoline(fighterMode);

            byte swayDirection = *(byte*)(fighterMode + 0x9C);
            string commandName = "Sway";

            switch (swayDirection)
            {
                case 0:
                    commandName += "F";
                    break;
                case 1:
                    commandName += "L";
                    break;
                case 3:
                    commandName += "R";
                    break;
                case 2:
                    commandName += "B";
                    break;
            }

            //Example:SwayL2
            commandName += swayNum;

            uint id1 = human.HumanModeManager.CommandsetModel.GetCommandset(0);
            uint id2 = human.HumanModeManager.CommandsetModel.GetCommandset(1);
            uint commandset = id2 != 0 ? id2 : id1;

            FighterCommandSet set = FighterCommandManager.GetSet(commandset);
            short commandIndex = set.FindCommandInfo(commandName);

            if (commandIndex < 0)
                return _swayMotionIDTrampoline(fighterMode);

            FighterCommandID cfcID = new FighterCommandID((ushort)commandset, commandIndex);
            FighterCommandInfo commandInfo = cfcID.GetInfo();

            return commandInfo.GmtID;
        }
    }
}
