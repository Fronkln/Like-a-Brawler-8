﻿using System;


namespace ElvisCommand
{
    public class AttackInput
    {
        public AttackInputID Key;
        public bool Hold;

        public AttackInput(AttackInputID key, bool hold)
        {
            Key = key;
            Hold = hold;
        }
    }
}
