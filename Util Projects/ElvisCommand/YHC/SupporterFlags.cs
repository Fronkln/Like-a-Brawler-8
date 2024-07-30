using System;

namespace ElvisCommand
{
    [Flags]
    public enum SupporterFlags : int
    {
        None = 0,
        Party = 1 << 0,
        Male = 1 << 1,
        Female = 1 << 2,
        Kiryu = 1 << 3,
        Adachi = 1 << 4,
        Nanba = 1 << 5,
        Saeko = 1 << 6,
        Jyungi = 1 << 7,
        Chou = 1 << 8,
        Tomizawa = 1 << 9,
        Chitose = 1 << 10,
        Kasuga = 1 << 11,
        Sonhi = 1 << 12
    }
}
