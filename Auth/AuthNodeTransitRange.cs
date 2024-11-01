using System;
using System.Runtime.InteropServices;
using DragonEngineLibrary;
using ElvisCommand;

namespace LikeABrawler2
{
    internal static class AuthNodeTransitRange
    {
        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, IntPtr parent)
        {
            IntPtr dataPtr = (IntPtr)(thisObj.ToInt64() + 48);
            int rangeType = Marshal.ReadInt32(dataPtr);

            if (rangeType <= 0)
                return;

            Vector4 offsetXYZ = Marshal.PtrToStructure<Vector4>(dataPtr + 4);
            HActRangeInfo cfcRangeAtkInf = new HActRangeInfo();

            if (BrawlerBattleManager.PlayerFighter.GetStatus().HAct.GetPlayInfo(ref cfcRangeAtkInf, (HActRangeType)rangeType))
            {
                Vector3 finalPos = (Vector3)cfcRangeAtkInf.Pos;
                finalPos += (cfcRangeAtkInf.Rot * Vector3.forward) * offsetXYZ.z;
                finalPos += (cfcRangeAtkInf.Rot * Vector3.up) * offsetXYZ.y;
                finalPos += (cfcRangeAtkInf.Rot * -Vector3.right) * offsetXYZ.x;

                BrawlerBattleManager.PlayerCharacter.WarpPosAndOrient(BrawlerBattleManager.PlayerCharacter.Transform.Position, cfcRangeAtkInf.Rot);
                BrawlerBattleManager.PlayerCharacter.RequestWarpPose(new PoseInfo(finalPos, BrawlerBattleManager.PlayerCharacter.GetAngleY()));
            }

            DragonEngine.Log("Transit range");
        }
    }
}
