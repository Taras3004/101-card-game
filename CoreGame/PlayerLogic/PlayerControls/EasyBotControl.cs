using CoreGame.CardsLogic;

namespace CoreGame.PlayerLogic.PlayerControls;

public class EasyBotControl : IControl
{
    public Card? MakeMove(GameContext context)
    {
        foreach (var card in context.PlayerHand)
        {
            if (IsMoveLegal(card, context))
            {
                return card;
            }
        }
        return null;
    }

    public Suit ChooseSuit(GameContext context)
    {
        if (!context.PlayerHand.Any())
            return (Suit)new Random().Next(0, 4);

        return context.PlayerHand
            .GroupBy(card => card.Suit)
            .OrderByDescending(group => group.Count())
            .First().Key;
    }

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