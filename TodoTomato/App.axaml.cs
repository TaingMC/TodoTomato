using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TodoTomato.Services;
using TodoTomato.ViewModels;
using TodoTomato.Views;

namespace TodoTomato
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = new ServiceCollection();
                
                var mainWindow = new MainWindow();
                services.AddSingleton(mainWindow);
                services.AddSingleton<IDialogService>(new DialogService(mainWindow));
                services.AddSingleton<MainWindowViewModel>();

                var serviceProvider = services.BuildServiceProvider();

                mainWindow.DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}