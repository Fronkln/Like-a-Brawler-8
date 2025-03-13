using System;
using System.Runtime.InteropServices;
using DragonEngineLibrary;
using DragonEngineLibrary.Unsafe;

namespace LikeABrawler2
{
    internal unsafe static class NativeFuncs
    {
        private delegate long PointManagerGetPlayerPoint(IntPtr playerPointMan, PlayerPoint.ID id);
        public delegate int GetDifficulty();


        private static PointManagerGetPlayerPoint PlayerPointFunc;
        public static GetDifficulty BattleDifficultyFunc;



        internal static void Init()
        {
            PlayerPointFunc = Marshal.GetDelegateForFunctionPointer<PointManagerGetPlayerPoint>(CPP.PatternSearch("48 89 5C 24 08 57 48 83 EC ? 8B DA 48 8B F9 E8 ? ? ? ? 4C 8B C0"));
            BattleDifficultyFunc = Marshal.GetDelegateForFunctionPointer<GetDifficulty>(CPP.ReadCall(CPP.PatternSearch("E8 ? ? ? ? 8B C8 E8 ? ? ? ? 8B F8 B9 ? ? ? ?")));
        }

        public static long GetPlayerPoint(PlayerPoint.ID id)
        {
            EntityBase sceneEntity = DragonEngineLibrary.Service.SceneService.GetSceneInfo().ScenePlay.Get().GetSceneEntity((SceneEntity)181);

            if (sceneEntity.IsValid())
                return PlayerPointFunc(sceneEntity.Pointer, id);
            else
                return 0;
        }
    }
}
