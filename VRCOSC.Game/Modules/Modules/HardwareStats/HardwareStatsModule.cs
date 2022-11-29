// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.HardwareStats;

public sealed class HardwareStatsModule : ChatBoxModule
{
    public override string Title => "Hardware Stats";
    public override string Description => "Sends hardware stats and displays them in the ChatBox";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => 2000;
    protected override int ChatBoxPriority => 1;

    private const string format_description = "How the information should be displayed in the ChatBox.\n"
                                              + "Available values: $cpuusage$ (%), $gpuusage$ (%), $ramusage$ (%), "
                                              + "$cputemp$ (C), $gputemp$ (C), $ramtotal$ (GB), $ramused$ (GB), $ramavailable$ (GB)";

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

    protected override string GetChatBoxText()
    {
        if (!hardwareStatsProvider!.CanAcceptQueries) return string.Empty;

        return GetSetting<string>(HardwareStatsSetting.ChatBoxFormat)
               .Replace("$cpuusage$", (hardwareStatsProvider!.CpuUsage).ToString("0.00"))
               .Replace("$gpuusage$", (hardwareStatsProvider!.GpuUsage).ToString("0.00"))
               .Replace("$ramusage$", (hardwareStatsProvider!.RamUsage).ToString("0.00"))
               .Replace("$cputemp$", (hardwareStatsProvider!.CpuTemp).ToString())
               .Replace("$gputemp$", (hardwareStatsProvider!.GpuTemp).ToString())
               .Replace("$ramtotal$", (hardwareStatsProvider!.RamTotal).ToString("0.0"))
               .Replace("$ramused$", (hardwareStatsProvider!.RamUsed).ToString("0.0"))
               .Replace("$ramavailable$", (hardwareStatsProvider!.RamAvailable).ToString("0.0"));
    }

    protected override async Task OnStart(CancellationToken cancellationToken)
    {
        await base.OnStart(cancellationToken);
        hardwareStatsProvider = new HardwareStatsProvider();

        Log("Loading hardware monitors...");

        while (!hardwareStatsProvider.CanAcceptQueries)
        {
            await Task.Delay(1, cancellationToken);
        }

        Log("Hardware monitors loaded!");
    }

    protected override Task OnUpdate()
    {
        if (!hardwareStatsProvider!.CanAcceptQueries) return Task.CompletedTask;

        hardwareStatsProvider.Update();

        SendParameter(HardwareStatsParameter.CpuUsage, hardwareStatsProvider.CpuUsage / 100f);
        SendParameter(HardwareStatsParameter.GpuUsage, hardwareStatsProvider.GpuUsage / 100f);
        SendParameter(HardwareStatsParameter.RamUsage, hardwareStatsProvider.RamUsage / 100f);
        SendParameter(HardwareStatsParameter.CpuTemp, hardwareStatsProvider.CpuTemp);
        SendParameter(HardwareStatsParameter.GpuTemp, hardwareStatsProvider.GpuTemp);
        SendParameter(HardwareStatsParameter.RamTotal, hardwareStatsProvider.RamTotal);
        SendParameter(HardwareStatsParameter.RamUsed, hardwareStatsProvider.RamUsed);
        SendParameter(HardwareStatsParameter.RamAvailable, hardwareStatsProvider.RamAvailable);

        return Task.CompletedTask;
    }

    protected override async Task OnStop()
    {
        await base.OnStop();
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

    private enum HardwareStatsSetting
    {
        UseChatBox,
        ChatBoxFormat
    }
}
