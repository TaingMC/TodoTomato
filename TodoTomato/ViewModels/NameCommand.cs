using System;
using System.Windows.Input;

namespace TodoTomato.ViewModels;

public class NamedCommand : ICommand
{
    private readonly Action _execute;
    public string Name { get; }

    public NamedCommand(string name, Action execute)
    {
        Name = name;
        _execute = execute;
    }

    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged;
}