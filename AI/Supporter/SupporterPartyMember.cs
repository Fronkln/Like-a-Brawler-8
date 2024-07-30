using System;
using DragonEngineLibrary;
using ElvisCommand;

namespace LikeABrawler2
{
    internal class SupporterPartyMember : BaseSupporterAI
    {
        public Player.ID PlayerID;

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
            Fighter.GetStatus().HPLock = 1;
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
                    break;
            }

            return 0;
        }
    }
}
