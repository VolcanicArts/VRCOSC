// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.Modules.Modules.HardwareStats;

public sealed class HardwareStatsProvider
{
    private readonly Computer computer;

    public bool CanAcceptQueries { get; private set; }

    public HardwareStatsProvider()
    {
        computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };

        Task.Run(() =>
        {
            computer.Open();
            CanAcceptQueries = true;
        });
    }

    public void Update()
    {
        computer.Hardware.ForEach(updateHardware);
    }

    private void updateHardware(IHardware hardware)
    {
        hardware.Update();
        hardware.SubHardware.ForEach(updateHardware);
        hardware.Sensors.ForEach(handleSensor);
    }

    private void handleSensor(ISensor sensor)
    {
        switch (sensor.SensorType)
        {
            case SensorType.Load:
                switch (sensor.Name)
                {
                    case "CPU Total":
                        CpuUsage = sensor.Value ?? 0f;
                        break;

                    case "GPU Core":
                        GpuUsage = sensor.Value ?? 0f;
                        break;

                    case "Memory":
                        RamUsage = sensor.Value ?? 0f;
                        break;
                }

                break;

            case SensorType.Temperature:
                switch (sensor.Name)
                {
                    case "CPU Package":
                        CpuTemp = (int?)sensor.Value ?? 0;
                        break;

                    case "GPU Core":
                        GpuTemp = (int?)sensor.Value ?? 0;
                        break;
                }

                break;

            case SensorType.Data:
                switch (sensor.Name)
                {
                    case "Memory Used":
                        RamUsed = sensor.Value ?? 0f;
                        break;

                    case "Memory Available":
                        RamAvailable = sensor.Value ?? 0f;
                        break;
                }

                break;
        }
    }

    public float CpuUsage { get; private set; }
    public float GpuUsage { get; private set; }
    public float RamUsage { get; private set; }

    public int CpuTemp { get; private set; }
    public int GpuTemp { get; private set; }

    public float RamUsed { get; private set; }
    public float RamAvailable { get; private set; }
    public float RamTotal => RamUsed + RamAvailable;
}
