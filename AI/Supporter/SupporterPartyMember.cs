using System;
using DragonEngineLibrary;
using ElvisCommand;

namespace LikeABrawler2
{
    internal class SupporterPartyMember : BaseSupporterAI
    {
        public Player.ID PlayerID;
        public float LastTimeSinceSkillUsed = 99999;

        public static int DecideAutoModeStrategy(Fighter partyMember)
        {
            SupporterPartyMember ai = SupporterManager.GetAI(partyMember) as SupporterPartyMember;
            Random rnd = new Random();

            if (ai == null)
            {
                if (rnd.Next(0, 101) > IniSettings.PartyMemberSkillChance)
                    return 3;

                return 1;
            }

            if (ai.LastTimeSinceSkillUsed < IniSettings.PartyMemberSkillTime)
                return 3;

            if (rnd.Next(0, 101) > IniSettings.PartyMemberSkillChance)
                return 3;

            int heat = Player.GetHeatNow(ai.PlayerID);
            int maxHeat = Player.GetHeatMax(ai.PlayerID);

            int requiredHeat = (int)(maxHeat * IniSettings.PartyMemberSkillMPReqRatio);

            if (requiredHeat > heat)
                return 3;

            ai.LastTimeSinceSkillUsed = 0;
            return 1;
        }

        public override void Awake()
        {
            base.Awake();

            PlayerID = Character.Attributes.player_id;
            Flags |= SupporterFlags.Party;

            if (PlayerID == Player.ID.kasuga ||
                PlayerID == Player.ID.kiryu ||
                PlayerID == Player.ID.adachi ||
                PlayerID == Player.ID.nanba ||
                PlayerID == Player.ID.chou ||
                PlayerID == Player.ID.tomizawa ||
                PlayerID == Player.ID.jyungi)
                Flags |= SupporterFlags.Male;
            else
                Flags |= SupporterFlags.Female;

            SupporterFlags characterFlag;

            if (Enum.TryParse(PlayerID.ToString().FirstCharToUpper(), out characterFlag))
                Flags |= characterFlag;


            //Don't let em die
            //14.03.2025 Update: Let 'em die let 'em die let em shrivel up and die
            //Fighter.GetStatus().HPLock = 1;
        }

        public override bool IsPartyMember()
        {
            return true;
        }

        public override void BattleStartEvent()
        {
            base.BattleStartEvent();

            CharacterID specialID = GetSpecialCharacterIDForAlly();

            if (specialID != 0)
                Character.GetRender().Reload(specialID);
        }

        private CharacterID GetSpecialCharacterIDForAlly()
        {
            //battle config 37 - wong tou fight

            uint configID = BrawlerBattleManager.BattleConfigID;
            bool specialSuit = configID == 37 || configID == 38 || configID == 39 || configID == 40;

            switch (PlayerID)
            {
                default:
                    return 0;
                case Player.ID.kiryu:
                    if (specialSuit)
                        return (CharacterID)25111;
                    break;
                case Player.ID.tomizawa:
                    if (specialSuit)
                        return (CharacterID)25112;
                    break;
                case Player.ID.chitose:
                    if (specialSuit)
                        return (CharacterID)25113;
                    else if (configID == 194) //waitress
                        return (CharacterID)27230;
                    break;
            }

            return 0;
        }

        public override void CombatUpdate()
        {
            base.CombatUpdate();

            LastTimeSinceSkillUsed += DragonEngine.deltaTime;
        }
    }
}
