using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossMajima : EnemyAIBossWPB
    {
        public static EnemyAIBossMajima Instance;
        public override void Awake()
        {
            base.Awake();
            Instance = this;
            HActList = YazawaCommandManager.LoadYHC("boss/majima.ehc");
        }
    }
}
