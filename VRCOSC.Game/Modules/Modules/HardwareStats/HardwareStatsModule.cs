// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;

namespace VRCOSC.Game.Modules.Modules.HardwareStats;

public sealed partial class HardwareStatsModule : ChatBoxModule
{
    public override string Title => "Hardware Stats";
    public override string Description => "Sends hardware stats and displays them in the ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;
    protected override int DeltaUpdate => 500;

    protected override bool DefaultChatBoxDisplay => true;
    protected override string DefaultChatBoxFormat => "CPU: $cpuusage$% | GPU: $gpuusage$%                RAM: $ramused$GB/$ramtotal$GB";
    protected override IEnumerable<string> ChatBoxFormatValues => new[] { "$cpuusage$ (%)", "$gpuusage$ (%)", "$ramusage$ (%)", "$cputemp$ (C)", "$gputemp$ (C)", "$ramtotal$ (GB)", "$ramused$ (GB)", "$ramavailable$ (GB)" };

    private HardwareStatsProvider? hardwareStatsProvider;

    protected override void CreateAttributes()
    {
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

        return GetSetting<string>(ChatBoxSetting.ChatBoxFormat)
               .Replace("$cpuusage$", hardwareStatsProvider.CpuUsage.ToString("0.00"))
               .Replace("$gpuusage$", hardwareStatsProvider.GpuUsage.ToString("0.00"))
               .Replace("$ramusage$", hardwareStatsProvider.RamUsage.ToString("0.00"))
               .Replace("$cputemp$", hardwareStatsProvider.CpuTemp.ToString())
               .Replace("$gputemp$", hardwareStatsProvider.GpuTemp.ToString())
               .Replace("$ramtotal$", hardwareStatsProvider.RamTotal.ToString("0.0"))
               .Replace("$ramused$", hardwareStatsProvider.RamUsed.ToString("0.0"))
               .Replace("$ramavailable$", hardwareStatsProvider.RamAvailable.ToString("0.0"));
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

        SendParameter(HardwareStatsParameter.CpuUsage, hardwareStatsProvider.CpuUsage / 100f);
        SendParameter(HardwareStatsParameter.GpuUsage, hardwareStatsProvider.GpuUsage / 100f);
        SendParameter(HardwareStatsParameter.RamUsage, hardwareStatsProvider.RamUsage / 100f);
        SendParameter(HardwareStatsParameter.CpuTemp, hardwareStatsProvider.CpuTemp);
        SendParameter(HardwareStatsParameter.GpuTemp, hardwareStatsProvider.GpuTemp);
        SendParameter(HardwareStatsParameter.RamTotal, hardwareStatsProvider.RamTotal);
        SendParameter(HardwareStatsParameter.RamUsed, hardwareStatsProvider.RamUsed);
        SendParameter(HardwareStatsParameter.RamAvailable, hardwareStatsProvider.RamAvailable);
    }

    protected override void OnModuleStop()
    {
        hardwareStatsProvider = null;
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
