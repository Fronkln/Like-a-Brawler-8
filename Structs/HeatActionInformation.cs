using DragonEngineLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElvisCommand;

namespace LikeABrawler2
{
    //Information returned by checking EHC
    public class HeatActionInformation
    {
        public HeatActionAttack Hact = null;
        public Fighter Performer = new Fighter();
        public HActRangeInfo RangeInfo; //Assigned if found
        public Dictionary<HeatActionActorType, Fighter> Map = new Dictionary<HeatActionActorType, Fighter>();

        public Vector3 PosOverride;

        public bool UseHeat = true;
    }
}
