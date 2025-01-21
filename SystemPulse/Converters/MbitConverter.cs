using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SystemPulse.Converters;
public class MbitConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        var val = System.Convert.ToDouble(value);
        return val / (1000000f / 8f);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        var val = System.Convert.ToDouble(value);
        return val * (1000000f / 8f);
    }
}
