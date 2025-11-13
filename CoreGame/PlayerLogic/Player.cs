using CoreGame.CardsLogic;
using CoreGame.PlayerLogic.PlayerControls;

namespace CoreGame.PlayerLogic;

public class Player
{
    public List<Card> CurrentCards { get; private set; }

    public readonly bool IsBot;

    private readonly IControl controls;

    public Player(IControl controls, List<Card> currentCards)
    {
        CurrentCards = currentCards;

        this.controls = controls;

        if (controls is not HumanControl)
        {
            IsBot = true;
        }
    }

    public Card? MakeMove(GameContext context) => controls.MakeMove(context);
    
}