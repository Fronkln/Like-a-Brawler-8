﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElvisCommand
{
    public enum HeatActionSpecialType : uint
    {
        Normal = 0,
        Asset = 1, //Asset HAct, use nearest asset position + forward direction
        SpecialRange = 18, //range type: special
    }
}
