using Avalonia;
using Avalonia.Controls.Primitives;

namespace SystemPulse.Controls;

public class DataItem : TemplatedControl {
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<DataItem, string?>(nameof(Label));

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<DataItem, object?>(nameof(Icon));

    public static readonly StyledProperty<bool> IsWarningProperty =
        AvaloniaProperty.Register<DataItem, bool>(nameof(IsWarning));

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<DataItem, double>(nameof(Value));

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<DataItem, double>(nameof(Minimum), 0.0);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<DataItem, double>(nameof(Maximum), 100.0);

    public static readonly StyledProperty<string?> ValueFormatProperty =
        AvaloniaProperty.Register<DataItem, string?>(nameof(ValueFormat));

    public static readonly StyledProperty<string?> ValueTextProperty =
        AvaloniaProperty.Register<DataItem, string?>(nameof(ValueText));

    public static readonly StyledProperty<string?> ProgressValueProperty =
        AvaloniaProperty.Register<DataItem, string?>(nameof(ProgressValue));

    public static readonly StyledProperty<string?> LabelValueProperty =
        AvaloniaProperty.Register<DataItem, string?>(nameof(LabelValue));

    public static readonly StyledProperty<object?> SecondaryIconProperty =
        AvaloniaProperty.Register<DataItem, object?>(nameof(SecondaryIcon));

    public static readonly StyledProperty<object?> LabelValueToolTipProperty =
        AvaloniaProperty.Register<DataItem, object?>(nameof(LabelValueToolTip));

    public static readonly StyledProperty<object?> ProgressToolTipProperty =
        AvaloniaProperty.Register<DataItem, object?>(nameof(ProgressToolTip));

    public string? Label {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public object? Icon {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsWarning {
        get => GetValue(IsWarningProperty);
        set => SetValue(IsWarningProperty, value);
    }

    public double Value {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public string? ValueFormat {
        get => GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    public string? ValueText {
        get => GetValue(ValueTextProperty);
        set => SetValue(ValueTextProperty, value);
    }

    public string? ProgressValue {
        get => GetValue(ProgressValueProperty);
        private set => SetValue(ProgressValueProperty, value);
    }

    public string? LabelValue {
        get => GetValue(LabelValueProperty);
        set => SetValue(LabelValueProperty, value);
    }

    public object? SecondaryIcon {
        get => GetValue(SecondaryIconProperty);
        set => SetValue(SecondaryIconProperty, value);
    }

    public object? LabelValueToolTip {
        get => GetValue(LabelValueToolTipProperty);
        set => SetValue(LabelValueToolTipProperty, value);
    }

    public string? FormattedValue {
        get => ValueText;
        set => ValueText = value;
    }

    public object? ProgressToolTip {
        get => GetValue(ProgressToolTipProperty);
        set => SetValue(ProgressToolTipProperty, value);
    }

    static DataItem() {
        ValueProperty.Changed.AddClassHandler<DataItem>((item, _) => item.UpdateProgressValue());
        ValueFormatProperty.Changed.AddClassHandler<DataItem>((item, _) => item.UpdateProgressValue());
        ValueTextProperty.Changed.AddClassHandler<DataItem>((item, _) => item.UpdateProgressValue());
    }

    public DataItem() {
        UpdateProgressValue();
    }

    private void UpdateProgressValue() {
        if (ValueText is not null) {
            SetCurrentValue(ProgressValueProperty, ValueText);
        } else if (!string.IsNullOrEmpty(ValueFormat)) {
            try {
                SetCurrentValue(ProgressValueProperty, string.Format(ValueFormat, Value));
            } catch {
                SetCurrentValue(ProgressValueProperty, Value.ToString());
            }
        } else {
            SetCurrentValue(ProgressValueProperty, null);
        }
    }
}