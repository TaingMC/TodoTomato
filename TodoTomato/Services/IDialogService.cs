using System.Threading.Tasks;
using Avalonia.Controls;

namespace TodoTomato.Services
{
    public interface IDialogService
    {
        Task<string?> ShowInputDialogAsync(string title, string message);
    }
}