using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossRoman : EnemyAIBossWPY
    {
        public override void Awake()
        {
            base.Awake();

            EvasionModule.SetEvasionChance(40);
        }
    }
}
