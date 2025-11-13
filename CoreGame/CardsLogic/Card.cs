using CoreGame.CardsLogic.CardAbilities;
using CoreGame.PlayerLogic;

namespace CoreGame.CardsLogic;

public class Card(Suit suit, Rank rank, ICardAbility? ability = null)
{
    public Rank Rank { get; } = rank;
    public Suit Suit { get; } = suit;

    private readonly ICardAbility? Ability = ability;

    public void Use(GameEngine gameEngine, Player player)
    {
        Ability?.ApplyEffect(gameEngine, player);
    }

    public virtual int GetValue()
    {
        switch (Rank)
        {
            default: return 0;
            case Rank.Jack: return 2;
            case Rank.Queen: return 3;
            case Rank.King: return 4;
            case Rank.Ace: return 11;
            case Rank.Six: return 6;
            case Rank.Seven: return 7;
            case Rank.Eight: return 8;
            case Rank.Ten: return 10;
        }
    }
}