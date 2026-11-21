using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using System;

namespace SystemPulse.Controls;

public static class ToolTipHelper {
    public static readonly AttachedProperty<object?> TipProperty =
        AvaloniaProperty.RegisterAttached<Control, object?>("Tip", typeof(ToolTipHelper));

    private static readonly AttachedProperty<LazyToolTipHost?> HostProperty =
        AvaloniaProperty.RegisterAttached<Control, LazyToolTipHost?>("Host", typeof(ToolTipHelper));

    static ToolTipHelper() {
        TipProperty.Changed.AddClassHandler<Control>(OnTipChanged);
    }

    public static object? GetTip(Control element) => element.GetValue(TipProperty);
    public static void SetTip(Control element, object? value) => element.SetValue(TipProperty, value);

    private static void OnTipChanged(Control control, AvaloniaPropertyChangedEventArgs e) {
        if (e.OldValue is IDataTemplate) {
            control.RemoveHandler(ToolTip.ToolTipOpeningEvent, OnToolTipOpening);
            control.ClearValue(HostProperty);
        }

        if (e.NewValue is IDataTemplate) {
            var host = new LazyToolTipHost(control);
            control.SetValue(HostProperty, host);
            ToolTip.SetTip(control, host);
            control.AddHandler(ToolTip.ToolTipOpeningEvent, OnToolTipOpening);
        } else {
            ToolTip.SetTip(control, e.NewValue);
        }
    }

    private static void OnToolTipOpening(object? sender, CancelRoutedEventArgs e) {
        if (sender is not Control control || control.GetValue(HostProperty) is not { } host) {
            return;
        }

        if (control.DataContext is null) {
            e.Cancel = true;
            return;
        }

        host.Populate();
    }

    private sealed class LazyToolTipHost(Control owner) : ContentControl {
        private readonly WeakReference<Control> _owner = new(owner);

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            Populate();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            Content = null;
            ContentTemplate = null;
        }

        public void Populate() {
            if (Content is null &&
                _owner.TryGetTarget(out var owner) &&
                owner.DataContext is { } dataContext &&
                GetTip(owner) is IDataTemplate template) {
                ContentTemplate = template;
                Content = dataContext;
            }
        }
    }
}