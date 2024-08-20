using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DragonEngineLibrary;
using DragonEngineLibrary.Unsafe;

namespace LikeABrawler2
{
    internal class TalkPatches : BrawlerPatch
    {
        private delegate long PartyTalkProc(IntPtr mng, IntPtr talkIDPtr);

        //purpose: prevent combat party talks from constantly replaying
        private static HashSet<int> m_playedTalks = new HashSet<int>();


        private IntPtr m_partyTalkFunc;

        public override void Init()
        {
            base.Init();

            m_partyTalkFunc = CPP.ReadCall(CPP.PatternSearch("E8 ? ? ? ? E9 ? ? ? ? 33 F6 49 8D 8F 98 02 00 00"));

            BrawlerBattleManager.OnBattleEndEvent += OnBattleEnd;
        }

        protected override void SetActive()
        {
            base.SetActive();

            if (m_partyTalkTrampoline == null)
                m_partyTalkTrampoline = BrawlerPatches.HookEngine.CreateHook<PartyTalkProc>(m_partyTalkFunc, PartyTalk_Proc);

            BrawlerPatches.HookEngine.EnableHook(m_partyTalkTrampoline);
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            if (m_partyTalkTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(m_partyTalkTrampoline);
        }

        private static PartyTalkProc m_partyTalkTrampoline;
        private unsafe static long PartyTalk_Proc(IntPtr mng, IntPtr talkIDPtr)
        {
            if (!BrawlerBattleManager.Battling)
                return m_partyTalkTrampoline(mng, talkIDPtr);

            if (HeatActionManager.AwaitingHAct || GameVarManager.GetValueBool(GameVarID.is_hact))
                return 0;

            int talkID = *(int*)talkIDPtr;

            if (m_playedTalks.Contains(talkID))
                return 0;

            m_playedTalks.Add(talkID);
            return m_partyTalkTrampoline(mng, talkIDPtr);
        }

        private void OnBattleEnd()
        {
            m_playedTalks.Clear();
        }
    }
}
