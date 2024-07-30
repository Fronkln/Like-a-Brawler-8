using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal static class SupporterManager
    {
        public static Dictionary<uint, BaseSupporterAI> Supporters = new Dictionary<uint, BaseSupporterAI>();
        public static BaseSupporterAI[] SupportersNearest = new BaseSupporterAI[0];
        //Used by party supporter AI at awake to set stats to party member stats
        public static Dictionary<Player.ID, PartyMemberTempStatStore> PartyStats = new Dictionary<Player.ID, PartyMemberTempStatStore>();
        public static BaseSupporterAI NextSupporterAttacker;

        public const bool ConvertPartyMemberToSupporter = false;

        /// <summary>
        /// Used for kiryu rush style party member
        /// </summary>
        public static bool SkipDoubleTurn = false;

        public static void UpdateSupporterDB()
        {
            return;

            /*
            for (uint i = 1; i < 4; i++)
            {
                Fighter fighter = FighterManager.GetFighter(i);

                if (!fighter.IsValid())
                    continue;

                string dbEntry = "supporter_" + fighter.Character.Attributes.player_id;
                ArmpEntry entry = DBManager.BattleRpgEnemy.MainTable.GetEntry(dbEntry);

                DBManager.BattleRpgEnemy.MainTable.SetValue(entry.ID, "equip_r", (ushort)6806);
            }
            */

            //1.02.2024: Crash, does it need to write it the memory way?
            //DBManager.OverwriteARMPData(3050, DBManager.BattleRpgEnemy);
        }


        public static void OnBattleEnd()
        {
            Supporters.Clear();
            NextSupporterAttacker = null;
            SkipDoubleTurn = false;
        }

        public static void Update()
        {
            if (BrawlerBattleManager.AllEnemies.Length <= 0 && BattleTurnManager.CurrentPhase >= BattleTurnManager.TurnPhase.Action)
                return;

            if (ConvertPartyMemberToSupporter)
            {
                foreach (Fighter fighter in BrawlerBattleManager.AllFighters)
                {
                    if (!fighter.IsEnemy() && !fighter.IsPlayer() && !fighter.IsPartyMember())
                        if (!Supporters.ContainsKey(fighter.Character.UID))
                        {
                            CharacterAttributes attribs = fighter.Character.Attributes;
                            string soldierName = DBManager.GetSoldier((uint)attribs.soldier_data_id);
                            bool isParty = soldierName.StartsWith("supporter_");
                            Player.ID playerID = Player.ID.invalid;

                            if (isParty)
                                playerID = (Player.ID)Enum.Parse(typeof(Player.ID), soldierName.Split('_')[1]); //supporter_->>>playername

                            CreateSupporter(fighter, playerID);
                        }
                }
            }
            else
            {
                foreach (Fighter fighter in BrawlerBattleManager.AllFighters)
                {
                    if (!fighter.IsMainPlayer() && (fighter.IsPartyMember() || !fighter.IsEnemy()))
                    {
                        if (!Supporters.ContainsKey(fighter.Character.UID))
                        {
                            CreateSupporter(fighter, fighter.Character.Attributes.player_id);
                        }
                    }
                }
            }

            Supporters = Supporters.Where(x => new EntityHandle<Character>(x.Key).IsValid()).ToDictionary(x => x.Key, x => x.Value);
            SupportersNearest = Supporters.OrderBy(x => Vector3.Distance(BrawlerBattleManager.PlayerFighter.Character.Transform.Position, x.Value.Character.Transform.Position)).Select( x => x.Value).ToArray();

            foreach (var kv in Supporters)
            {
                BaseSupporterAI supporter = kv.Value;
                supporter.Update();

                if (BattleTurnManager.CurrentPhase == BattleTurnManager.TurnPhase.Action && !HeatActionManager.IsHAct())
                    supporter.CombatUpdate();
            }
        }

        public static BaseSupporterAI CreateSupporter(Fighter fighter, Player.ID playerID)
        {
            BaseSupporterAI ai = null;

            if (playerID > 0)
            {
                SupporterPartyMember pMemberAi = null;

                if (playerID == Player.ID.kiryu)
                    pMemberAi = new SupporterPartyMemberKiryu();
                else
                    pMemberAi = new SupporterPartyMember();

                pMemberAi.PlayerID = playerID;

                ai = pMemberAi;
            }
            else
                ai = new BaseSupporterAI();

            ai.Fighter = fighter;
            ai.Character = fighter.Character;

            ai.Awake();

            Supporters.Add(fighter.Character.UID, ai);

            return ai;
        }

        public static BaseSupporterAI GetAI(Fighter fighter)
        {
            if (Supporters.ContainsKey(fighter.Character.UID))
                return Supporters[fighter.Character.UID];

            return null;
        }

        public static BaseSupporterAI GetAI(uint UID)
        {
            if (Supporters.ContainsKey(UID))
                return Supporters[UID];

            return null;
        }

        public static void ReloadContent()
        {
            foreach (var kv in Supporters)
                kv.Value.LoadContent();
        }
    }
}
