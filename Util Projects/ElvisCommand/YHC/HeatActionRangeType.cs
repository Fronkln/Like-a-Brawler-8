using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElvisCommand
{
    public enum HeatActionRangeType
    {
        None = 0,
        mukou,
        hit_wall,
        hit_wall_safety,
        hit_wall_force,
        dropped_throw,
        pole,
        guardrail,
        corner,
        stand,
        high_range,
        low_range,
        stairs_up,
        stairs_down,
        oven,
        stairs,
        plain,
        train_throw,
        special,
        warp_safety,
        warp_safety_slope,
        warp_force,
        water_side,
        water_high_side,
        door_left,
        door_right,
        battle_space,
        battle_result,
    }
}
