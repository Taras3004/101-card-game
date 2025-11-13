using CoreGame.CardsLogic;

namespace CoreGame.PlayerLogic.PlayerControls;

public class EasyBotControl : IControl
{
    // Логика "Легкого" бота
    public Card? MakeMove(GameContext context)
    {
        // Бот просто ищет первую карту, которая легальна
        foreach (var card in context.PlayerHand)
        {
            if (IsMoveLegal(card, context))
            {
                return card;
            }
        }
        return null; // Нет легального хода
    }

    // Бот выбирает масть, которой у него больше всего
    public Suit ChooseSuit(GameContext context)
    {
        if (!context.PlayerHand.Any())
            return (Suit)new Random().Next(0, 4); // Случайная, если нет карт

        return context.PlayerHand
            .GroupBy(card => card.Suit)
            .OrderByDescending(group => group.Count())
            .First().Key;
    }

    // У "Легкого" бота есть своя копия логики IsMoveLegal
    // (Это нормально, т.к. "стратегия" должна быть независимой)
    private bool IsMoveLegal(Card card, GameContext context)
    {
        if (context.CurrentSuitOverride != null)
        {
            return card.Suit == context.CurrentSuitOverride.Value || card.Rank == Rank.Queen;
        }

        if (context.ActiveSixToCover != null)
        {
            return card.Rank == Rank.Six || card.Suit == context.ActiveSixToCover.Suit;
        }

        return card.Rank == context.TopCard.Rank || card.Suit == context.TopCard.Suit;
    }
}