// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.ChatBox;
using VRCOSC.Game.Providers.Hardware;

namespace VRCOSC.Modules.HardwareStats;

public sealed class HardwareStatsModule : ChatBoxModule
{
    public override string Title => "Hardware Stats";
    public override string Description => "Sends hardware stats and displays them in the ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;
    protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(0.5);

    private HardwareStatsProvider? hardwareStatsProvider;

    protected override void CreateAttributes()
    {
        CreateSetting(HardwareStatsSetting.SelectedCPU, "Selected CPU", "If you have multiple CPUs, enter the (0th based) index of the one you want to track", 0);
        CreateSetting(HardwareStatsSetting.SelectedGPU, "Selected GPU", "If you have multiple GPUs, enter the (0th based) index of the one you want to track", 0);

        CreateParameter<float>(HardwareStatsParameter.CpuUsage, ParameterMode.Write, "VRCOSC/Hardware/CPUUsage", "CPU Usage", "The CPU usage normalised");
        CreateParameter<float>(HardwareStatsParameter.GpuUsage, ParameterMode.Write, "VRCOSC/Hardware/GPUUsage", "GPU Usage", "The GPU usage normalised");
        CreateParameter<float>(HardwareStatsParameter.RamUsage, ParameterMode.Write, "VRCOSC/Hardware/RAMUsage", "RAM Usage", "The RAM usage normalised");
        CreateParameter<int>(HardwareStatsParameter.CpuTemp, ParameterMode.Write, "VRCOSC/Hardware/CPUTemp", "CPU Temp", "The CPU temp in C");
        CreateParameter<int>(HardwareStatsParameter.GpuTemp, ParameterMode.Write, "VRCOSC/Hardware/GPUTemp", "GPU Temp", "The GPU temp in C ");
        CreateParameter<int>(HardwareStatsParameter.RamTotal, ParameterMode.Write, "VRCOSC/Hardware/RAMTotal", "RAM Total", "The total amount of RAM in GB");
        CreateParameter<int>(HardwareStatsParameter.RamUsed, ParameterMode.Write, "VRCOSC/Hardware/RAMUsed", "RAM Used", "The used RAM in GB");
        CreateParameter<int>(HardwareStatsParameter.RamAvailable, ParameterMode.Write, "VRCOSC/Hardware/RAMAvailable", "RAM Available", "The available RAM in GB");
        CreateParameter<float>(HardwareStatsParameter.VRamFree, ParameterMode.Write, "VRCOSC/Hardware/VRamFree", @"VRAM Free", @"The amount of free VRAM in GB");
        CreateParameter<float>(HardwareStatsParameter.VRamUsed, ParameterMode.Write, "VRCOSC/Hardware/VRamUsed", @"VRAM Used", @"The amount of used VRAM in GB");
        CreateParameter<float>(HardwareStatsParameter.VRamTotal, ParameterMode.Write, "VRCOSC/Hardware/VRamTotal", @"VRAM Total", @"The amount of total VRAM in GB");

        CreateState(HardwareStatsState.Available, "Available", @"CPU: {cpuusage}% | GPU: {gpuusage}%                RAM: {ramused}GB/{ramtotal}GB");
        CreateState(HardwareStatsState.Unavailable, "Unavailable", string.Empty);

        CreateVariable(HardwareStatsParameter.CpuUsage, @"CPU Usage (%)", @"{cpuusage}");
        CreateVariable(HardwareStatsParameter.GpuUsage, @"GPU Usage (%)", @"{gpuusage}");
        CreateVariable(HardwareStatsParameter.RamUsage, @"RAM Usage (%)", @"{ramusage}");
        CreateVariable(HardwareStatsParameter.CpuTemp, @"CPU Temp (C)", @"{cputemp}");
        CreateVariable(HardwareStatsParameter.GpuTemp, @"GPU Temp (C)", @"{gputemp}");
        CreateVariable(HardwareStatsParameter.RamTotal, @"RAM Total (GB)", @"{ramtotal}");
        CreateVariable(HardwareStatsParameter.RamUsed, @"RAM Used (GB)", @"{ramused}");
        CreateVariable(HardwareStatsParameter.RamAvailable, @"RAM Available (GB)", @"{ramavailable}");
        CreateVariable(HardwareStatsParameter.VRamUsed, @"VRAM Used (GB)", @"{vramused}");
        CreateVariable(HardwareStatsParameter.VRamFree, @"VRAM Free (GB)", @"{vramfree}");
        CreateVariable(HardwareStatsParameter.VRamTotal, @"VRAM Total (GB)", @"{vramtotal}");
    }

    protected override void OnModuleStart()
    {
        base.OnModuleStart();
        hardwareStatsProvider = new HardwareStatsProvider();
    }

    protected override void OnModuleUpdate()
    {
        if (hardwareStatsProvider is null || !hardwareStatsProvider.CanAcceptQueries) return;

        hardwareStatsProvider.Update();

        try
        {
            var cpu = hardwareStatsProvider.Cpus[GetSetting<int>(HardwareStatsSetting.SelectedCPU)];
            var gpu = hardwareStatsProvider.Gpus[GetSetting<int>(HardwareStatsSetting.SelectedGPU)];
            var ram = hardwareStatsProvider.Ram;

            SendParameter(HardwareStatsParameter.CpuUsage, cpu.Usage / 100f);
            SendParameter(HardwareStatsParameter.GpuUsage, gpu.Usage / 100f);
            SendParameter(HardwareStatsParameter.RamUsage, ram.Usage / 100f);
            SendParameter(HardwareStatsParameter.CpuTemp, cpu.Temperature);
            SendParameter(HardwareStatsParameter.GpuTemp, gpu.Temperature);
            SendParameter(HardwareStatsParameter.RamTotal, ram.Total);
            SendParameter(HardwareStatsParameter.RamUsed, ram.Used);
            SendParameter(HardwareStatsParameter.RamAvailable, ram.Available);
            SendParameter(HardwareStatsParameter.VRamFree, gpu.MemoryFree / 1000f);
            SendParameter(HardwareStatsParameter.VRamUsed, gpu.MemoryUsed / 1000f);
            SendParameter(HardwareStatsParameter.VRamTotal, gpu.MemoryTotal / 1000f);

            SetVariableValue(HardwareStatsParameter.CpuUsage, cpu.Usage.ToString("0.00"));
            SetVariableValue(HardwareStatsParameter.GpuUsage, gpu.Usage.ToString("0.00"));
            SetVariableValue(HardwareStatsParameter.RamUsage, ram.Usage.ToString("0.00"));
            SetVariableValue(HardwareStatsParameter.CpuTemp, cpu.Temperature.ToString());
            SetVariableValue(HardwareStatsParameter.GpuTemp, gpu.Temperature.ToString());
            SetVariableValue(HardwareStatsParameter.RamTotal, ram.Total.ToString("0.0"));
            SetVariableValue(HardwareStatsParameter.RamUsed, ram.Used.ToString("0.0"));
            SetVariableValue(HardwareStatsParameter.RamAvailable, ram.Available.ToString("0.0"));
            SetVariableValue(HardwareStatsParameter.VRamFree, (gpu.MemoryFree / 1000f).ToString("0.0"));
            SetVariableValue(HardwareStatsParameter.VRamUsed, (gpu.MemoryUsed / 1000f).ToString("0.0"));
            SetVariableValue(HardwareStatsParameter.VRamTotal, (gpu.MemoryTotal / 1000f).ToString("0.0"));
        }
        catch { }
    }

    protected override void OnModuleStop()
    {
        hardwareStatsProvider = null;
    }

    private enum HardwareStatsSetting
    {
        SelectedCPU,
        SelectedGPU
    }

    private enum HardwareStatsParameter
    {
        CpuUsage,
        GpuUsage,
        RamUsage,
        CpuTemp,
        GpuTemp,
        RamTotal,
        RamUsed,
        RamAvailable,
        VRamFree,
        VRamUsed,
        VRamTotal
    }

    private enum HardwareStatsState
    {
        Available,
        Unavailable
    }
}
