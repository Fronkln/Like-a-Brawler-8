﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElvisCommand
{
    public enum NearestEnemyFlag : uint
    {
        Invalid = 0,
        IsDown,
        IsGettingUp,
        FacingPlayer,
        PlayerFacing,
        Distance
    }
}
