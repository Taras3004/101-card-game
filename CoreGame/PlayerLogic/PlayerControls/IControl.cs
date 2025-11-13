using CoreGame.CardsLogic;

namespace CoreGame.PlayerLogic.PlayerControls;

public interface IControl
{
    public Card? MakeMove(GameContext context);

    public Suit ChooseSuit(GameContext context);
}