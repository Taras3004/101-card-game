using CoreGame.CardsLogic;
using CoreGame.PlayerLogic;

namespace CoreGame;

public class GameEngine
{
    private const int CardsPerPlayer = 4;

    private readonly List<Player> players;
    public Player CurrentPlayer { get; private set; }
    private int currentPlayerIndex;

    private bool isReversed;
    public Deck Deck { get; }

    public Player? PlayerToChooseSuit { get; private set; }

    public GameEngine(List<Player> players)
    {
        ArgumentNullException.ThrowIfNull(players);
        ArgumentOutOfRangeException.ThrowIfLessThan(players.Count, 2);

        this.players = players;

        CurrentPlayer = players[currentPlayerIndex];
        Deck = new Deck();
    }

    public void StartGame()
    {
        foreach (Player player in players)
        {
            var playerHand = Deck.Draw(CardsPerPlayer);

            player.CurrentCards.AddRange(playerHand);
        }
    }

    public void PlayTurn(Card playedCard)
    {
        if (!Deck.IsMoveLegal(playedCard))
        {
            throw new Exception("Nope");
        }

        CurrentPlayer.CurrentCards.Remove(playedCard);

        Deck.PlayCard(playedCard);

        playedCard.Use(this, CurrentPlayer);

        PassTurnToTheNextPlayer();
    }

    public void PlayBotTurn()
    {
        if (!CurrentPlayer.IsBot)
        {
            return;
        }

        var context = new GameContext(CurrentPlayer.CurrentCards, Deck.TopCard, Deck.CurrentSuitOverride, Deck.ActiveSixToCover);

        var botMove = CurrentPlayer.MakeMove(context);

        if (botMove != null)
        {
            PlayTurn(botMove);
        }
        else
        {
            if (Deck.ActiveSixToCover != null)
            {
                Card? cardToPlay;

                do
                {
                    cardToPlay = Deck.Draw();
                    CurrentPlayer.CurrentCards.Add(cardToPlay);
                } while (!Deck.IsMoveLegal(cardToPlay));

                PlayTurn(cardToPlay);
            }
            else
            {
                var additionalCard = Deck.Draw();
                CurrentPlayer.CurrentCards.Add(additionalCard);

                var secondTry = CurrentPlayer.MakeMove(context);

                if (secondTry != null)
                {
                    PlayTurn(secondTry);
                }
                else
                {
                    PassTurnToTheNextPlayer();
                }
            }
        }
    }

    public void SetCurrentSuitOverride(Suit suit)
    {
        Deck.SetSuitOverride(suit);
        PlayerToChooseSuit = null;
    }

    public void RequestSuitFrom(Player player)
    {
        PlayerToChooseSuit = player;
    }

    public void Reverse()
    {
        isReversed = !isReversed;
    }

    public void GiveCardsToNextPlayer(int count)
    {
        int nextPlayerIndex = GetNextPlayerIndex();

        Player nextPlayer = players[nextPlayerIndex];

        AddCardsToPlayer(nextPlayer, count);

        PassTurnToTheNextPlayer();
    }

    private int GetNextPlayerIndex()
    {
        if (!isReversed)
        {
            return (currentPlayerIndex + 1 == players.Count) ? 0 : currentPlayerIndex + 1;
        }

        return (currentPlayerIndex == 0) ? players.Count - 1 : currentPlayerIndex - 1;
    }


    public void AddCardsToPlayer(Player player, int countOfCards)
    {
        var cards = Deck.Draw(countOfCards);
        player.CurrentCards.AddRange(cards);
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

        CurrentPlayer = players[currentPlayerIndex];
    }
}