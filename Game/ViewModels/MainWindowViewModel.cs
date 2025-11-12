using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Game.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _newTaskName;

    public ObservableCollection<string> Tasks { get; } = new ObservableCollection<string>();

    private bool CanAddNewTask()
    {
        return !string.IsNullOrEmpty(NewTaskName);
    }

    [RelayCommand(CanExecute = nameof(CanAddNewTask))]
    private void AddTask()
    {
        Tasks.Add(NewTaskName!);

        NewTaskName = null;
    }

    partial void OnNewTaskNameChanged(string? value)
    {
        AddTaskCommand.NotifyCanExecuteChanged();
    }
}
