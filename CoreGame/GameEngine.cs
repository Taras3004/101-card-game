using CoreGame.CardsLogic;
using CoreGame.PlayerLogic;

namespace CoreGame;

public class GameEngine
{
    private List<Player> players;
    public Player currentPlayer { get; private set; }
    private int currentPlayerIndex = 0;

    private List<Card> unusedCards;
    private bool isReversed;

    public GameEngine(List<Player> players)
    {
        ArgumentNullException.ThrowIfNull(players);
        ArgumentOutOfRangeException.ThrowIfLessThan(players.Count, 2);

        this.players = players;

        currentPlayer = players[currentPlayerIndex];

        unusedCards = CreateDeck();
    }

    private static List<Card> CreateDeck()
    {
        List<Card> newDeck = new List<Card>();

        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                var card = new Card(suit, rank);
                newDeck.Add(card);
            }
        }

        return newDeck;
    }

    public void Reverse()
    {
        isReversed = !isReversed;
    }

    public void AddCardsToPlayer()
    {
        throw new NotImplementedException();
    }

    public void PassTurnToTheNextPlayer()
    {
        if (!isReversed)
        {
            if (currentPlayerIndex + 1 == players.Count)
            {
                currentPlayerIndex = 0;
            }
            else
            {
                currentPlayerIndex++;
            }
        }
        else if (isReversed)
        {
            if (currentPlayerIndex == 0)
            {
                currentPlayerIndex = players.Count - 1;
            }
            else
            {
                currentPlayerIndex--;
            }
        }

        currentPlayer = players[currentPlayerIndex];
    }
}