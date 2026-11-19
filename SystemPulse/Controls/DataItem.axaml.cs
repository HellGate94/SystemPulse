using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace SystemPulse.Controls;

[PseudoClasses(":alert")]
public class DataItem : TemplatedControl {
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<DataItem, string?>(nameof(Label));

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<DataItem, object?>(nameof(Icon));

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

    public static readonly StyledProperty<string?> DisplayValueProperty =
        AvaloniaProperty.Register<DataItem, string?>(nameof(DisplayValue));

    public static readonly StyledProperty<bool> IsAlertProperty =
        AvaloniaProperty.Register<DataItem, bool>(nameof(IsAlert));

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

    public string? DisplayValue {
        get => GetValue(DisplayValueProperty);
        private set => SetValue(DisplayValueProperty, value);
    }

    public string? FormattedValue {
        get => ValueText;
        set => ValueText = value;
    }

    public bool IsAlert {
        get => GetValue(IsAlertProperty);
        set => SetValue(IsAlertProperty, value);
    }

    public object? ProgressToolTip {
        get => GetValue(ProgressToolTipProperty);
        set => SetValue(ProgressToolTipProperty, value);
    }

    static DataItem() {
        ValueProperty.Changed.AddClassHandler<DataItem>((item, _) => item.UpdateDisplayValue());
        ValueFormatProperty.Changed.AddClassHandler<DataItem>((item, _) => item.UpdateDisplayValue());
        ValueTextProperty.Changed.AddClassHandler<DataItem>((item, _) => item.UpdateDisplayValue());
    }

    public DataItem() {
        UpdateDisplayValue();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);

        if (change.Property == IsAlertProperty) {
            PseudoClasses.Set(":alert", change.GetNewValue<bool>());
        }
    }

    private void UpdateDisplayValue() {
        if (ValueText is not null) {
            SetCurrentValue(DisplayValueProperty, ValueText);
        } else if (!string.IsNullOrEmpty(ValueFormat)) {
            try {
                SetCurrentValue(DisplayValueProperty, string.Format(ValueFormat, Value));
            } catch {
                SetCurrentValue(DisplayValueProperty, Value.ToString());
            }
        } else {
            SetCurrentValue(DisplayValueProperty, Value.ToString());
        }
    }
}