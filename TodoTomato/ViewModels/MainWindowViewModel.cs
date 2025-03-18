using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TodoTomato.Services;

namespace TodoTomato.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ToDoListWindowViewModel ToDoListWindowViewModel { get; }
        private readonly IDialogService _dialogService;

        public MainWindowViewModel(IDialogService dialogService)
        {
            ToDoListWindowViewModel = new ToDoListWindowViewModel();
            _dialogService = dialogService;
        }

        public ICommand ShowAllTasksCommand => ToDoListWindowViewModel.ShowAllTasksCommand;
        public ICommand ShowImportantTasksCommand => ToDoListWindowViewModel.ShowImportantTasksCommand;
        public ICommand ShowCompletedTasksCommand => ToDoListWindowViewModel.ShowCompletedTasksCommand;
        public ObservableCollection<NamedCommand> CustomCollectionCommands => ToDoListWindowViewModel.CustomCollectionCommands;

        [RelayCommand]
        private async Task CreateNewCollection()
        {
            var collectionName = await _dialogService.ShowInputDialogAsync("新建自定义列表", "请输入新自定义列表的名称:");
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                if (ToDoListWindowViewModel.CustomCollections.ContainsKey(collectionName))
                {
                    await _dialogService.ShowInputDialogAsync("Error", "已存在同名自定义列表。");
                    return;
                }

                var newCollection = new ObservableCollection<string>();
                ToDoListWindowViewModel.CustomCollections.Add(collectionName, newCollection);
                ToDoListWindowViewModel.CreateCustomCollectionCommand(collectionName);
                await ToDoListWindowViewModel.SaveCustomCollectionsAsync(); // Add this line
            }
        }
        
        [RelayCommand]
        private async Task DeleteCustomCollection()
        {
            if (ToDoListWindowViewModel.CustomCollections.Count == 0)
            {
                await _dialogService.ShowInputDialogAsync("Error", "没有可以删除的自定义列表.");
                return;
            }

            var collectionNames = ToDoListWindowViewModel.CustomCollections.Keys.ToList();
            var collectionToDelete = await _dialogService.ShowInputDialogAsync("删除自定义列表", 
                "请输入需要删除的自定义列表的名称:");

            if (!string.IsNullOrWhiteSpace(collectionToDelete) && 
                ToDoListWindowViewModel.CustomCollections.ContainsKey(collectionToDelete))
            {
                ToDoListWindowViewModel.CustomCollections.Remove(collectionToDelete);
                var commandToRemove = ToDoListWindowViewModel.CustomCollectionCommands
                    .FirstOrDefault(c => c.Name == collectionToDelete);
                if (commandToRemove != null)
                {
                    ToDoListWindowViewModel.CustomCollectionCommands.Remove(commandToRemove);
                }
                await ToDoListWindowViewModel.SaveCustomCollectionsAsync();
            }
            else
            {
                await _dialogService.ShowInputDialogAsync("Error", "未找到此自定义列表.");
            }
        }
    }
}