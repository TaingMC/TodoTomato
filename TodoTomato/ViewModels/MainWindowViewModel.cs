using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;

namespace TodoTomato.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ToDoListWindowViewModel ToDoListWindowViewModel { get; }

        public MainWindowViewModel()
        {
            ToDoListWindowViewModel = new ToDoListWindowViewModel();
        }

        public ICommand ShowAllTasksCommand => ToDoListWindowViewModel.ShowAllTasksCommand;
        public ICommand ShowImportantTasksCommand => ToDoListWindowViewModel.ShowImportantTasksCommand;
        public ICommand ShowCompletedTasksCommand => ToDoListWindowViewModel.ShowCompletedTasksCommand;
    }
}