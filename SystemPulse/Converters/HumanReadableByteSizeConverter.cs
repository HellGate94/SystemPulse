using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SystemPulse.Converters;
public class HumanReadableByteSizeConverter : IValueConverter {
    private static readonly string[] MetricNames = ["", "K", "M", "G", "T", "P", "E", "Z", "Y", "R", "Q"];
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        ulong bytes = System.Convert.ToUInt64(value);
        int basisIndex = parameter != null ? System.Convert.ToInt32(parameter) : 0;
        bytes *= (ulong)Math.Pow(1024, basisIndex);
        return ClosestMetricString(bytes, 1024, "0.00", null);
    }

    private static string ClosestMetricString(ulong value, ulong basis, string? format, IFormatProvider? provider) {
        double num;
        int metricid;
        if (value == 0)
            (num, metricid) = (0d, 0);
        else
            (num, metricid) = ClosestMetric(value, basis);

        return $"{num.ToString(format, provider)} {MetricNames[metricid]}";
    }

    private static (double num, int metricid) ClosestMetric(ulong value, ulong basis) {
        int place = (int)Math.Floor(Math.Log(value, basis));
        double num = value / Math.Pow(basis, place);
        return (num, place);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
