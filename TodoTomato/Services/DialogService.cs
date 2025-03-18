using System.Threading.Tasks;
using Avalonia.Controls;
using TodoTomato.ViewModels;
using TodoTomato.Views;

namespace TodoTomato.Services
{
    public class DialogService : IDialogService
    {
        private readonly Window _mainWindow;

        public DialogService(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }
        
        public async Task<string?> ShowInputDialogAsync(string title, string message)
        {
            var dialog = new InputDialog
            {
                DataContext = new InputDialogViewModel(title, message)
            };

            var result = await dialog.ShowDialog<bool?>(_mainWindow);
            return result == true ? dialog.ResponseText : null;
        }
    }
}