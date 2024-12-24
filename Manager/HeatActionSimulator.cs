using DragonEngineLibrary;
using System.Collections.Generic;
using ElvisCommand;
using HeatActionCondition = ElvisCommand.HeatActionCondition;
using FighterMap = System.Collections.Generic.Dictionary<ElvisCommand.HeatActionActorType, DragonEngineLibrary.Fighter>;
using System;
using DragonEngineLibrary.Service;
using System.Linq;

namespace LikeABrawler2
{
    public static class HeatActionSimulator
    {
        public static HeatActionInformation Check(Fighter performer, EHC hactList)
        {
            if(!performer.IsValid() || hactList == null || hactList.Attacks.Count <= 0)
                return null;

            FighterMap map = new FighterMap();

            map[HeatActionActorType.Fighter] = performer;
            map[HeatActionActorType.Player] = BrawlerBattleManager.PlayerFighter;

            if (performer.IsPlayer() || !performer.IsEnemy())
            {
                Fighter[] enemies = BrawlerBattleManager.AllEnemiesNearest;
                int curEnemyIdx = 0;

                if (enemies != null && enemies.Length > 0)
                {
                    //Register enemies for player
                    for (int i = (int)HeatActionActorType.Enemy1; ; i++)
                    {
                        if (curEnemyIdx >= enemies.Length || curEnemyIdx == 5)
                            break;

                        map[(HeatActionActorType)i] = enemies[curEnemyIdx];

                        curEnemyIdx++;
                    }
                }

                BaseSupporterAI[] supporters = SupporterManager.SupportersNearest;
                curEnemyIdx = 0;

                if (supporters != null && supporters.Length > 0)
                {
                    //Register supporters for player
                    for (int i = (int)HeatActionActorType.Ally1; ; i++)
                    {
                        if (curEnemyIdx >= supporters.Length || curEnemyIdx == 5)
                            break;

                        map[(HeatActionActorType)i] = supporters[curEnemyIdx].Fighter;

                        curEnemyIdx++;
                    }
                }
            }
            else
            {
                BaseEnemyAI ai = EnemyManager.GetAI(performer);

                if (ai == null)
                    return null;

                List<Fighter> enemies = ai.PlayersNearest;
                int curEnemyIdx = 0;

                if (enemies != null && enemies.Count > 0)
                {
                    //Register enemies for player
                    for (int i = (int)HeatActionActorType.Enemy1; ; i++)
                    {
                        if (curEnemyIdx >= enemies.Count || curEnemyIdx == 5)
                            break;

                        map[(HeatActionActorType)i] = enemies[curEnemyIdx];

                        curEnemyIdx++;
                    }
                }
            }

            foreach (HeatActionAttack attack in hactList.Attacks)
            {
                if (attack.IsFollowupOnly)
                    continue;

                HActRangeInfo info = new HActRangeInfo();

                if (attack.Range != HeatActionRangeType.None)
                    if (!performer.GetStatus().HAct.GetPlayInfo(ref info, (HActRangeType)attack.Range))
                        goto cont;

                HeatActionInformation inf = new HeatActionInformation();
                inf.Performer = performer;
                inf.Map = map;
                inf.Hact = attack;
                inf.RangeInfo = info;

                FighterMap adjustedMap = new FighterMap(map);

                List<HeatActionActor> invalidActors = new List<HeatActionActor>();


                foreach (HeatActionActor actor in attack.Actors)
                {
                    bool exists = map.ContainsKey(actor.Type);

                    if (!exists && !actor.Optional)
                        goto cont;

                    if (exists)
                    {
                        BaseAI ai = map[actor.Type].TryGetAI();

                        if (ai != null && !ai.CanBeHActed())
                            goto cont;

                        foreach (HeatActionCondition cond in actor.Conditions)
                            if (!CheckFlag(attack, map, map[actor.Type], cond, inf.RangeInfo))
                            {
                                if (!actor.Optional)
                                    goto cont;
                                else
                                    invalidActors.Add(actor);
                            }
                    }
                }

                foreach (var invActor in invalidActors)
                    adjustedMap.Remove(invActor.Type);

                inf.Map = adjustedMap;

                return inf;

            cont:
                continue;
            }


            return null;
        }

        private static bool CheckFlag(HeatActionAttack attack, FighterMap fightersList, Fighter actor, ElvisCommand.HeatActionCondition cond, HActRangeInfo rangeInf)
        {
            if (!actor.IsValid())
                return false;

            bool flag = false;
            bool performerIsPlayer = actor.IsPlayer();

            CharacterAttributes actorAttributes = actor.Character.Attributes;
            BaseAI ai = actor.TryGetAI();
            BrawlerFighterInfo inf = actor.GetBrawlerInfo();

            if (!performerIsPlayer && ai == null)
                return false;

            switch (cond.Type)
            {
                case HeatActionConditionType.Invalid:
                    flag = true;
                    break;

                case HeatActionConditionType.CriticalHP:
                    flag = actor.IsBrawlerCriticalHP();
                    break;
                case HeatActionConditionType.CharacterLevel:
                    flag = Logic.CheckNumberLogicalOperator(actor.GetStatus().Level, cond.Param1U32, cond.LogicalOperator);
                    break;
                case HeatActionConditionType.Job:
                    if (actor.IsPlayer())
                        flag = Logic.CheckNumberLogicalOperator((uint)Player.GetCurrentJob(BrawlerPlayer.CurrentPlayer), cond.Param1U32, cond.LogicalOperator);
                    break;
                case HeatActionConditionType.JobLevel:
                    if (actor.IsPlayer())
                        flag = Logic.CheckNumberLogicalOperator((uint)Player.GetJobLevel(BrawlerPlayer.CurrentPlayer), cond.Param1U32, cond.LogicalOperator);
                    break;

                case HeatActionConditionType.Down:

                    if (!performerIsPlayer)
                        if (ai.IsBeingJuggled())
                        {
                            flag = false;
                            break;
                        }


                    if (cond.Param2B)
                        flag = inf.IsFaceDown || inf.IsDown;
                    else
                    {
                        if (cond.Param1B)
                            flag = inf.IsFaceDown;
                        else
                            flag = inf.IsDown;
                    }
                    break;

                case HeatActionConditionType.DownOrGettingUp:
                    if (!performerIsPlayer)
                        if (ai.IsBeingJuggled())
                        {
                            flag = false;
                            break;
                        }

                    flag = inf.IsDown || inf.IsGettingUp;
                    break;

                case HeatActionConditionType.DownTime:
                    flag = Logic.CheckNumberLogicalOperator(inf.DownTime, cond.Param1F, cond.LogicalOperator);
                    break;

                case HeatActionConditionType.CanAttackGeneric:
                    flag = !inf.CantAttackOverall();

                    if (!cond.Param1B && inf.IsAttack)
                        flag = false;

                    break;

                case HeatActionConditionType.Attacking:
                    flag = inf.IsAttack;
                    break;

                case HeatActionConditionType.Flinching:
                    flag = inf.IsFlinching;
                    break;

                case HeatActionConditionType.DistanceToHactPosition:
                    Vector3 hactPos = new Vector3(attack.Position[0], attack.Position[1], attack.Position[2]);
                    float distToHact = Vector3.Distance(hactPos, (Vector3)actor.Character.Transform.Position);
                    flag = Logic.CheckNumberLogicalOperator(distToHact, cond.Param1F, cond.LogicalOperator);
                    break;
                case HeatActionConditionType.Distance:
                    bool range = cond.Param1B;

                    if (!fightersList.ContainsKey((HeatActionActorType)cond.Param1U32))
                        break;

                    Fighter distanceTarget = fightersList[(HeatActionActorType)cond.Param1U32];
                    float dist = Vector3.Distance(actor.Character.Transform.Position, distanceTarget.Character.Transform.Position);

                    if (range)
                    {
                        float minRange = cond.Param1F;
                        float maxRange = cond.Param2F;

                        flag = dist >= minRange && dist <= maxRange;
                    }
                    else
                        flag = Logic.CheckNumberLogicalOperator(dist, cond.Param1F, cond.LogicalOperator);

                    break;

                case HeatActionConditionType.FacingTarget:
                    HeatActionActorType faceTarget = (HeatActionActorType)cond.Param1U32;

                    if (!fightersList.ContainsKey(faceTarget))
                        return false;

                    Character faceTargetActor = fightersList[faceTarget].Character;

                    float dot = Vector3.Dot(actor.Character.Transform.forwardDirection, (faceTargetActor.Transform.Position - actor.Character.Transform.Position).normalized);

                    flag = dot >= 0.2;
                    break;

                case HeatActionConditionType.FacingRange:
                    HActRangeInfo facingInf = new HActRangeInfo();

                    if (!actor.GetStatus().HAct.GetPlayInfo(ref facingInf, (HActRangeType)cond.Param1U32))
                        return false;
                    else
                    {
                        float faceDot = Vector3.Dot(actor.Character.Transform.forwardDirection, ((Vector3)facingInf.Pos - actor.Character.Transform.Position).normalized);
                        flag = faceDot >= 0.2;
                    }
                    break;
                case HeatActionConditionType.DistanceToRange:
                    flag = Logic.CheckNumberLogicalOperator(Vector3.Distance(actor.Character.GetPosCenter(), (Vector3)rangeInf.Pos), cond.Param1F, cond.LogicalOperator);
                    break;

                case HeatActionConditionType.InRange:
                    HActRangeInfo inRangeInf = new HActRangeInfo();
                    flag = actor.GetStatus().HAct.GetPlayInfo(ref inRangeInf, (HActRangeType)cond.Param1U32);
                    break;

                case HeatActionConditionType.WeaponType:
                    if (inf.RightWeapon == null)
                        return false;

                    flag = (uint)Asset.GetArmsCategory(actor.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().AssetID) == cond.Param1U32;
                    break;

                case HeatActionConditionType.AssetID:
                    if (inf.RightWeapon == null)
                        return false;

                    flag = (uint)actor.GetWeapon(AttachmentCombinationID.right_weapon).Unit.Get().AssetID == cond.Param1U32;
                    break;

                case HeatActionConditionType.WeaponSubtype:
                    if (inf.RightWeapon == null)
                        return false;

                    flag = (uint)Asset.GetArmsCategorySub(inf.RightWeapon.Unit.AssetID) == cond.Param1U32;
                    break;

                case HeatActionConditionType.CurrentCommand:
                    FighterCommandID commandId = actor.Character.HumanModeManager.CurrentMode.GetCommandID();
                    ushort set = commandId.set_;
                    FighterCommandInfo commandInf = commandId.GetInfo();

                    bool ccond1 = DBManager.GetCommandSet(set) == cond.ParamString1;
                    bool ccond2 = (string.IsNullOrEmpty(cond.ParamString2) || commandInf.Id == cond.ParamString2);
                    bool ccond3 = (string.IsNullOrEmpty(cond.ParamString3) || commandInf.Id.StartsWith(cond.ParamString3));

                    if (!string.IsNullOrEmpty(cond.ParamString1))
                        flag = ccond2 || ccond3;
                    else
                        flag = ccond1 && ccond2 && ccond3;

                    flag = ccond1 || ccond2 && ccond3;
                    break;

                case HeatActionConditionType.EXHeat:
                    flag = BrawlerPlayer.IsExtremeHeat;
                    break;

                case HeatActionConditionType.Running:
                    flag = actor.IsRunning();
                    break;

                case HeatActionConditionType.IsSupporter:
                    flag = SupporterManager.GetAI(actor) != null;
                    break;

                case HeatActionConditionType.SupporterFlags:
                    SupporterFlags sflag = (SupporterFlags)cond.Param1I32;

                    BaseSupporterAI supporter = SupporterManager.GetAI(actor);

                    if (supporter == null)
                        flag = false;
                    else
                        flag = supporter.Flags.HasFlag(sflag);
                    break;

                case HeatActionConditionType.Grabbing:
                    string currentCommand = actor.Character.HumanModeManager.GetCommandName();
                    string grabString = "GrabProcedure";

                    AttackSyncDirection grabDir = (AttackSyncDirection)cond.Param1I32;
                    AttackSyncCategory grabType = (AttackSyncCategory)cond.Param2I32;

                    if (grabDir > 0)
                        grabString += (grabDir == AttackSyncDirection.Front ? "F" : "B");

                    if (grabType > 0)
                        grabString += "_" + grabType.ToString();

                    //GrabProcedureB_Neck_XXXXXXX
                    flag = currentCommand.Contains(grabString);
                    break;

                case HeatActionConditionType.DistanceToNearestAsset:
                    flag = Logic.CheckNumberLogicalOperator(Vector3.Distance(actor.Character.GetPosCenter(), AssetManager.FindNearestAssetFromAll(actor.Character.GetPosCenter(), 0).Get().GetPosCenter()), cond.Param1F, cond.LogicalOperator);
                    break;
                case HeatActionConditionType.NearestAssetSpecialType:

                    AssetID specialAssetID = AssetManager.FindNearestAssetFromAll(actor.Character.GetPosCenter(), 0).Get().AssetID;
                    switch (cond.Param1U32)
                    {
                        default:
                            flag = false;
                            break;
                        case 1:
                            flag = specialAssetID.ToString().StartsWith("car");
                            break;
                        case 2:
                            flag = specialAssetID == AssetID.stgy162 || specialAssetID == AssetID.stgy131;
                            break;
                    }
                    break;

                case HeatActionConditionType.BeingJuggled:
                    if (ai != null)
                        flag = ai.IsBeingJuggled();
                    break;

                case HeatActionConditionType.HumanMode:
                    flag = actor.Character.HumanModeManager.CurrentMode.ModeName == cond.ParamString1;
                    break;

                case HeatActionConditionType.MotionID:
                    flag = (uint)actor.Character.GetMotion().GmtID == cond.Param1U32;
                    break;

                case HeatActionConditionType.IsBoss:
                    if(ai != null)
                    {
                        var enemyAI = ai as BaseEnemyAI;

                        if (enemyAI != null)
                            flag = enemyAI.IsBoss();
                    }
                    break;

                case HeatActionConditionType.StatusEffect:
                    flag = actor.HasExEffect((int)cond.Param1U32);
                    break;

                case HeatActionConditionType.Swaying:
                    flag = actor.Character.HumanModeManager.CurrentMode.ModeName == "Sway";
                    break;

                case HeatActionConditionType.PlayerPoint:
                    flag = Logic.CheckNumberLogicalOperator(PlayerPoint.GetPoint((PlayerPoint.ID)cond.Param1U32), cond.Param1U32, cond.LogicalOperator);
                    break;
            }

            switch (cond.LogicalOperator)
            {
                case LogicalOperator.TRUE:
                    if (flag)
                        return true;
                    else
                        return false;
                case LogicalOperator.FALSE:
                    if (!flag)
                        return true;
                    else
                        return false;
            }

            return flag;
        }
    }
}
