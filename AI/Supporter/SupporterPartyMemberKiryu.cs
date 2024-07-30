using DragonEngineLibrary;
using System.Collections.Generic;

namespace LikeABrawler2
{
    internal class SupporterPartyMemberKiryu : SupporterPartyMember
    {
        //change style a maximum of once per turn
        private bool m_styleChangeOnce = false;
        private float m_nextStyleCheckTime = 0;

        private PlayerStyle currentStyle = PlayerStyle.Default;

        private void TransitStyleChange()
        {
            List<int> styleRotation = new List<int>();

            for (int i = 1; i < 4; i++)
                if ((int)currentStyle != i)
                    styleRotation.Add(i);

            PlayerStyle newStyle = (PlayerStyle)styleRotation[new System.Random().Next(0, styleRotation.Count)];
            currentStyle = newStyle;
            Character.HumanModeManager.ToStyleChange((int)newStyle);
        }

        public override void OnStartTurn()
        {
            base.OnStartTurn();

            if (currentStyle == PlayerStyle.Rush)
                SupporterManager.SkipDoubleTurn = true;
        }

        public override void CombatUpdate()
        {
            base.CombatUpdate();

            if(!MyTurn)
            {
                if (m_nextStyleCheckTime > 0)
                    m_nextStyleCheckTime -= DragonEngine.deltaTime;

                if (!m_styleChangeOnce)
                {
                    if (TimeSinceMyTurn > 1 && BrawlerBattleManager.BattleTime > 2.5f)
                    {
                        TransitStyleChange();
                        m_styleChangeOnce = true;
                    }
                }
            }
            else
            {
                if(!SupporterManager.SkipDoubleTurn)
                    m_styleChangeOnce = false;
            }
        }
    }
}
