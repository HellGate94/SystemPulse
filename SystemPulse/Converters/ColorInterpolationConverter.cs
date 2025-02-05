using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SystemPulse.Converters;
public class ColorInterpolationConverter : IMultiValueConverter {
    private readonly SolidColorBrush _colorBrush = new(Colors.Black);

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
        if (values.Count < 3 ||
            values[0] is not double currentValue ||
            values[1] is not double maxValue ||
            values[2] is not Color lowColor ||
            values[3] is not Color highColor) {
            return _colorBrush;
        }

        if (maxValue > 0) {
            double t = Math.Clamp(currentValue / maxValue, 0d, 1d);
            _colorBrush.Color = InterpolateColor(lowColor, highColor, t);
        }

        return _colorBrush;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    private static Color InterpolateColor(Color low, Color high, double t) {
        return new Color(
            255,
            (byte)(low.R + (high.R - low.R) * t),
            (byte)(low.G + (high.G - low.G) * t),
            (byte)(low.B + (high.B - low.B) * t)
        );
    }
}