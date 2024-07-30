using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    [Flags]
    internal enum TutorialModifier : uint
    {
        None = 0,
        DontAllowPlayerDamage = 1,
        DontAllowEnemyDamage = 2,
        FullHeat = 4,
        NoHeat = 8,
        DontAllowStyleChange = 16
    }
}
