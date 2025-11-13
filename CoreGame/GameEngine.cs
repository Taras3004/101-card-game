using CoreGame.CardsLogic;
using CoreGame.PlayerLogic;

namespace CoreGame;

public class GameEngine
{
    public event Action<RoundSummary>? OnRoundEnded;

    public Dictionary<Player, int> PlayerScores { get; private set; }
    public Player CurrentPlayer { get; private set; }
    public Deck Deck { get; }
    public Player? PlayerToChooseSuit { get; private set; }

    private const int CardsPerPlayer = 4;
    private readonly List<Player> players;
    private int currentPlayerIndex;
    private bool isReversed;

    public GameEngine(List<Player> players)
    {
        ArgumentNullException.ThrowIfNull(players);
        ArgumentOutOfRangeException.ThrowIfLessThan(players.Count, 2);

        this.players = players;
        PlayerScores = new Dictionary<Player, int>(players.Count);
        foreach (var p in this.players)
        {
            PlayerScores.Add(p, 0);
        }

        CurrentPlayer = players[currentPlayerIndex];
        Deck = new Deck();
    }

    public void StartGame()
    {
        DealCards();
    }

    public void StartNewRound()
    {
        Deck.Reset();

        foreach (var player in players)
        {
            player.ClearHand();
        }

        isReversed = false;
        PlayerToChooseSuit = null;

        currentPlayerIndex = 0;
        CurrentPlayer = players[currentPlayerIndex];

        DealCards();
    }

    private void DealCards()
    {
        foreach (Player player in players)
        {
            var playerHand = Deck.Draw(CardsPerPlayer);
            player.CurrentCards.AddRange(playerHand);
        }
    }

    public void EndRound(Player winner)
    {
        var losers = players.Where(p => p != winner);
        var roundScoreChanges = new Dictionary<Player, int>();
        int loserScoreMultiplier = 1;

        int winnerScoreAdjustment = 0;
        bool multiplyLoserScore = false;

        if (Deck.TopCard.Rank == Rank.Queen)
        {
            multiplyLoserScore = true;
            winnerScoreAdjustment = -GetQueenEndGameValue(Deck.TopCard);
        }

        if (multiplyLoserScore)
        {
            loserScoreMultiplier = 2;
        }

        foreach (var player in losers)
        {
            int loserScoreGained;
            if (IsPlayerFinishedOnlyWithQueens(player))
            {
                loserScoreGained = player.CurrentCards.Sum(GetQueenEndGameValue);
            }
            else
            {
                loserScoreGained = player.CurrentCards.Sum(card => card.GetValue());
            }
            int finalScore = loserScoreGained * loserScoreMultiplier;
            PlayerScores[player] += finalScore;
            roundScoreChanges[player] = finalScore;
        }

        PlayerScores[winner] += winnerScoreAdjustment;
        if (PlayerScores[winner] < 0)
        {
            PlayerScores[winner] = 0;
        }

        roundScoreChanges[winner] = winnerScoreAdjustment;

        var summary = new RoundSummary(winner, PlayerScores, roundScoreChanges);
        OnRoundEnded?.Invoke(summary);
    }

    private bool IsPlayerFinishedOnlyWithQueens(Player player)
    {
        bool onlyQueens = true;

        foreach (var card in player.CurrentCards)
        {
            if (card.Rank != Rank.Queen)
            {
                onlyQueens = false;
            }
        }

        return onlyQueens;
    }

    private int GetQueenEndGameValue(Card card)
    {
        if (card.Rank != Rank.Queen)
        {
            return 0;
        }

        return card.Suit switch
        {
            Suit.Club => 20,
            Suit.Spade => 40,
            Suit.Heart => 20,
            Suit.Diamond => 20,
            _ => 0,
        };
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

        if (CurrentPlayer.CurrentCards.Count == 0)
        {
            EndRound(CurrentPlayer);
            return;
        }

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

                var newContext = new GameContext(CurrentPlayer.CurrentCards, Deck.TopCard, Deck.CurrentSuitOverride, Deck.ActiveSixToCover);

                var secondTry = CurrentPlayer.MakeMove(newContext);

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