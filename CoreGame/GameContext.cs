using CoreGame.CardsLogic;

namespace CoreGame
{
    public class GameContext
    {
        public List<Card> PlayerHand { get; }
        public Card TopCard { get; }
        public Suit? CurrentSuitOverride { get; }
        public Card? ActiveSixToCover { get; }

        public GameContext(List<Card> playerHand, Card topCard, Suit? suitOverride, Card? sixToCover)
        {
            PlayerHand = playerHand;
            TopCard = topCard;
            CurrentSuitOverride = suitOverride;
            ActiveSixToCover = sixToCover;
        }
    }
}