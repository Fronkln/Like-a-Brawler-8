namespace LikeABrawler2
{
    public enum BaseAIParams : byte
    {
        Invalid,
        ExtendCombo = 1,
        SwayAttack = 2,
        CanDoNonTurnNearbyAttack = 3 //attack outside of our turn, mimicking realtime AI
    }
}
