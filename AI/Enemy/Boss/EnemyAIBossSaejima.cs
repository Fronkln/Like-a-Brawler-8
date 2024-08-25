using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossSaejima : EnemyAIBoss
    {
        public static EnemyAIBossSaejima Instance;

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
    }
}
