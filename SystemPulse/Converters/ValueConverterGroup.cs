﻿using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SystemPulse.Converters;
public class ValueConverterGroup : List<IValueConverter>, IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return this.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}