// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;

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
        foreach (IHardware hardware in computer.Hardware)
        {
            hardware.Update();

            foreach (IHardware subHardware in hardware.SubHardware)
            {
                foreach (ISensor sensor in subHardware.Sensors)
                {
                    handleSensor(sensor);
                }
            }

            foreach (ISensor sensor in hardware.Sensors)
            {
                handleSensor(sensor);
            }
        }
    }

    private void handleSensor(ISensor sensor)
    {
        switch (sensor.Name)
        {
            case "CPU Total":
                CpuUsage = sensor.Value ?? 0f;
                break;

            case "D3D 3D":
                GpuUsage = sensor.Value ?? 0f;
                break;

            case "Memory Used":
                RamUsage = sensor.Value ?? 0f;
                break;
        }
    }

    public float CpuUsage { get; private set; }
    public float GpuUsage { get; private set; }
    public float RamUsage { get; private set; }
}
