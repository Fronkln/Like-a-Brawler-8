using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossDaigo : EnemyAIBoss
    {
        public static EnemyAIBossDaigo Instance;
        private bool Hact = false;

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }

        public override void CombatUpdate()
        {
            base.CombatUpdate();
        }

    }
}
