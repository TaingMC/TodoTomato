using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TodoTomato.ViewModels;

namespace TodoTomato;

public class BooleanToColorConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2 || values[0] is not ToDoListWindowViewModel viewModel || values[1] is null)
            return new SolidColorBrush(Colors.Gray);

        var task = values[1].ToString();
        return viewModel.IsTaskImportant(task)
            ? new SolidColorBrush(Colors.Gold)
            : new SolidColorBrush(Colors.Gray);
    }

    public object[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}