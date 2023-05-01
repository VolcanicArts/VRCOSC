// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.Providers.Hardware;

public sealed class HardwareStatsProvider
{
    private readonly Computer computer = new()
    {
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true
    };

    public bool CanAcceptQueries { get; private set; }

    private readonly List<HardwareComponent> components = new();

    public CPU? GetCPU(int id) => components.Where(component => component.GetType().IsSubclassOf(typeof(CPU))).Select(component => (CPU)component).SingleOrDefault(cpu => cpu.Id == id);
    public GPU? GetGPU(int id) => components.Where(component => component.GetType() == typeof(GPU)).Select(component => (GPU)component).SingleOrDefault(gpu => gpu.Id == id);
    public RAM? GetRam() => components.Where(component => component.GetType() == typeof(RAM)).Select(component => (RAM)component).SingleOrDefault();

    public void Init()
    {
        Task.Run(() =>
        {
            computer.Open();
            CanAcceptQueries = true;
        }).ConfigureAwait(false);
    }

    public void Shutdown()
    {
        Task.Run(() =>
        {
            CanAcceptQueries = false;
            components.Clear();
            computer.Close();
        }).ConfigureAwait(false);
    }

    public void Update()
    {
        computer.Hardware.ForEach(hardware =>
        {
            updateHardware(hardware);
            auditHardware(hardware);
            hardware.Sensors.ForEach(sensor => components.ForEach(component => component.Update(sensor)));
        });
    }

    private void updateHardware(IHardware hardware)
    {
        hardware.Update();
        hardware.SubHardware.ForEach(updateHardware);
    }

    private void auditHardware(IHardware hardware)
    {
        var address = hardware.Identifier.ToString();
        var index = 0;

        try
        {
            index = int.Parse(address.Split('/').Last());
        }
        catch (FormatException) { }

        if (address.Contains("cpu", StringComparison.InvariantCultureIgnoreCase))
        {
            var cpu = GetCPU(index);

            if (cpu is null)
            {
                if (address.Contains("intel", StringComparison.InvariantCultureIgnoreCase))
                {
                    components.Add(new IntelCPU(index));
                }

                if (address.Contains("amd", StringComparison.InvariantCultureIgnoreCase))
                {
                    components.Add(new AMDCPU(index));
                }
            }
        }

        if (address.Contains("gpu", StringComparison.InvariantCultureIgnoreCase))
        {
            var gpu = GetGPU(index);

            if (gpu is null)
            {
                components.Add(new GPU(index));
            }
        }

        if (address.Contains("ram", StringComparison.InvariantCultureIgnoreCase))
        {
            var ram = GetRam();

            if (ram is null)
            {
                components.Add(new RAM());
            }
        }
    }
}
