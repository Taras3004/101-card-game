using CoreGame;
using CoreGame.CardsLogic;
using CoreGame.PlayerLogic;
using CoreGame.PlayerLogic.PlayerControls;

public static class Program
{
    private static GameEngine gameEngine;
    private static Deck gameDeck; 
    private static bool isRoundOver = false; // Flag to control the round loop
    private static Player humanPlayer; // Need a direct reference for scores
    private static Player botPlayer;   // Need a direct reference for scores

    public static void Main(string[] args)
    {
        // 1. Налаштування гравців
        var humanControl = new HumanControl();
        var botControl = new EasyBotControl(); // Переконайся, що EasyBotControl реалізований

        humanPlayer = new Player(humanControl, new List<Card>(), isBot: false);
        botPlayer = new Player(botControl, new List<Card>(), isBot: true);

        var players = new List<Player> { humanPlayer, botPlayer };

        // 2. Ініціалізація рушія
        gameEngine = new GameEngine(players);
        gameDeck = gameEngine.Deck; 

        // --- 🚀 ОСЬ ВИРІШЕННЯ БАГУ 🚀 ---
        // Ми підписуємося на подію. Тепер Program.cs
        // БУДЕ знати, коли раунд закінчився.
        gameEngine.OnRoundEnded += HandleRoundEnded;
        // ------------------------------------

        Console.WriteLine("--- 🃏 \"101\" Game Started! 🃏 ---");
        
        bool playAgain = true;
        
        // --- "ІГРОВИЙ" ЦИКЛ (для "Зіграти ще?") ---
        while (playAgain)
        {
            // 3. Починаємо раунд
            isRoundOver = false;
            gameEngine.StartNewRound(); // Використовуємо твій новий метод

            // --- "РАУНДОВИЙ" ЦИКЛ (крутиться, поки isRoundOver == false) ---
            while (!isRoundOver) // <-- Більше ніяких 'while(true)'
            {
                // Тут НЕМАЄ перевірки 'if (Count == 0)', 
                // бо нам це не потрібно.

                // 4. Обробка вибору масті (Дама)
                HandleSuitChoice();

                // 5. Показ стану
                DisplayGameState();

                // 6. Хід
                if (gameEngine.CurrentPlayer.IsBot)
                {
                    HandleBotTurn();
                }
                else
                {
                    HandleHumanTurn();
                }
            }
            
            // --- Раунд закінчився (isRoundOver став true) ---
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nPlay another round? (y/n): ");
            Console.ResetColor();
            
            var choice = Console.ReadLine()?.ToLower();
            if (choice != "y")
            {
                playAgain = false;
            }
        }
        
        Console.WriteLine("\nThanks for playing! Final score:");
        DisplayScores();
    }

    // --- EVENT HANDLER ---
    // This method is called AUTOMATICALLY by GameEngine when a round ends
    private static void HandleRoundEnded(RoundSummary summary)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n🎉🎉🎉 ROUND OVER! 🎉🎉🎉");
        Console.WriteLine($"Winner: {(summary.Winner.IsBot ? "Bot" : "You")}");
        Console.ResetColor();

        // Show score changes
        foreach (var change in summary.RoundScoreChanges)
        {
            string playerName = change.Key.IsBot ? "Bot" : "You";
            string prefix = change.Value >= 0 ? "+" : "";
            Console.WriteLine($"  {playerName} gets: {prefix}{change.Value} points");
        }
        
        DisplayScores();
        
        // Set flag to stop the "Round" loop in Main()
        isRoundOver = true;
    }

    // --- Display Total Score ---
    private static void DisplayScores()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("--- TOTAL SCORE ---");
        Console.WriteLine($"  You: {gameEngine.PlayerScores[humanPlayer]}");
        Console.WriteLine($"  Bot: {gameEngine.PlayerScores[botPlayer]}");
        Console.WriteLine("-------------------");
        Console.ResetColor();
    }

    // --- Helper methods (almost identical to your version) ---

    private static void HandleSuitChoice()
    {
        var playerToChoose = gameEngine.PlayerToChooseSuit;
        if (playerToChoose == null) return;

        Suit chosenSuit;
        // We must pass the FULL context for the bot to make a good choice
        var context = new GameContext(playerToChoose.CurrentCards, gameDeck.TopCard,
                                     gameDeck.CurrentSuitOverride, gameDeck.ActiveSixToCover);

        if (playerToChoose.IsBot)
        {
            // Now we call the bot's own logic
            chosenSuit = playerToChoose.ChooseSuit(context);
            Console.WriteLine($"[Bot played Queen and chose: {chosenSuit}]");
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
        // Show total score every turn
        Console.WriteLine($"SCORE: [You: {gameEngine.PlayerScores[humanPlayer]}] [Bot: {gameEngine.PlayerScores[botPlayer]}]");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Playing: {(gameEngine.CurrentPlayer.IsBot ? "Bot" : "You")}");
        Console.ResetColor();

        Console.WriteLine($"Top card: {gameDeck.TopCard.Rank} of {gameDeck.TopCard.Suit}");

        if (gameDeck.CurrentSuitOverride.HasValue)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"CHOSEN SUIT: {gameDeck.CurrentSuitOverride.Value}");
            Console.ResetColor();
        }

        if (gameDeck.ActiveSixToCover != null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ATTENTION! Need to cover {gameDeck.ActiveSixToCover.Rank} of {gameDeck.ActiveSixToCover.Suit}!");
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
            Console.WriteLine($"Bot's cards: {gameEngine.CurrentPlayer.CurrentCards.Count} шт.");
        }
    }

    private static void HandleBotTurn()
    {
        Console.WriteLine("[Bot thinking...]");
        Thread.Sleep(1000); 

        // We "peek" at the bot's move to show it
        var context = new GameContext(gameEngine.CurrentPlayer.CurrentCards, gameDeck.TopCard,
                                     gameDeck.CurrentSuitOverride, gameDeck.ActiveSixToCover);
        var botMove = gameEngine.CurrentPlayer.MakeMove(context);

        // All logic (drawing if null, etc.) is inside PlayBotTurn
        gameEngine.PlayBotTurn();

        // Show result
        if (botMove != null)
            Console.WriteLine($"[Bot played: {botMove.Rank} of {botMove.Suit}]");
        else
            Console.WriteLine("[Bot took card(s) as there was no move.]");
    }

    private static void HandleHumanTurn()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Your turn (enter card number or 'd'): ");
        Console.ResetColor();

        while (true) // Loop for retrying on invalid input
        {
            string input = Console.ReadLine() ?? "";

            if (input.ToLower() == "d")
            {
                // Handle human drawing logic
                HandleHumanDraw();
                break; // Turn is over
            }

            if (int.TryParse(input, out int cardIndex) &&
                cardIndex >= 0 && cardIndex < gameEngine.CurrentPlayer.CurrentCards.Count)
            {
                // Handle playing a card
                var cardToPlay = gameEngine.CurrentPlayer.CurrentCards[cardIndex];

                if (gameDeck.IsMoveLegal(cardToPlay))
                {
                    gameEngine.PlayTurn(cardToPlay);
                    Console.WriteLine($"You played: {cardToPlay.Rank} of {cardToPlay.Suit}");
                    break; // Turn is over
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Illegal move! Try again.");
                    Console.ResetColor();
                    continue; // Stay in the loop
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
        // This logic is identical to your version, just with English text
        if (gameDeck.ActiveSixToCover != null)
        {
            // --- "Six" logic: Draw until you cover ---
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
                    Thread.Sleep(500); 
                }
            }
            gameEngine.PlayTurn(cardToPlay); // Automatically play the card
        }
        else
        {
            // --- "Crocodile" logic (Six is NOT active) ---
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