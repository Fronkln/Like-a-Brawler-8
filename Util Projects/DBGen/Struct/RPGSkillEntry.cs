using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBGen
{
    public class RPGSkillEntry
    {
        public string Name;
        public string Motion;
        public byte UseCond = 23;
        public string CommandSet;
        public string CommandName;
        public string HAct;
        public float BaseAttackRatio = 100f;
        public int Category = 11;
        public byte Attribute = 1;
        public float BootDist = 1.5f;
        public float BootDistMin = 0.5f;
        public ushort EffRange = 2;
        public byte EffTargetLotType = 2;
        public uint SortID = 31;
        public uint RestTime = 100;
        public bool IsSync = false;
        public string OverrideName = "";
    }
}
