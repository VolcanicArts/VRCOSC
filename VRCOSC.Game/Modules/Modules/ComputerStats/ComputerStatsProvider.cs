// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.ComputerStats;

public static class ComputerStatsProvider
{
    public static async Task<float> GetCpuUsage()
    {
        try
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var _ = cpuCounter.NextValue();
            await Task.Delay(1000);
            var usage = cpuCounter.NextValue();
            return MathF.Round(usage) / 100f;
        }
        catch
        {
            return 0f;
        }
    }

    public static async Task<float> GetGpuUsage()
    {
        try
        {
            var category = new PerformanceCounterCategory("GPU Engine");
            var counterNames = category.GetInstanceNames();
            var gpuCounters = new List<PerformanceCounter>();
            var usage = 0f;

            foreach (string counterName in counterNames)
            {
                if (!counterName.EndsWith("engtype_3D")) continue;

                gpuCounters.AddRange(category.GetCounters(counterName).Where(counter => counter.CounterName == "Utilization Percentage"));
            }

            gpuCounters.ForEach(x => _ = x.NextValue());
            await Task.Delay(1000);
            gpuCounters.ForEach(x => usage += x.NextValue());

            return MathF.Round(usage) / 100f;
        }
        catch
        {
            return 0f;
        }
    }

    public static async Task<float> GetRamUsage()
    {
        var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

        var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
        {
            FreePhysicalMemory = double.Parse(mo["FreePhysicalMemory"].ToString() ?? string.Empty),
            TotalVisibleMemorySize = double.Parse(mo["TotalVisibleMemorySize"].ToString() ?? string.Empty)
        }).FirstOrDefault();

        await Task.Delay(1000);

        if (memoryValues == null) return 0f;

        var percentageUsage = (float)((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100f;
        return MathF.Round(percentageUsage) / 100f;
    }
}
