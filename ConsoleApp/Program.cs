using CoreGame;
using CoreGame.CardsLogic;
using CoreGame.PlayerLogic;
using CoreGame.PlayerLogic.PlayerControls;

public static class Program
{
    private static GameEngine gameEngine;
    private static Deck gameDeck;

    public static void Main(string[] args)
    {
        var humanControl = new HumanControl();
        var botControl = new EasyBotControl();

        var player1 = new Player(humanControl, new List<Card>(), isBot: false);
        var player2 = new Player(botControl, new List<Card>(), isBot: true);

        var players = new List<Player> { player1, player2 };

        gameEngine = new GameEngine(players);

        gameDeck = gameEngine.Deck;

        gameEngine.StartGame();

        while (true)
        {
            if (gameEngine.CurrentPlayer.CurrentCards.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{(gameEngine.CurrentPlayer.IsBot ? "Bot" : "you")} win!");
                break;
            }

            HandleSuitChoice();

            DisplayGameState();

            if (gameEngine.CurrentPlayer.IsBot)
            {
                HandleBotTurn();
            }
            else
            {
                HandleHumanTurn();
            }
        }
    }

    private static void HandleSuitChoice()
    {
        var playerToChoose = gameEngine.PlayerToChooseSuit;
        if (playerToChoose == null) return;

        Suit chosenSuit;
        var context = new GameContext(playerToChoose.CurrentCards, gameDeck.TopCard,
                                     gameDeck.CurrentSuitOverride, gameDeck.ActiveSixToCover);

        if (playerToChoose.IsBot)
        {
            chosenSuit = playerToChoose.ChooseSuit(context);
            Console.WriteLine($"[Bot player Queen and choose: {chosenSuit}]");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("You played Queen! Choose suit (1-4):");
            Console.WriteLine("  1. Club\n  2. Diamond\n  3. Heart\n  4. Spade");
            Console.ResetColor();
            while (true)
            {
                var key = Console.ReadKey(true).KeyChar;
                if (key == '1') { chosenSuit = Suit.Club; break; }
                if (key == '2') { chosenSuit = Suit.Diamond; break; }
                if (key == '3') { chosenSuit = Suit.Heart; break; }
                if (key == '4') { chosenSuit = Suit.Spade; break; }
            }
            Console.WriteLine($"You chose: {chosenSuit}");
        }

        gameEngine.SetCurrentSuitOverride(chosenSuit);
    }

    private static void DisplayGameState()
    {
        Console.WriteLine("\n---------------------------------");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Playing: {(gameEngine.CurrentPlayer.IsBot ? "Bot" : "you")}");
        Console.ResetColor();

        Console.WriteLine($"Top card: {gameDeck.TopCard.Rank} of {gameDeck.TopCard.Suit}");

        if (gameDeck.CurrentSuitOverride.HasValue)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Chose suit: {gameDeck.CurrentSuitOverride.Value}");
            Console.ResetColor();
        }

        if (gameDeck.ActiveSixToCover != null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Opponent played {gameDeck.ActiveSixToCover.Rank} of {gameDeck.ActiveSixToCover.Suit}!");
            Console.ResetColor();
        }

        if (!gameEngine.CurrentPlayer.IsBot)
        {
            Console.WriteLine("Your cards:");
            int index = 0;
            foreach (var card in gameEngine.CurrentPlayer.CurrentCards)
            {
                bool isLegal = gameDeck.IsMoveLegal(card);
                if (isLegal) Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine($"  {index}. {card.Rank} of {card.Suit}");
                Console.ResetColor();
                index++;
            }
            Console.WriteLine("  d. Take a card");
        }
        else
        {
            Console.WriteLine($"Bot's cards: {gameEngine.CurrentPlayer.CurrentCards.Count}");
        }
    }

    private static void HandleBotTurn()
    {
        Console.WriteLine("[Bot thinking...]");
        Thread.Sleep(1000);

        var context = new GameContext(gameEngine.CurrentPlayer.CurrentCards, gameDeck.TopCard,
                                     gameDeck.CurrentSuitOverride, gameDeck.ActiveSixToCover);

        var botMove = gameEngine.CurrentPlayer.MakeMove(context);

        gameEngine.PlayBotTurn();

        if (botMove != null)
            Console.WriteLine($"[Bot played: {botMove.Rank} of {botMove.Suit}]");
        else
            Console.WriteLine("[Bot took card");
    }

    private static void HandleHumanTurn()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Your turn (enter card number or 'd'): ");
        Console.ResetColor();

        while (true)
        {
            string input = Console.ReadLine() ?? "";

            if (input.ToLower() == "d")
            {
                HandleHumanDraw();
                break;
            }

            if (int.TryParse(input, out int cardIndex) &&
                cardIndex >= 0 && cardIndex < gameEngine.CurrentPlayer.CurrentCards.Count)
            {
                var cardToPlay = gameEngine.CurrentPlayer.CurrentCards[cardIndex];

                if (gameDeck.IsMoveLegal(cardToPlay))
                {
                    gameEngine.PlayTurn(cardToPlay);
                    Console.WriteLine($"You played: {cardToPlay.Rank} of {cardToPlay.Suit}");
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Illegal move! Try again.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("Incorrect input. Enter card number or 'd'.");
            }
        }
    }

    private static void HandleHumanDraw()
    {
        if (gameDeck.ActiveSixToCover != null)
        {
            Console.WriteLine($"Need to cover {gameDeck.ActiveSixToCover.Rank}!");
            Card? cardToPlay = null;

            while (cardToPlay == null)
            {
                var newCard = gameDeck.Draw();
                gameEngine.CurrentPlayer.CurrentCards.Add(newCard);
                Console.WriteLine($"You took: {newCard.Rank} of {newCard.Suit}");

                if (gameDeck.IsMoveLegal(newCard))
                {
                    cardToPlay = newCard;
                    Console.WriteLine("Card can cover six! Playing this card...");
                    Thread.Sleep(1000);
                }
                else
                {
                    Console.WriteLine("Card can't cover six, continuing...");
                    Thread.Sleep(1000);
                }
            }
            gameEngine.PlayTurn(cardToPlay);
        }
        else
        {
            var newCard = gameDeck.Draw();
            gameEngine.CurrentPlayer.CurrentCards.Add(newCard);
            Console.WriteLine($"You took: {newCard.Rank} of {newCard.Suit}");

            if (gameDeck.IsMoveLegal(newCard))
            {
                Console.Write("Card can be played. Play it? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    gameEngine.PlayTurn(newCard);
                }
                else
                {
                    Console.WriteLine("You've decided to hold. Passing turn to the next player.");
                    gameEngine.PassTurnToTheNextPlayer();
                }
            }
            else
            {
                Console.WriteLine("Card can't be played. Passing turn to the next player.");
                gameEngine.PassTurnToTheNextPlayer();
            }
        }
    }
}