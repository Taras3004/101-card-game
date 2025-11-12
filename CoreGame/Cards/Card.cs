public abstract class Card(Suit suit, Rank rank) : IUsableCard, IHaveEndgameValue
{
    public Rank Rank { get; } = rank;
    public Suit Suit { get; } = suit;

    public abstract int GetValue();

    public virtual void Use(GameEngine gameEngine, Player nextPlayer)
    {
        gameEngine.PassTurnToTheNextPlayer();
    }
}