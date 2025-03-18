namespace TodoTomato.ViewModels
{
    public class InputDialogViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Message { get; set; }

        public InputDialogViewModel(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}