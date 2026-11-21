using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SystemPulse.Converters;

public enum ComparisonOperator {
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
}

public class ThresholdConverter : IValueConverter, IMultiValueConverter {
    public ComparisonOperator Operator { get; set; } = ComparisonOperator.GreaterThanOrEqual;
    public double Threshold { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (!TryToDouble(value, out double actual)) {
            return false;
        }

        double limit = parameter is not null && TryToDouble(parameter, out double paramLimit)
            ? paramLimit
            : Threshold;

        return Compare(actual, limit);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
        if (values.Count < 2 || !TryToDouble(values[0], out double actual) || !TryToDouble(values[1], out double limit)) {
            return false;
        }

        return Compare(actual, limit);
    }

    private bool Compare(double actual, double limit) => Operator switch {
        ComparisonOperator.GreaterThan => actual > limit,
        ComparisonOperator.GreaterThanOrEqual => actual >= limit,
        ComparisonOperator.LessThan => actual < limit,
        ComparisonOperator.LessThanOrEqual => actual <= limit,
        _ => false,
    };

    private static bool TryToDouble(object? value, out double result) {
        switch (value) {
            case float f:
                result = f;
                return !float.IsNaN(f);
            case double d:
                result = d;
                return !double.IsNaN(d);
            case int i:
                result = i;
                return true;
            case null:
                result = 0;
                return false;
            default:
                if (value == AvaloniaProperty.UnsetValue) {
                    result = 0;
                    return false;
                }
                try {
                    result = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    return !double.IsNaN(result);
                } catch {
                    result = 0;
                    return false;
                }
        }
    }
}