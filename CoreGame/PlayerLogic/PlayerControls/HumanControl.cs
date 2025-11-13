using CoreGame.CardsLogic;

namespace CoreGame.PlayerLogic.PlayerControls;

public class HumanControl : IControl
{
    // Ётот метод не должен вызыватьс€, т.к. UI (консоль)
    // обрабатывает ход человека
    public Card? MakeMove(GameContext context)
    {
        throw new InvalidOperationException("Human move should be handled by UI");
    }

    // јналогично дл€ выбора масти
    public Suit ChooseSuit(GameContext context)
    {
        throw new InvalidOperationException("Human suit choice should be handled by UI");
    }
}