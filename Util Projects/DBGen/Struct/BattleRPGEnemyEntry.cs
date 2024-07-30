using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBGen
{
    public class BattleRPGEnemyEntry
    {
        //Greater than 0 - override specified ID
        public uint IDOverride = 0;

        public string Name;
        public string CtrlType;
        public string Arts;
        public ushort Model;
        public ushort EquipL;
        public ushort EquipR;
    }
}
