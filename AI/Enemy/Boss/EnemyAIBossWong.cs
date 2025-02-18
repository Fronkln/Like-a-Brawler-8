using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossWong : EnemyAIBoss
    {
        public override void Awake()
        {
            base.Awake();

            CounterAttacks.Add(DBManager.GetSkill("boss_wong_atk_b"));
        }

        public override bool TransitMortalAttack()
        {
            m_mortalSkill = DBManager.GetSkill("e_wong_mortal_attack");
            return true;
        }

        public override bool DamageExecValid(IntPtr battleDamageInfo)
        {
            bool baseVal = base.DamageExecValid(battleDamageInfo);

            if (!baseVal)
                return false;

            if (Character.GetMotion().GmtID == (MotionID)17213) //skl_buff
            {
                EntityHandle<Character> attacker = new BattleDamageInfoSafe(battleDamageInfo).Attacker;

                if (attacker.IsValid())
                {
                    if (Vector3.Distance(attacker.Get().Transform.Position, Character.Transform.Position) <= 2.5f)
                    {
                        BattleTurnManager.ForceCounterCommand(Fighter, attacker.Get().TryGetPlayerFighter(), DBManager.GetSkill("boss_wong_parry"));
                    }

                    return false;
                }
            }


            return baseVal;
        }
    }
}