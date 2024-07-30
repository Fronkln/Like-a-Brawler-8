using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class EnemyAIBossLongPierrot : EnemyAIBoss
    {
        public override void Update()
        {
            base.Update();

            Fighter.GetStatus().SetSuperArmor(true);
        }

        public override bool CanBeHActed()
        {
            return false;
        }
    }
}
