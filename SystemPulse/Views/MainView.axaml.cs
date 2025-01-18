using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SystemPulse.Views;

public partial class MainView : UserControl {
    public MainView() {
        InitializeComponent();
        if (!Design.IsDesignMode)
            DataContext = Ioc.Default.GetService<ViewModels.MainViewModel>();
    }
    private void LabelPointerReleasedHandler(object sender, PointerReleasedEventArgs args) {
        TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync((sender as Label)!.Content as string);
    }
}
