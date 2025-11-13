using CoreGame.PlayerLogic;

namespace CoreGame.CardsLogic.CardAbilities
{
    internal class SkipTurnEffect : ICardAbility
    {
        public void ApplyEffect(GameEngine gameEngine, Player player)
        {
            gameEngine.PassTurnToTheNextPlayer();
        }
    }
}
