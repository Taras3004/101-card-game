public class GameEngine
{
    private List<Player> players;
    public Player currentPlayer { get; private set; }
    private int currentPlayerIndex = 0;

    public GameEngine(List<Player> players)
    {
        ArgumentNullException.ThrowIfNull(players);
        ArgumentOutOfRangeException.ThrowIfLessThan(players.Count, 2);

        this.players = players;

        currentPlayer = players[currentPlayerIndex];
    }

    public void PassTurnToTheNextPlayer()
    {
        if (currentPlayerIndex + 1 == players.Count)
        {
            currentPlayerIndex = 0;
        }
        else
        {
            currentPlayerIndex++;
        }

        currentPlayer = players[currentPlayerIndex];
    }
}