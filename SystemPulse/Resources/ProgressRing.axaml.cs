using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SystemPulse;

public class ProgressRing : RangeBase {
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
            ProgressBar.IsIndeterminateProperty.AddOwner<ProgressRing>();

    public static readonly StyledProperty<bool> PreserveAspectProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(PreserveAspect), true);

    public static readonly StyledProperty<double> ValueAngleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(ValueAngle), 0);

    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(StartAngle), 0);

    public static readonly StyledProperty<double> SweepAngleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(SweepAngle), 360);

    static ProgressRing() {
        ValueProperty.Changed.AddClassHandler<ProgressRing>(OnValuePropertyChanged);

        StartAngleProperty.Changed.AddClassHandler<ProgressRing>(OnValuePropertyChanged);
        SweepAngleProperty.Changed.AddClassHandler<ProgressRing>(OnValuePropertyChanged);
        MinimumProperty.Changed.AddClassHandler<ProgressRing>(OnValuePropertyChanged);
        MaximumProperty.Changed.AddClassHandler<ProgressRing>(OnValuePropertyChanged);
    }

    public ProgressRing() {
        UpdatePseudoClasses(IsIndeterminate, PreserveAspect);
    }

    public bool IsIndeterminate {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public bool PreserveAspect {
        get => GetValue(PreserveAspectProperty);
        set => SetValue(PreserveAspectProperty, value);
    }

    public double ValueAngle {
        get => GetValue(ValueAngleProperty);
        private set => SetValue(ValueAngleProperty, value);
    }

    public double StartAngle {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public double SweepAngle {
        get => GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);
        var e = change as AvaloniaPropertyChangedEventArgs<bool>;
        if (e is null) return;

        if (e.Property == IsIndeterminateProperty) {
            UpdatePseudoClasses(e.NewValue.GetValueOrDefault(), null);
        } else if (e.Property == PreserveAspectProperty) {
            UpdatePseudoClasses(null, e.NewValue.GetValueOrDefault());
        }
    }

    private void UpdatePseudoClasses(
        bool? isIndeterminate,
        bool? preserveAspect) {
        if (isIndeterminate.HasValue) {
            PseudoClasses.Set(":indeterminate", isIndeterminate.Value);
        }

        if (preserveAspect.HasValue) {
            PseudoClasses.Set(":preserveaspect", preserveAspect.Value);
        }
    }

    static void OnValuePropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e) {
        sender.ValueAngle = (sender.Value - sender.Minimum) * sender.SweepAngle / (sender.Maximum - sender.Minimum);
    }
}