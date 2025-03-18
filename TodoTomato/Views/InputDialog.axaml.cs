using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TodoTomato.ViewModels;

namespace TodoTomato.Views
{
    public partial class InputDialog : Window
    {
        public InputDialog()
        {
            InitializeComponent();

            var okButton = this.FindControl<Button>("OkButton");
            var cancelButton = this.FindControl<Button>("CancelButton");

            okButton!.Click += (_, _) => Close(true);
            cancelButton!.Click += (_, _) => Close(false);
        }

        public string? ResponseText
        {
            get => this.FindControl<TextBox>("ResponseTextBox")?.Text;
            set
            {
                if (this.FindControl<TextBox>("ResponseTextBox") is TextBox textBox)
                    textBox.Text = value;
            }
        }
    }
}