using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CoreGame;
using CoreGame.CardsLogic;
using CoreGame.PlayerLogic;
using CoreGame.PlayerLogic.PlayerControls;
using System.Collections.Generic;

namespace AvaloniaUI.ViewModels;

public partial class GameViewModel : ObservableObject
{
    // --- Поля Ігрового Рушія ---
    private readonly GameEngine _gameEngine;
    private readonly Deck _gameDeck;
    private readonly Player _humanPlayer;
    private readonly Player _botPlayer;

    // --- Властивості стану (для прив'язки до UI) ---

    // Рука гравця
    public ObservableCollection<CardViewModel> PlayerHand { get; } = new();
    
    // Рука бота (ми будемо показувати тільки кількість)
    [ObservableProperty]
    private int _botCardCount;

    // Верхня карта у скиді
    [ObservableProperty]
    private CardViewModel? _topCard;

    // Рахунок
    [ObservableProperty]
    private int _playerScore;
    [ObservableProperty]
    private int _botScore;

    // Повідомлення для гравця (напр., "Ваш хід", "Бот думає...")
    [ObservableProperty]
    private string _gameMessage;

    // Чи можна гравцю ходити (для блокування UI)
    [ObservableProperty]
    private bool _isPlayerTurn = true;

    // Чи показувати вікно вибору масті
    [ObservableProperty]
    private bool _isSuitPickerVisible = false;

    // --- Конструктор ---
    public GameViewModel()
    {
        // 1. Створюємо гравців та рушій (як у консолі)
        var humanControl = new HumanControl();
        var botControl = new EasyBotControl(); // Використовуємо твого EasyBot

        _humanPlayer = new Player(humanControl, new List<Card>(), isBot: false);
        _botPlayer = new Player(botControl, new List<Card>(), isBot: true);

        var players = new List<Player> { _humanPlayer, _botPlayer };
        
        _gameEngine = new GameEngine(players);
        _gameDeck = _gameEngine.Deck; // Отримуємо доступ до колоди

        // 2. Підписуємось на подію кінця раунду
        _gameEngine.OnRoundEnded += OnRoundEnded;

        // 3. Починаємо гру
        StartNewRound();
    }

    // --- Команди (для прив'язки кнопок) ---

    [RelayCommand]
    private async Task OnDrawCard()
    {
        if (!_isPlayerTurn || _isSuitPickerVisible) return;
        
        SetPlayerTurn(false); // Блокуємо UI
        GameMessage = "You are drawing cards...";

        // Логіка "Шістки": тягнути, доки не поб'єш
        if (_gameDeck.ActiveSixToCover != null)
        {
            Card? cardToPlay = null;
            while (cardToPlay == null)
            {
                var newCard = _gameDeck.Draw();
                _humanPlayer.CurrentCards.Add(newCard);
                UpdatePlayerHand(); // Оновлюємо UI, щоб гравець бачив
                await Task.Delay(500); // Пауза, щоб було видно карти

                if (_gameDeck.IsMoveLegal(newCard))
                {
                    cardToPlay = newCard;
                }
            }
            GameMessage = $"You found a card! Playing {cardToPlay.Rank}...";
            await Task.Delay(500);
            _gameEngine.PlayTurn(cardToPlay);
        }
        else // Логіка "Крокодила": тягнути одну
        {
            var newCard = _gameDeck.Draw();
            _humanPlayer.CurrentCards.Add(newCard);
            
            if (_gameDeck.IsMoveLegal(newCard))
            {
                GameMessage = $"You drew {newCard.Rank} and can play it. Playing...";
                await Task.Delay(1000);
                _gameEngine.PlayTurn(newCard);
            }
            else
            {
                GameMessage = "You drew a card. Passing turn.";
                _gameEngine.PassTurnToTheNextPlayer();
            }
        }
        
        // Оновлюємо UI і перевіряємо, чи не час ходити боту
        UpdateAllViewModels();
        CheckForBotTurn();
    }

    [RelayCommand]
    private void OnChooseSuit(string suit)
    {
        if (Enum.TryParse<Suit>(suit, out var chosenSuit))
        {
            _gameEngine.SetCurrentSuitOverride(chosenSuit);
            IsSuitPickerVisible = false;
            UpdateAllViewModels(); // Оновлюємо легальність ходів
            
            // Якщо Даму зіграв бот, передаємо хід гравцю
            if (!_isPlayerTurn)
            {
                SetPlayerTurn(true);
            }
        }
    }

    [RelayCommand]
    private void StartNewRound()
    {
        _gameEngine.StartNewRound();
        UpdateAllViewModels();
        SetPlayerTurn(!_gameEngine.CurrentPlayer.IsBot);
    }

    // --- Приватна логіка ---

    // Цей метод викликається з CardViewModel
    private async void OnPlayCard(CardViewModel cardVM)
    {
        if (!_isPlayerTurn || !_isSuitPickerVisible && !cardVM.IsPlayable)
        {
            GameMessage = "You can't play that card!";
            return;
        }

        SetPlayerTurn(false); // Блокуємо UI
        _gameEngine.PlayTurn(cardVM.Card);
        
        // Чекаємо, чи не закінчився раунд
        if (_gameEngine.CurrentPlayer.CurrentCards.Count == 0) return;

        UpdateAllViewModels();
        CheckForBotTurn();
    }

    private async void CheckForBotTurn()
    {
        // Якщо зараз хід бота (або він ходить знову, напр. Тузом)
        while (_gameEngine.CurrentPlayer.IsBot)
        {
            if (_gameEngine.PlayerToChooseSuit == _botPlayer)
            {
                // Бот зіграв Даму і має обрати масть
                GameMessage = "Bot is choosing a suit...";
                await Task.Delay(1000);
                
                var context = new GameContext(_botPlayer.CurrentCards, _gameDeck.TopCard, _gameDeck.CurrentSuitOverride, _gameDeck.ActiveSixToCover);
                var chosenSuit = _botPlayer.ChooseSuit(context);
                _gameEngine.SetCurrentSuitOverride(chosenSuit);
                GameMessage = $"Bot chose {chosenSuit}!";
                UpdateAllViewModels();
            }

            GameMessage = "Bot is thinking...";
            await Task.Delay(1500); // Симуляція роздумів

            _gameEngine.PlayBotTurn();
            
            // Перевіряємо, чи не виграв бот
            if (_botPlayer.CurrentCards.Count == 0) break;
            
            UpdateAllViewModels();
        }

        // Хід повернувся до гравця
        SetPlayerTurn(true);
    }

    // Оновлює ВСІ властивості UI зі стану GameEngine
    private void UpdateAllViewModels()
    {
        // Оновлюємо руку гравця
        PlayerHand.Clear();
        foreach (var card in _humanPlayer.CurrentCards.OrderBy(c => c.Suit).ThenBy(c => c.Rank))
        {
            var cardVM = new CardViewModel(card, OnPlayCard);
            // Встановлюємо, чи можна цією картою ходити
            cardVM.IsPlayable = _gameDeck.IsMoveLegal(card);
            PlayerHand.Add(cardVM);
        }

        // Оновлюємо руку бота (тільки кількість)
        BotCardCount = _botPlayer.CurrentCards.Count;

        // Оновлюємо стіл
        TopCard = _gameDeck.TopCard != null ? new CardViewModel(_gameDeck.TopCard, null) : null;
        if(TopCard != null) TopCard.IsPlayable = false; // Карту на столі не можна "грати"

        // Оновлюємо рахунок
        PlayerScore = _gameEngine.PlayerScores[_humanPlayer];
        BotScore = _gameEngine.PlayerScores[_botPlayer];

        // Перевіряємо, чи потрібно показати вибір масті
        IsSuitPickerVisible = _gameEngine.PlayerToChooseSuit == _humanPlayer;
    }

    private void SetPlayerTurn(bool isTurn)
    {
        _isPlayerTurn = isTurn;
        if (isTurn && !IsSuitPickerVisible)
        {
            GameMessage = "Your turn!";
        }
    }

    // --- Обробник подій ---
    private void OnRoundEnded(RoundSummary summary)
    {
        // Оскільки ця подія приходить з іншого потоку (GameEngine),
        // ми маємо оновити UI в головному потоці (UI thread)
        Dispatcher.UIThread.Invoke(() =>
        {
            UpdateAllViewModels();
            IsSuitPickerVisible = false;
            GameMessage = $"{(summary.Winner.IsBot ? "Bot" : "You")} won the round! Click 'Start New Round'.";
            // Можна додати кнопку "Start New Round", яка стає видимою
        });
    }
}