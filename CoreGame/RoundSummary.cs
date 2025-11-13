using CoreGame.PlayerLogic;

namespace CoreGame
{
    public record RoundSummary(
        Player Winner,
        Dictionary<Player, int> AllPlayerScores,
        Dictionary<Player, int> RoundScoreChanges
    );
}
