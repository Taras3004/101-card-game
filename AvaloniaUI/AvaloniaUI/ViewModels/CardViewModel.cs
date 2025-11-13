using System.Windows.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreGame;
using CoreGame.CardsLogic;
using CoreGame.PlayerLogic;
using CoreGame.PlayerLogic.PlayerControls;

namespace AvaloniaUI.ViewModels
{
    public partial class CardViewModel : ViewModelBase
    {
    public Card Card { get; }

    public string DisplayText { get; }

    public IBrush SuitColor { get; }

    public ICommand PlayCardCommand { get; }

    [ObservableProperty]
    private bool _isPlayable;

    public CardViewModel(Card card, Action<CardViewModel> onPlayCardAction)
    {
        Card = card;

        DisplayText = $"{card.Rank}\n{GetSuitSymbol(card.Suit)}";
        
        SuitColor = (card.Suit == Suit.Heart || card.Suit == Suit.Diamond)
            ? Brushes.Red
            : Brushes.Black;
            
        PlayCardCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(() =>
        {
            onPlayCardAction?.Invoke(this);
        });
    }

    private string GetSuitSymbol(Suit suit)
    {
        return suit switch
        {
            Suit.Heart => "♥",
            Suit.Diamond => "♦",
            Suit.Club => "♣",
            Suit.Spade => "♠",
            _ => "?"
        };
    }
    }
}
