using System;
using MinHook;
using DragonEngineLibrary;
using System.Runtime.InteropServices;

namespace LikeABrawler2
{
    internal unsafe class CFCPatches : BrawlerPatch
    {
        private IntPtr m_LeverConditionFunc;

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool ItemBuffCheck(IntPtr handler, IntPtr checkFighterResult, IntPtr fighter);

        public override void Init()
        {
            base.Init();

            m_LeverConditionFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("40 53 48 83 EC ? 49 8B C8 48 8B DA E8 ? ? ? ? 84 C0");
        }

        protected override void SetActive()
        {
            base.SetActive();

            m_leverTrampoline = BrawlerPatches.HookEngine.CreateHook<ItemBuffCheck>(m_LeverConditionFunc, Check_AnalogDeadzone);
            BrawlerPatches.HookEngine.EnableHook(m_leverTrampoline);
        }

        private static ItemBuffCheck m_leverTrampoline;
        private static bool Check_AnalogDeadzone(IntPtr handler, IntPtr checkFighterResult, IntPtr fighter)
        {
            return ProcessSpecialCond(handler, checkFighterResult, fighter);
        }


        private unsafe static bool ProcessSpecialCond(IntPtr handler, IntPtr checkFighterResult, IntPtr fighter)
        {
            byte* paramsPtr = *(byte**)(checkFighterResult.ToInt64() + 0x8);
            byte condType = *paramsPtr;
            byte op = *(paramsPtr + 3);

            switch(condType)
            {
                case 1:
                    return CheckPlayerLevelCond(fighter, op, paramsPtr);
                case 2:
                    return CheckNearestPickableAssetType(fighter, op, paramsPtr);
                case 3:
                    return CheckFacingNearestPickableAsset(fighter, op, paramsPtr);
                case 4:
                    return CheckKiryuStyle(fighter, op, paramsPtr);
                case 5:
                    return CheckIsIchiban(fighter, op, paramsPtr);
                case 6:
                    return CheckExtremeHeat(fighter, op, paramsPtr);
                case 7:
                    return CheckPlayerJob(fighter, op, paramsPtr);
                case 8:
                    return CheckAIParam(fighter, op, paramsPtr);
                case 9:
                    return CheckIsRealtime(fighter, op, paramsPtr);
                case 10:
                    return CheckIsActiveBrawlerPlayer(fighter, op, paramsPtr);
                case 11:
                    return CheckWallJump(fighter, op, paramsPtr);
            }

            return false;
        }

        private unsafe static bool CheckPlayerLevelCond(IntPtr fighter, byte op, byte* paramsPtr)
        {
            byte level = *(paramsPtr + 1);
            uint fLevel = Player.GetLevel(BrawlerPlayer.CurrentPlayer);

            switch (op)
            {
                default:
                    return true;
                case 0:
                    return fLevel == level;
                case 1:
                    return fLevel >= level;
                case 2:
                    return fLevel <= level;
            }
        }
        private static bool CheckNearestPickableAssetType(IntPtr fighter, byte op, byte* paramsPtr)
        {
            byte category = *(paramsPtr + 1);

            var unit = AssetManager.FindNearestAssetFromAll(DragonEngine.GetHumanPlayer().GetPosCenter(), 2);

            if (!unit.IsValid())
                return false;

            switch (op)
            {
                default:
                    return true;
                case 0:
                    return Asset.GetArmsCategory(unit.Get().AssetID) == (AssetArmsCategoryID)category;
                case 3:
                    return Asset.GetArmsCategory(unit.Get().AssetID) != (AssetArmsCategoryID)category;
            }
        }

        private static bool CheckFacingNearestPickableAsset(IntPtr fighter, byte op, byte* paramsPtr)
        {
            byte category = *(paramsPtr + 1);

            var unit = AssetManager.FindNearestAssetFromAll(DragonEngine.GetHumanPlayer().GetPosCenter(), 2);

            if (!unit.IsValid())
                return false;

            Fighter fighterChara = new Fighter(fighter);

            switch (op)
            {
                default:
                    return true;
                case 0:
                    return fighterChara.Character.IsFacingEntity(unit.Get());
                case 3:
                    return !fighterChara.Character.IsFacingEntity(unit.Get());
            }
        }

        private static bool CheckIsIchiban(IntPtr fighter, byte op, byte* paramsPtr)
        {
            byte category = *(paramsPtr + 1);

            switch (op)
            {
                default:
                    return true;
                case 0:
                    return BrawlerPlayer.IsKasuga();
                case 3:
                    return !BrawlerPlayer.IsKasuga();
            }
        }

        private static bool CheckKiryuStyle(IntPtr fighter, byte op, byte* paramsPtr)
        {
            if (!BrawlerPlayer.IsKiryu())
                return false;

            byte style = *(paramsPtr + 1);

            switch (op)
            {
                default:
                    return true;
                case 0:
                    return (byte)BrawlerPlayer.CurrentStyle == style;
                case 3:
                    return (byte)BrawlerPlayer.CurrentStyle != style;
            }
        }

        private static bool CheckExtremeHeat(IntPtr fighter, byte op, byte* paramsPtr)
        {
            switch (op)
            {
                default:
                    return true;
                case 0:
                    return BrawlerPlayer.IsExtremeHeat;
                case 3:
                    return !BrawlerPlayer.IsExtremeHeat;
            }
        }

        private static bool CheckPlayerJob(IntPtr fighter, byte op, byte* paramsPtr)
        {
            byte job = *(paramsPtr + 1);

            switch (op)
            {
                default:
                    return true;
                case 0:
                    return job == (byte)Player.GetCurrentJob(BrawlerPlayer.CurrentPlayer);
                case 3:
                    return job != (byte)Player.GetCurrentJob(BrawlerPlayer.CurrentPlayer);
            }
        }

        private static bool CheckAIParam(IntPtr fighterPtr, byte op, byte* paramsPtr)
        {
            Fighter fighter = new Fighter(fighterPtr);
            BaseAI ai = fighter.TryGetAI();

            if (ai == null)
                return false;

            BaseAIParams param = (BaseAIParams)(*(paramsPtr + 1));

            switch (op)
            {
                default:
                    return true;

                case 0:
                    return ai.CheckParam(param);
                case 3:
                    return !ai.CheckParam(param);
            }      
        }

        private static bool CheckIsRealtime(IntPtr fighterPtr, byte op, byte* paramsPtr)
        {
            switch (op)
            {
                default:
                    return true;

                case 0:
                    return Mod.IsRealtime();
                case 3:
                    return !Mod.IsRealtime();
            }
        }

        private static bool CheckIsActiveBrawlerPlayer(IntPtr fighterPtr, byte op, byte* paramsPtr)
        {
            if(!CheckIsRealtime(fighterPtr, 0, paramsPtr)) 
                return false;

            Fighter fighter = new Fighter(fighterPtr);

            switch (op)
            {
                default:
                    return true;

                case 0:
                    return fighter.IsMainPlayer();
                case 3:
                    return !fighter.IsMainPlayer();
            }
        }

        private static bool CheckWallJump(IntPtr fighterPtr, byte op, byte* paramsPtr)
        {
            Fighter fighter = new Fighter(fighterPtr);

            HActRangeInfo inf = new HActRangeInfo();

            if (!fighter.GetStatus().HAct.GetPlayInfo(ref inf, HActRangeType.hit_wall))
                return false;

            bool valid =fighter.GetBrawlerInfo().MoveTime >= 1f && !CombatPlayerPatches.HumanModeManager_IsInputKamae(fighter.Character.HumanModeManager.Pointer) &&  Vector3.Distance(fighter.Character.Transform.Position, (Vector3)inf.Pos) <= 1f && fighter.Character.IsFacingPosition((Vector3)inf.Pos);       

            switch (op)
            {
                default:
                    return true;

                case 0:
                    return valid;
                case 3:
                    return !valid;
            }
        }
    }
}
