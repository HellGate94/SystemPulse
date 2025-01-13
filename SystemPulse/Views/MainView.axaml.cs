using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SystemPulse.Views;

public partial class MainView : UserControl {
    public MainView() {
        InitializeComponent();
        if (!Design.IsDesignMode)
            DataContext = Ioc.Default.GetService<ViewModels.MainViewModel>();
    }
}
