// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.Modules.Modules.HardwareStats;

public sealed class HardwareStatsProvider : IDisposable
{
    private PerformanceCounter? cpuUsageProvider;
    private IEnumerable<PerformanceCounter>? gpuUsageProviders;
    private ManagementObjectSearcher? ramUsageProvider;

    public HardwareStatsProvider()
    {
        Task.Run(() =>
        {
            initCpu();
            initGpu();
            initRam();
        });
    }

    private void initCpu()
    {
        cpuUsageProvider = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    }

    private void initGpu()
    {
        var localGpuUsageProviders = new List<PerformanceCounter>();

        var category = new PerformanceCounterCategory("GPU Engine");

        foreach (string counterName in category.GetInstanceNames().Where(counterName => counterName.EndsWith("engtype_3D")))
        {
            localGpuUsageProviders.AddRange(category.GetCounters(counterName).Where(counter => counter.CounterName == "Utilization Percentage"));
        }

        gpuUsageProviders = localGpuUsageProviders;
    }

    private void initRam()
    {
        ramUsageProvider = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
    }

    public float GetCpuUsage()
    {
        if (cpuUsageProvider is null) return 0f;

        return cpuUsageProvider.NextValue() / 100f;
    }

    public float GetGpuUsage()
    {
        if (gpuUsageProviders is null) return 0f;

        return gpuUsageProviders.Aggregate(0f, (current, gpuUsageProvider) => current + gpuUsageProvider.NextValue()) / 100f;
    }

    public float GetRamUsage()
    {
        if (ramUsageProvider is null) return 0f;

        var memoryValues = ramUsageProvider.Get().Cast<ManagementObject>().Select(mo => new
        {
            FreePhysicalMemory = double.Parse(mo["FreePhysicalMemory"].ToString() ?? "0"),
            TotalVisibleMemorySize = double.Parse(mo["TotalVisibleMemorySize"].ToString() ?? "0")
        }).FirstOrDefault();

        if (memoryValues is null) return 0f;

        return (float)((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize);
    }

    public void Dispose()
    {
        cpuUsageProvider?.Dispose();
        gpuUsageProviders?.ForEach(gpuUsageProvider => gpuUsageProvider.Dispose());
        ramUsageProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
