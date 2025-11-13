using CoreGame.CardsLogic;
using CoreGame.PlayerLogic.PlayerControls;

namespace CoreGame.PlayerLogic;

public class Player(IControl controls, List<Card> currentCards, bool isBot)
{
    public List<Card> CurrentCards { get; } = currentCards;
    public readonly bool IsBot = isBot;

    public Card? MakeMove(GameContext context) => controls.MakeMove(context);

    public Suit ChooseSuit(GameContext context) => controls.ChooseSuit(context);

    public void ClearHand() => CurrentCards.Clear();
}