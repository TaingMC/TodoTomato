using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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
        public static ObservableCollection<NamedCommand> CustomCollectionCommands { get; } = new();
        public Dictionary<string, ObservableCollection<string>> CustomCollections { get; } = new();
        private readonly Dictionary<string, ObservableCollection<string>> _taskOriginalCollections = new();
        private ObservableCollection<string> _currentTasks;
        private int _completedPomodoroCount;
        private string _taskName;
        private DispatcherTimer _timer;
        private TimeSpan _remainingTime;
        private string _countdownDisplay = "25:00";
        private string _focusButtonText = "开始专注";
        public string FocusButtonText
        {
            get => _focusButtonText;
            set => SetProperty(ref _focusButtonText, value);
        }
        public void CreateCustomCollectionCommand(string collectionName)
        {
            var command = new NamedCommand(collectionName, () => CurrentTasks = CustomCollections[collectionName]);
            CustomCollectionCommands.Add(command);
        }
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
            LoadCustomCollections();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _remainingTime = TimeSpan.FromMinutes(25);
            LoadPomodoroCount();
            FocusButtonText = "开始专注";
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
        
        private async void LoadCustomCollections()
        {
            try
            {
                var loadedCollections = await TaskPersistence.LoadCustomCollectionsAsync();
                CustomCollections.Clear();
                CustomCollectionCommands.Clear();

                foreach (var kvp in loadedCollections)
                {
                    CustomCollections.Add(kvp.Key, kvp.Value);
                    CreateCustomCollectionCommand(kvp.Key);
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }
        
        [RelayCommand]
        private async void AddTask()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(TaskName))
                {
                    if (CurrentTasks == ImportantTasks)
                    {
                        AllTasks.Add(TaskName);
                        ImportantTasks.Add(TaskName);
                        await TaskPersistence.SaveTasksAsync(AllTasks);
                        await TaskPersistence.SaveImportantTasksAsync(ImportantTasks);
                    }
                    else if (CurrentTasks == CompletedTasks)
                    {
                        CompletedTasks.Add(TaskName);
                        await TaskPersistence.SaveCompletedTasksAsync(CompletedTasks);
                    }
                    else if (CustomCollections.ContainsValue(CurrentTasks))
                    {
                        CurrentTasks.Add(TaskName);
                        AllTasks.Add(TaskName);
                        await TaskPersistence.SaveTasksAsync(AllTasks);
                        await TaskPersistence.SaveCustomCollectionsAsync(CustomCollections);
                    }
                    else
                    {
                        AllTasks.Add(TaskName);
                        await TaskPersistence.SaveTasksAsync(AllTasks);
                    }
            
                    TaskName = string.Empty;
                }
            }
            catch (Exception e)
            {
                // Handle error
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
                    return;
                }

                if (CurrentTasks != AllTasks)
                {
                    _taskOriginalCollections[task] = CurrentTasks;
                }

                if (AllTasks.Contains(task))
                {
                    AllTasks.Remove(task);
                }
                if (ImportantTasks.Contains(task))
                {
                    ImportantTasks.Remove(task);
                }
                foreach (var collection in CustomCollections.Values)
                {
                    if (collection.Contains(task))
                    {
                        collection.Remove(task);
                    }
                }

                CompletedTasks.Add(task);

                await TaskPersistence.SaveTasksAsync(AllTasks);
                await TaskPersistence.SaveImportantTasksAsync(ImportantTasks);
                await TaskPersistence.SaveCompletedTasksAsync(CompletedTasks);
                await TaskPersistence.SaveCustomCollectionsAsync(CustomCollections);
            }
            catch (Exception)
            {
                // Handle error
            }
        }

        [RelayCommand]
        private async Task RestoreTask(string task)
        {
            if (!CompletedTasks.Contains(task)) return;

            CompletedTasks.Remove(task);

            if (_taskOriginalCollections.TryGetValue(task, out var originalCollection))
            {
                originalCollection.Add(task);
                
                if (originalCollection != AllTasks)
                {
                    AllTasks.Add(task);
                }

                _taskOriginalCollections.Remove(task);
                
                await TaskPersistence.SaveTasksAsync(AllTasks);
                if (originalCollection == ImportantTasks)
                {
                    await TaskPersistence.SaveImportantTasksAsync(ImportantTasks);
                }
            }
            else
            {
                AllTasks.Add(task);
                await TaskPersistence.SaveTasksAsync(AllTasks);
            }

            await TaskPersistence.SaveCompletedTasksAsync(CompletedTasks);
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
        
        private async Task SaveCustomCollections()
        {
            try
            {
                await TaskPersistence.SaveCustomCollectionsAsync(CustomCollections);
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }
        
        public async Task SaveCustomCollectionsAsync()
        {
            try
            {
                await TaskPersistence.SaveCustomCollectionsAsync(CustomCollections);
            }
            catch (Exception ex)
            {
                // Handle error
            }
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
                FocusButtonText = "开始专注"; // Add this line

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
                FocusButtonText = "开始专注";
            }
            else
            {
                _timer.Start();
                FocusButtonText = "暂停专注";
            }
        }
        
        private async void LoadPomodoroCount()
        {
            CompletedPomodoroCount = await TaskPersistence.LoadPomodoroCountAsync();
        }
        
        [RelayCommand]
        private void ResetCountdown()
        {
            _timer.Stop();
            _remainingTime = TimeSpan.FromMinutes(25);
            CountdownDisplay = "25:00";
            FocusButtonText = "开始专注";
        }
    }
}