using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBGen
{
    public class SoldierInfoEntry
    {
        //Not null - override specified ID
        public string IDOverride = null;

        public string EnemyID = "";
        public int CharaID = 0;
        public int Level = 1;
        public float HPRatio = 1f;
        public uint Health = 3;
        public ushort Attack = 8;
        public ushort Defense = 3;
        public ushort SPAttack = 8;
        public ushort SPDefense = 8;
        public int BaseWait = 8;
        public int EXPPoint = 6;
        public int JobEXPPoint = 6;
        public byte LifeGaugeType = 0;
        public bool ForceKind = false;
        public bool NoSujimon = false;
    }
}
