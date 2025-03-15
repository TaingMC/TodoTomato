using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TodoTomato.Services;

namespace TodoTomato.ViewModels
{
    public partial class ToDoListWindowViewModel : ViewModelBase
    {
        public ObservableCollection<string> AllTasks { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ImportantTasks { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> CompletedTasks { get; } = new ObservableCollection<string>();
        private ObservableCollection<string> _currentTasks;
        private int _completedPomodoroCount;
        private string _taskName;
        private DispatcherTimer _timer;
        private TimeSpan _remainingTime;
        private string _countdownDisplay = "25:00";
        public string CountdownDisplay
        {
            get => _countdownDisplay;
            set => SetProperty(ref _countdownDisplay, value);
        }
        public ObservableCollection<string> CurrentTasks
        {
            get => _currentTasks;
            set => SetProperty(ref _currentTasks, value);
        }
        public string TaskName
        {
            get => _taskName;
            set => SetProperty(ref _taskName, value);
        }
        public int CompletedPomodoroCount
        {
            get => _completedPomodoroCount;
            set => SetProperty(ref _completedPomodoroCount, value);
        }
        public bool IsCompleted => CurrentTasks == CompletedTasks;
        public bool IsTaskImportant(string task)
        {
            return ImportantTasks.Contains(task);
        }

        public ToDoListWindowViewModel()
        {
            CurrentTasks = AllTasks;
            LoadTasks();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _remainingTime = TimeSpan.FromMinutes(25);
            LoadPomodoroCount();
        }

        [RelayCommand]
        private void ShowAllTasks() => CurrentTasks = AllTasks;

        [RelayCommand]
        private void ShowImportantTasks() => CurrentTasks = ImportantTasks;

        [RelayCommand]
        private void ShowCompletedTasks() => CurrentTasks = CompletedTasks;

        private async void LoadTasks()
        {
            var loadedTasks = await TaskPersistence.LoadTasksAsync();
            var loadedImportantTasks = await TaskPersistence.LoadImportantTasksAsync();
            var loadedCompletedTasks = await TaskPersistence.LoadCompletedTasksAsync();

            AllTasks.Clear();
            ImportantTasks.Clear();
            CompletedTasks.Clear();

            foreach (var task in loadedTasks)
            {
                AllTasks.Add(task);
            }

            foreach (var task in loadedImportantTasks)
            {
                ImportantTasks.Add(task);
            }

            foreach (var task in loadedCompletedTasks)
            {
                CompletedTasks.Add(task);
            }
        }
        
        [RelayCommand]
        private async void AddTask()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(TaskName))
                {
                    AllTasks.Add(TaskName);
                    await TaskPersistence.SaveTasksAsync(AllTasks);
                    TaskName = string.Empty;
                }
            }
            catch (Exception e)
            {
                
            }
        }

        [RelayCommand]
        private async Task ToggleImportant(string task)
        {
            if (ImportantTasks.Contains(task))
            {
                ImportantTasks.Remove(task);
                await TaskPersistence.SaveImportantTasksAsync(ImportantTasks);
            }
            else if (!CompletedTasks.Contains(task))
            {
                ImportantTasks.Add(task);
                await TaskPersistence.SaveImportantTasksAsync(ImportantTasks);
            }
            
            OnPropertyChanged($"IsTaskImportant_{task}");
        }
        
        [RelayCommand]
        private async void CompleteTask(string task)
        {
            try
            {
                if (CurrentTasks == CompletedTasks)
                {
                    await DeleteTask(task);
                }
                else
                {
                    if (AllTasks.Contains(task))
                    {
                        AllTasks.Remove(task);
                    }
                    if (ImportantTasks.Contains(task))
                    {
                        ImportantTasks.Remove(task);
                        await TaskPersistence.SaveImportantTasksAsync(ImportantTasks);
                    }
                    CompletedTasks.Add(task);
                    await TaskPersistence.SaveTasksAsync(AllTasks);
                    await TaskPersistence.SaveCompletedTasksAsync(CompletedTasks);
                }
            }
            catch (Exception e)
            {
                
            }
        }
        
        [RelayCommand]
        private async Task RestoreTask(string task)
        {
            if (CompletedTasks.Contains(task))
            {
                CompletedTasks.Remove(task);
                AllTasks.Add(task);
        
                await TaskPersistence.SaveTasksAsync(AllTasks);
                await TaskPersistence.SaveCompletedTasksAsync(CompletedTasks);
            }
        }
        
        private async Task DeleteTask(string task)
        {
            AllTasks.Remove(task);
            ImportantTasks.Remove(task);
            CompletedTasks.Remove(task);
            
            await TaskPersistence.SaveTasksAsync(AllTasks);
            await TaskPersistence.SaveImportantTasksAsync(ImportantTasks);
            await TaskPersistence.SaveCompletedTasksAsync(CompletedTasks);
        }
        
        private void Timer_Tick(object? sender, EventArgs e)
        {
            _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
            CountdownDisplay = $"{_remainingTime.Minutes:D2}:{_remainingTime.Seconds:D2}";
    
            if (_remainingTime <= TimeSpan.Zero)
            {
                _timer.Stop();
                _remainingTime = TimeSpan.FromMinutes(25);
                CountdownDisplay = "25:00";
            
                CompletedPomodoroCount++;
                _ = TaskPersistence.SavePomodoroCountAsync(CompletedPomodoroCount);
            }
        }
        
        [RelayCommand]
        private void StartCountdown()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                _remainingTime = TimeSpan.FromMinutes(25);
                CountdownDisplay = "25:00";
            }
            else
            {
                _timer.Start();
            }
        }
        
        private async void LoadPomodoroCount()
        {
            CompletedPomodoroCount = await TaskPersistence.LoadPomodoroCountAsync();
        }
    }
}