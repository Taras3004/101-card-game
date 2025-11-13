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
        throw new NotImplementedException();
    }
}