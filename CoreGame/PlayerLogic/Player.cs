using CoreGame.CardsLogic;
using CoreGame.PlayerLogic.PlayerControls;

namespace CoreGame.PlayerLogic;

public class Player
{
    public List<Card> CurrentCards { get; private set; }
    public readonly bool IsBot;
    private readonly IControl controls;

    public Player(IControl controls, List<Card> currentCards, bool isBot)
    {
        CurrentCards = currentCards;
        this.controls = controls;
        IsBot = isBot;
    }

    public Card? MakeMove(GameContext context) => controls.MakeMove(context);

    public Suit ChooseSuit(GameContext context) => controls.ChooseSuit(context);
}