using CoreGame.CardsLogic;
using CoreGame.PlayerLogic.PlayerControls;

namespace CoreGame.PlayerLogic;

public class Player
{
    public List<Card> CurrentCards { get; private set; }

    private readonly IControl controls;

    public Player(IControl controls, List<Card> currentCards)
    {
        CurrentCards = currentCards;

        this.controls = controls;
    }

    public Card MakeMove() => controls.MakeMove();
}