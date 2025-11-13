using CoreGame.CardsLogic;
using CoreGame.CardsLogic.CardAbilities;

namespace CoreGame
{
    public class Deck
    {
        private List<Card> drawPile;
        public List<Card> DiscardPile { get; private set; }
        public Card TopCard => DiscardPile.LastOrDefault()!;

        public Suit? CurrentSuitOverride { get; private set; }
        public Card? ActiveSixToCover { get; private set; }

        public Deck()
        {
            DiscardPile = new List<Card>();
            drawPile = CreateDeck();
            Shuffle();
            PlaceFirstCard();
        }

        private void Shuffle()
        {
            Random random = new Random();

            int n = drawPile.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (drawPile[k], drawPile[n]) = (drawPile[n], drawPile[k]);
            }
        }

        private void PlaceFirstCard()
        {
            DiscardPile.Add(drawPile[^1]);
            drawPile.RemoveAt(drawPile.Count - 1);
        }

        public void SetSuitOverride(Suit suit)
        {
            CurrentSuitOverride = suit;
        }

        public bool IsMoveLegal(Card card)
        {
            if (CurrentSuitOverride != null)
            {
                return card.Suit == CurrentSuitOverride.Value || card.Rank == Rank.Queen;
            }

            if (ActiveSixToCover != null)
            {
                return card.Rank == Rank.Six || card.Suit == ActiveSixToCover.Suit;
            }

            return card.Rank == TopCard.Rank || card.Suit == TopCard.Suit;
        }

        private static List<Card> CreateDeck()
        {
            List<Card> newDeck = new List<Card>();

            ICardAbility reverseEffect = new ReverseEffect();
            ICardAbility sevenEffect = new AddCardsEffect(2);
            ICardAbility kingOfHeartsEffect = new AddCardsEffect(5);
            ICardAbility kingOfSpadesEffect = new AddCardsEffect(5);
            ICardAbility changeSuitEffect = new ChangeCurrentSuitEffect();
            ICardAbility aceAbility = new SkipTurnEffect();

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    ICardAbility? assignedAbility = null;

                    switch (rank)
                    {
                        case Rank.King:
                        {
                            if (suit == Suit.Spade)
                            {
                                assignedAbility = kingOfSpadesEffect;
                            }
                            else if (suit == Suit.Heart)
                            {
                                assignedAbility = kingOfHeartsEffect;
                            }

                            break;
                        }
                        case Rank.Ten:
                            assignedAbility = reverseEffect;
                            break;
                        case Rank.Seven:
                            assignedAbility = sevenEffect;
                            break;
                        case Rank.Queen:
                            assignedAbility = changeSuitEffect;
                            break;
                        case Rank.Ace:
                            assignedAbility = aceAbility;
                            break;
                    }

                    var card = new Card(suit, rank, assignedAbility);
                    newDeck.Add(card);
                }
            }

            return newDeck;
        }

        public void PlayCard(Card card)
        {
            if (!IsMoveLegal(card))
            {
                throw new Exception("Nope");
            }

            DiscardPile.Add(card);
            CurrentSuitOverride = null;
            ActiveSixToCover = card.Rank == Rank.Six ? card : null;
        }

        public List<Card> Draw(int count)
        {
            List<Card> drew = new List<Card>();

            for (int i = 0; i < count; i++)
            {
                var cardToDraw = Draw();

                drew.Add(cardToDraw);
            }

            return drew;
        }

        public Card Draw()
        {
            if (drawPile.Count == 0)
            {
                RestockDrawPile();
            }

            var cardToDraw = drawPile.LastOrDefault()!;
            drawPile.RemoveAt(drawPile.Count - 1);
            return cardToDraw;
        }

        private void RestockDrawPile()
        {
            var topCard = TopCard;
            drawPile = DiscardPile;
            drawPile.Remove(topCard);

            Shuffle();
            DiscardPile = [topCard];
        }
    }
}