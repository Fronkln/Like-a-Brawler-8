using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal static class Extensions
    {
        public static bool IsFacingPosition(this EntityBase ent, Vector3 position, float dotOverride = -10)
        {
            float dotVal = (dotOverride > -10 ? dotOverride : 0.35f);
            float dot = Vector3.Dot(ent.Transform.forwardDirection, (position - ent.Transform.Position).normalized);

            return dot >= dotVal;
        }

        public static bool IsFacingEntity(this EntityBase ent, EntityBase other, float dotOverride = -10)
        {
            float dotVal = (dotOverride > -10 ? dotOverride : 0.35f);
            float dot = Vector3.Dot(ent.Transform.forwardDirection, (other.Transform.Position - ent.Transform.Position).normalized);

            return dot >= dotVal;
        }


        public static bool IsFacingEachother(this EntityBase ent, EntityBase other, float dotOverride = -10)
        {
            return IsFacingEntity(ent, other, dotOverride) && IsFacingEntity(other, ent, dotOverride);
        }

        public static BrawlerFighterInfo GetBrawlerInfo(this Fighter fighter)
        {
            if (!BrawlerFighterInfo.Infos.ContainsKey(fighter.Character.UID))
                return new BrawlerFighterInfo();

            return BrawlerFighterInfo.Infos[fighter.Character.UID];
        }

        public static BaseAI TryGetAI(this Fighter fighter)
        {
            if (fighter.IsPlayer())
                return null;

            if (fighter.IsEnemy())
                return EnemyManager.GetAI(fighter);
            else
                return SupporterManager.GetAI(fighter);
        }

        public static BaseAI TryGetAI(this Character fighter)
        {
            BaseAI ai = EnemyManager.GetAI(fighter.UID);

            if (ai != null)
                return ai;
            else
                return SupporterManager.GetAI(fighter.UID);
        }

        public static bool IsAnyPartyMember(this Fighter fighter)
        {
            return FighterManager.GetFighter(10) == fighter || 
                   FighterManager.GetFighter(1) == fighter ||
                   FighterManager.GetFighter(2) == fighter ||
                   FighterManager.GetFighter(3) == fighter;
        }

        //TODO: Instead of checking like this get the fighter index and check if less than 4
        public static bool IsPartyMember(this Fighter fighter)
        {
            return FighterManager.GetFighter(1) == fighter ||
                   FighterManager.GetFighter(2) == fighter ||
                   FighterManager.GetFighter(3) == fighter;
        }

        public static bool IsPartyMember(this Character fighter)
        {
            return FighterManager.GetFighter(1).Character.UID == fighter.UID ||
                   FighterManager.GetFighter(2).Character.UID == fighter.UID ||
                   FighterManager.GetFighter(3).Character.UID == fighter.UID;
        }

        public static bool IsMainPlayer(this Fighter fighter)
        {
            return BrawlerBattleManager.PlayerCharacter.UID == fighter.Character.UID;
        }

        public static int GetPartyMemberIndex(this Fighter fighter)
        {
            if (FighterManager.GetFighter(0) == fighter)
                return 0;

            if (FighterManager.GetFighter(1) == fighter)
                return 1;

            if (FighterManager.GetFighter(2) == fighter)
                return 2;

            if (FighterManager.GetFighter(3) == fighter)
                return 3;

            return -1;
        }

        public static float GetHPRatio(this Fighter fighter)
        {
            ECBattleStatus status = fighter.GetStatus();

            return (float)status.CurrentHP / (float)status.MaxHP;
        }

        public static bool IsHPBelowRatio(this Fighter fighter, float ratio)
        {
            ECBattleStatus status = fighter.GetStatus();

            return status.CurrentHP <= (status.MaxHP * ratio);
        }

        public static bool IsBrawlerCriticalHP(this Fighter fighter)
        {
            return IsHPBelowRatio(fighter, Mod.CriticalHPRatio);
        }

        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input[0].ToString().ToUpper() + input.Substring(1);
            }
        }

        public static bool IsRunning(this Fighter fighter)
        {
            BrawlerFighterInfo inf = GetBrawlerInfo(fighter);

            if (inf == null)
                return false;

            return inf.IsMove && inf.MoveTime > 1f && !CombatPlayerPatches.HumanModeManager_IsInputKamae(fighter.Character.HumanModeManager.Pointer);
        }
    }
}
