using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBGen
{
    public class MotionFlagInfo
    {
        public bool is_common_pack = false;
        public bool is_loop = false;
        public ushort base_gmt = 0;
        public float start_frame = 0;
        public bool is_counter_attack = false;
        public uint action_joint_tick = 0;
        public bool is_charge_attack = false;
        public bool is_nage_attack = false;
        public bool is_finish_hold_attack = false;
        public bool is_heavy_attack = false;
        public bool is_light_attack = false;
        public bool is_damage_reversal = false;
        public bool is_ma_break = false;
        public bool is_reverse_attack = false;
        public bool is_enable_rev_sabaki = false;
        public bool is_sway_attack = false;
        public bool is_ex_attack = false;
        public bool is_revenge = false;
        public bool is_run_attack = false;
        public bool is_low_attack = false;
        public bool is_combo_finish = false;
    }
}
