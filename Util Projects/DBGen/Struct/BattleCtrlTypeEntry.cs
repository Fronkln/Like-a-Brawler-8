using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBGen
{
    public class BattleCtrlTypeEntry
    {
        public string CommandSet;
        public bool IsBoss = false;
        public ushort CharacterID = 0;
        public byte FighterType = 1;
        public bool IsNPC = false;
    }
}
