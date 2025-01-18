using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Cpu;
using System.Collections.ObjectModel;
using System.Linq;

namespace SystemPulse.Models.Hardware;

public record LogicalCore(int CoreId, SensorItem LoadValue);
public record PhysicalCore(int CoreId) {
    public ObservableCollection<LogicalCore> LogicalCores { get; } = [];
}
public class CpuHardwareItem : HardwareItem {
    public ObservableCollection<PhysicalCore> PhysicalCores { get; } = [];

    public CpuHardwareItem(IHardware hardware) : base(hardware) {
        var cpu = (GenericCpu)hardware;

        var cpuLoadSensors = Sensors
            .Where(s => s.SensorType == SensorType.Load)
            .OfType<SensorItem>()
            .Where(s => s.Index >= 2) // 0 is total load, 1 is core max
            .ToArray();

        for (int coreIdx = 0; coreIdx < cpu.CpuId.Length; coreIdx++) {
            var pcpu = cpu.CpuId[coreIdx];
            PhysicalCore physicalCore = new(coreIdx);
            for (int threadIdx = 0; threadIdx < pcpu.Length; threadIdx++) {
                var thread = cpu.CpuId[coreIdx][threadIdx];
                LogicalCore logicalCore = new(thread.Thread, cpuLoadSensors[thread.Thread]);
                physicalCore.LogicalCores.Add(logicalCore);
            }
            PhysicalCores.Add(physicalCore);
        }
    }
}
