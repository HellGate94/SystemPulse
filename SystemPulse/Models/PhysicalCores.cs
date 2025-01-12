using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SystemPulse.Models;
public partial class PhysicalCores(int coreId) : ObservableObject {
    [ObservableProperty]
    private int _coreId = coreId;

    [ObservableProperty]
    private ObservableCollection<LogicalCore> _logicalCores = new();
}
