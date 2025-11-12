using CoreGame.PlayerLogic;

namespace CoreGame.CardsLogic.CardAbilities;

public interface ICardAbility
{
    public void ApplyEffect(GameEngine gameEngine, Player nextPlayer);
}