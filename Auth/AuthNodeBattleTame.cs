using DragonEngineLibrary;
using ElvisCommand;
using System;
namespace LikeABrawler2
{
    internal static class AuthNodeBattleTame
    {
        private static IntPtr _ActiveTame;
        private static bool m_activeDoOnce = false;
        private static float m_tameTime = 0;
        private static bool m_tameCharged = false;

        public static bool ShouldApplyTame()
        {
            return m_tameCharged;
        }

        private const float TAME_CHARGE_TIME = 0.5f;

        public static void Play(IntPtr thisObj, uint tick, IntPtr mtx, IntPtr parent)
        {
            bool charging = BattleManager.PadInfo.IsHold(BattleButtonID.heavy) || BattleManager.PadInfo.IsTimingPush(BattleButtonID.heavy, 250);

            if (_ActiveTame != thisObj)
            {
                Reset();
                _ActiveTame = thisObj;
            }

            if (m_tameCharged && _ActiveTame == thisObj)
                return;

            _ActiveTame = thisObj;

            if (charging)
            {
                if (!m_activeDoOnce)
                {
                    OnTameStart();
                    m_activeDoOnce = true;
                }

                BrawlerBattleManager.PlayerCharacter.GetMotion().SetTempSpeed(0.1f);
                m_tameTime += DragonEngine.deltaTime;

                if (!m_tameCharged && m_tameTime >= TAME_CHARGE_TIME)
                {
                    OnTameCharged();
                    m_tameCharged = true;
                }
            }
            else
            {
                if (m_activeDoOnce)
                    OnTameCancelled();

                m_activeDoOnce = false;
            }
        }

        public static void Reset()
        {
            m_tameTime = 0;
            m_tameCharged = false;
            m_activeDoOnce = false;
        }

        private static void OnTameStart()
        {
            DragonEngine.Log("Tame Start");
            Reset();
        }

        private static void OnTameCharged()
        {
            SoundManager.PlayCue(DBManager.GetSoundCuesheet("y8b_common"), 7, 0);
            DragonEngine.Log("Tame Charged");
        }

        private static void OnTameCancelled()
        {
            Reset();
            DragonEngine.Log("Tame End Premature");
        }
    }
}
