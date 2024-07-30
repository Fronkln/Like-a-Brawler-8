using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibARMP;

namespace DBGen
{
    public static class Extensions
    {
        public static ArmpEntry GetEntryProper(this ArmpTable table, string name)
        {
            foreach (ArmpEntry entry in table.GetAllEntries())
                if (entry.Name == name)
                    return entry;

            return null;
        }
    }
}
