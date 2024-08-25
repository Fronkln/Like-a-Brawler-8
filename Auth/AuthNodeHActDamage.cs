using DragonEngineLibrary;
using LikeABrawler2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LikeABrawler2
{
    struct HActDamageResult
    {
        public bool Dies;
        public long Damage;
        public string DebugText;
    }

    internal class HActDamage : BrawlerPatch
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate void DamageNodeProcHook(IntPtr node, IntPtr damageInf, IntPtr fighter);

        private DamageNodeProcHook _dmgDeleg;
        private DamageNodeProcHook _dmgTrampoline;

        private IntPtr m_hactDmgFunc;

        public static event Action<Character, long, long> OnDamageDealt;

        public static void OnDamageDealtEvent(Character chara, long oldHp, long newHp)
        {
            OnDamageDealt?.Invoke(chara, oldHp, newHp);
        }

        public override void Init()
        {
            m_hactDmgFunc = DragonEngineLibrary.Unsafe.CPP.PatternSearch("48 89 5C 24 08 55 56 57 41 56 41 57 48 8D AC 24 10 F7 FF FF");
        }

        protected override void SetActive()
        {
            base.SetActive();

            if (_dmgTrampoline == null)
            {
                _dmgDeleg = new DamageNodeProcHook(DmgProc);
                _dmgTrampoline = BrawlerPatches.HookEngine.CreateHook<DamageNodeProcHook>(m_hactDmgFunc, DmgProc);
            }

           // BrawlerPatches.HookEngine.EnableHook(_dmgTrampoline);
        }

        protected override void SetInactive()
        {
            base.SetInactive();

            if (_dmgTrampoline != null)
                BrawlerPatches.HookEngine.DisableHook(_dmgTrampoline);
        }

        public static HActDamageResult CalculateDamage(Fighter attacker, Fighter victim, uint damage, int directDamage, float damageCap, bool isRatio, bool noDeath, bool forceDie)
        {
            HActDamageResult result = new HActDamageResult();

            ECBattleStatus victimStatus = victim.GetStatus();
            ECBattleStatus attackerStatus = attacker.GetStatus();

            BaseEnemyAI ai = EnemyManager.GetAI(victim);

            long finalDmg = 0;
            float hpPercentage = 0;
            long hpDamage = 0;

            long calculatedDamage = 0;

            if (isRatio)
            {
                hpPercentage = directDamage;
                hpDamage = ((long)(victimStatus.MaxHP * hpPercentage));
            }
            else
                hpDamage = directDamage;

            if (damage > 1)
            {
                if (ai != null)
                    calculatedDamage = (uint)(damage * HeatActionManager.DamageScale);
            }

            finalDmg = calculatedDamage + hpDamage;

            if (finalDmg == 0)
                finalDmg = 1;

#if DEBUG
            string dbgText = "\n(";

            if (noDeath)
                dbgText += "NON ";

            dbgText += "LETHAL)";
            dbgText +=
                $"\nCALCULATED: {calculatedDamage} {(calculatedDamage > 0 ? $"({damage} * {HeatActionManager.DamageScale})" : "")}" +
                $"\nMAX HP DMG: {hpDamage} {(hpPercentage > 0 ? $"({victimStatus.MaxHP} * {hpPercentage})" : "")}" +
                $"\nTOTAL: {finalDmg}";

            result.DebugText = dbgText;
#endif

            //Percentage
            if (damageCap > 0)
            {
                long cappedDamage = (long)(victimStatus.MaxHP * damageCap);

                //enemy has so little health this failed, 10% HP cap instead
                if (cappedDamage == 0)
                    cappedDamage = (long)(victimStatus.MaxHP * 0.1f);

                if (finalDmg > cappedDamage)
                {
                    finalDmg = cappedDamage;
#if DEBUG
                    Console.WriteLine("DAMAGE WAS TOO HIGH, WAS CAPPED TO " + cappedDamage + $"({hpPercentage * 100} of max HP)");
#endif
                }

                //Anti frustration feature: Bosses with more than 4k hp take 2.5% health damage in addition to base dmg
                if (attackerStatus.MaxHP > 4000)
                    finalDmg += ((long)(victimStatus.MaxHP * 0.025f));
            }

            long finalHp = finalHp = victimStatus.CurrentHP - finalDmg;

            if (ai != null)
            {
                int resistedDamage = (int)((finalDmg * ai.HeatActionDamageResist));
                finalHp += resistedDamage;
            }

            bool dieThroughNormalMeans = finalHp <= 0 && !noDeath;
            bool shouldDie = ((dieThroughNormalMeans || forceDie) && !noDeath);

            if (!ai.CanDieOnHAct())
            {
                dieThroughNormalMeans = false;
                result.Dies = false;
                result.Damage = 0;
            }
            else
            {
                result.Dies = shouldDie;
                result.Damage = finalDmg;
            }

            return result;
        }

        private unsafe void DmgProc(IntPtr node, IntPtr dmg, IntPtr fighter)
        {
            if (Mod.IsTurnBased())
            {
                _dmgTrampoline(node, dmg, fighter);
                return;
            }

            Fighter victim = new Fighter(fighter);

            //Bep/Sync/Normal Y8 hact damage
            if (BrawlerBattleManager.IsHAct)
            {
                long hp = victim.GetStatus().CurrentHP;

                _dmgTrampoline(node, dmg, fighter);
               // long newHp = victim.GetStatus().CurrentHP;
               // OnDamageDealt?.Invoke(victim.Character, hp, newHp);

                return;
            }
            else
            {
                _dmgTrampoline(node, dmg, fighter);
                return;
            }
        }
    }
}