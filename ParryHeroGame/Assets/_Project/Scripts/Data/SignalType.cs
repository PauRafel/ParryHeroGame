namespace CombatGame.Data
{
    /// <summary>
    /// Type of timing signal shown to the player.
    /// Simple = single tap, Charged = press and hold.
    /// </summary>
    public enum SignalType
    {
        Simple,
        Charged
    }

    /// <summary>
    /// Who is performing the action this turn.
    /// Enemy attacking = player must Parry/Block.
    /// Hero attacking = player must Attack.
    /// </summary>
    public enum TurnOwner
    {
        Enemy,
        Hero
    }

    /// <summary>
    /// Result of a single timed input resolution.
    /// </summary>
    public enum HitResult
    {
        Perfect,
        Good,
        Miss
    }
}