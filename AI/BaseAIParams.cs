namespace LikeABrawler2
{
    public enum BaseAIParams : byte
    {
        Invalid,
        ExtendCombo = 1,
        SwayAttack = 2,
        CanDoNonTurnNearbyAttack = 3, //attack outside of our turn, mimicking realtime AI
        StandupAttack = 4,
        Scripted1 = 5,
        Scripted2 = 6,
        Scripted3 = 7, 
        Scripted4 = 8,
        Scripted5 = 9,
        Scripted6 = 10,
        Scripted7 = 11,
        Scripted8 = 12,
        AltCombo = 13,
        GetupAttack = 14
    }
}
