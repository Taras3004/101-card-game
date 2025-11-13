using CoreGame.CardsLogic;

namespace CoreGame
{
    public class GameContext
    {
        public List<Card> PlayerHand { get; }
        public Card TopCard { get; }
        
        //... some info for bot

        private GameContext() { }

        public GameContext(List<Card> playerHand, Card topCard)
        {
            PlayerHand = playerHand;
            TopCard = topCard;
        }
    }
}
