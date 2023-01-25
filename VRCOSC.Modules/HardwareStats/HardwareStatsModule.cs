// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.HardwareStats;

public sealed partial class HardwareStatsModule : ChatBoxModule
{
    public override string Title => "Hardware Stats";
    public override string Description => "Sends hardware stats and displays them in the ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;
    protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(0.5);

    protected override string DefaultChatBoxFormat => @"CPU: $cpuusage$% | GPU: $gpuusage$%                RAM: $ramused$GB/$ramtotal$GB";
    protected override IEnumerable<string> ChatBoxFormatValues => new[] { @"$cpuusage$ (%)", @"$gpuusage$ (%)", @"$ramusage$ (%)", @"$cputemp$ (C)", @"$gputemp$ (C)", @"$ramtotal$ (GB)", @"$ramused$ (GB)", @"$ramavailable$ (GB)" };

    private HardwareStatsProvider? hardwareStatsProvider;

    protected override void CreateAttributes()
    {
        CreateSetting(HardwareStatsSetting.SelectedCPU, "Selected CPU", "If you have multiple CPUs, enter the (0th based) index of the one you want to track", 0);
        CreateSetting(HardwareStatsSetting.SelectedGPU, "Selected GPU", "If you have multiple GPUs, enter the (0th based) index of the one you want to track", 0);

        base.CreateAttributes();

        CreateParameter<float>(HardwareStatsParameter.CpuUsage, ParameterMode.Write, "VRCOSC/Hardware/CPUUsage", "The CPU usage normalised");
        CreateParameter<float>(HardwareStatsParameter.GpuUsage, ParameterMode.Write, "VRCOSC/Hardware/GPUUsage", "The GPU usage normalised");
        CreateParameter<float>(HardwareStatsParameter.RamUsage, ParameterMode.Write, "VRCOSC/Hardware/RAMUsage", "The RAM usage normalised");
        CreateParameter<int>(HardwareStatsParameter.CpuTemp, ParameterMode.Write, "VRCOSC/Hardware/CPUTemp", "The CPU temp in C");
        CreateParameter<int>(HardwareStatsParameter.GpuTemp, ParameterMode.Write, "VRCOSC/Hardware/GPUTemp", "The GPU temp in C ");
        CreateParameter<int>(HardwareStatsParameter.RamTotal, ParameterMode.Write, "VRCOSC/Hardware/RAMTotal", "The total amount of RAM in GB");
        CreateParameter<int>(HardwareStatsParameter.RamUsed, ParameterMode.Write, "VRCOSC/Hardware/RAMUsed", "The used RAM in GB");
        CreateParameter<int>(HardwareStatsParameter.RamAvailable, ParameterMode.Write, "VRCOSC/Hardware/RAMAvailable", "The available RAM in GB");
    }

    protected override string? GetChatBoxText()
    {
        if (!(hardwareStatsProvider?.CanAcceptQueries ?? false)) return null;

        try
        {
            var cpu = hardwareStatsProvider.CPUs[GetSetting<int>(HardwareStatsSetting.SelectedCPU)];
            var gpu = hardwareStatsProvider.GPUs[GetSetting<int>(HardwareStatsSetting.SelectedGPU)];
            var ram = hardwareStatsProvider.RAM;

            return GetSetting<string>(ChatBoxSetting.ChatBoxFormat)
                   .Replace(@"$cpuusage$", cpu.Usage.ToString("0.00"))
                   .Replace(@"$gpuusage$", gpu.Usage.ToString("0.00"))
                   .Replace(@"$ramusage$", ram.Usage.ToString("0.00"))
                   .Replace(@"$cputemp$", cpu.Temperature.ToString())
                   .Replace(@"$gputemp$", gpu.Temperature.ToString())
                   .Replace(@"$ramtotal$", ram.Total.ToString("0.0"))
                   .Replace(@"$ramused$", ram.Used.ToString("0.0"))
                   .Replace(@"$ramavailable$", ram.Available.ToString("0.0"));
        }
        catch { }

        return null;
    }

    protected override void OnModuleStart()
    {
        base.OnModuleStart();
        hardwareStatsProvider = new HardwareStatsProvider();
    }

    protected override void OnModuleUpdate()
    {
        if (!(hardwareStatsProvider?.CanAcceptQueries ?? false)) return;

        hardwareStatsProvider.Update();

        try
        {
            var cpu = hardwareStatsProvider.CPUs[GetSetting<int>(HardwareStatsSetting.SelectedCPU)];
            var gpu = hardwareStatsProvider.GPUs[GetSetting<int>(HardwareStatsSetting.SelectedGPU)];
            var ram = hardwareStatsProvider.RAM;

            SendParameter(HardwareStatsParameter.CpuUsage, cpu.Usage / 100f);
            SendParameter(HardwareStatsParameter.GpuUsage, gpu.Usage / 100f);
            SendParameter(HardwareStatsParameter.RamUsage, ram.Usage / 100f);
            SendParameter(HardwareStatsParameter.CpuTemp, cpu.Temperature);
            SendParameter(HardwareStatsParameter.GpuTemp, gpu.Temperature);
            SendParameter(HardwareStatsParameter.RamTotal, ram.Total);
            SendParameter(HardwareStatsParameter.RamUsed, ram.Used);
            SendParameter(HardwareStatsParameter.RamAvailable, ram.Available);
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
        RamAvailable
    }
}
