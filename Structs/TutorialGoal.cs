using DragonEngineLibrary;
using System;

namespace LikeABrawler2
{
    internal struct TutorialGoal
    {
        public TalkParamID TalkID;
        public string Instructions;
        public Func<bool> CheckDelegate;
        public float TimeToComplete;
        public TutorialModifier Modifier;
        public Action OnStart;

        public void SetTalkID(TalkParamID id)
        {
            TalkID = id;
        }

        public void SetInstructions(string instructions)
        {
            Instructions = instructions;
        }

        public void SetCheck(Func<bool> func)
        {
            CheckDelegate = func;
        }
    }
}
