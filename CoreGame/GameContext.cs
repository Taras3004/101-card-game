using CoreGame.CardsLogic;

namespace CoreGame
{
    public class GameContext(List<Card> playerHand, Card topCard, Suit? suitOverride, Card? sixToCover)
    {
        public List<Card> PlayerHand { get; } = playerHand;
        public Card TopCard { get; } = topCard;
        public Suit? CurrentSuitOverride { get; } = suitOverride;
        public Card? ActiveSixToCover { get; } = sixToCover;
    }
}