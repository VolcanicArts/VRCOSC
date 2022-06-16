// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace VRCOSC.Game.Modules.Modules.ComputerStats;

public class HardwareStatsProvider : IDisposable
{
    private PerformanceCounter cpuUsageProvider;
    private List<PerformanceCounter> gpuUsageProviders;
    private ManagementObjectSearcher ramUsageProvider;

    public HardwareStatsProvider()
    {
        initCpu();
        initGpu();
        initRam();
    }

    private void initCpu()
    {
        cpuUsageProvider = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    }

    private void initGpu()
    {
        gpuUsageProviders = new List<PerformanceCounter>();

        var category = new PerformanceCounterCategory("GPU Engine");

        foreach (string counterName in category.GetInstanceNames().Where(counterName => counterName.EndsWith("engtype_3D")))
        {
            gpuUsageProviders.AddRange(category.GetCounters(counterName).Where(counter => counter.CounterName == "Utilization Percentage"));
        }
    }

    private void initRam()
    {
        ramUsageProvider = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
    }

    public float GetCpuUsage()
    {
        return cpuUsageProvider.NextValue() / 100f;
    }

    public float GetGpuUsage()
    {
        float usage = 0f;
        gpuUsageProviders.ForEach(x => usage += x.NextValue());
        return usage / 100f;
    }

    public float GetRamUsage()
    {
        var memoryValues = ramUsageProvider.Get().Cast<ManagementObject>().Select(mo => new
        {
            FreePhysicalMemory = double.Parse(mo["FreePhysicalMemory"].ToString() ?? "0"),
            TotalVisibleMemorySize = double.Parse(mo["TotalVisibleMemorySize"].ToString() ?? "0")
        }).FirstOrDefault();

        if (memoryValues == null) return 0f;

        return (float)((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize);
    }

    public void Dispose()
    {
        cpuUsageProvider.Dispose();
        gpuUsageProviders.ForEach(gpuUsageProvider => gpuUsageProvider.Dispose());
        ramUsageProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
