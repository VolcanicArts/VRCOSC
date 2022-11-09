// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules.Modules.HardwareStats;

public sealed class HardwareStatsModule : Module
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
        CreateSetting(HardwareStatsSetting.UseChatBox, "Use ChatBox", "Should values be displayed in the ChatBox?", true);
        CreateSetting(HardwareStatsSetting.ChatBoxFormat, "ChatBox Format", format_description, "CPU: $cpuusage$% | GPU: $gpuusage$% | RAM: $ramusage$%");

        CreateOutgoingParameter<float>(HardwareStatsOutgoingParameter.CpuUsage, "The CPU usage normalised", "VRCOSC/Hardware/CPUUsage");
        CreateOutgoingParameter<float>(HardwareStatsOutgoingParameter.GpuUsage, "The GPU usage normalised", "VRCOSC/Hardware/GPUUsage");
        CreateOutgoingParameter<float>(HardwareStatsOutgoingParameter.RamUsage, "The RAM usage normalised", "VRCOSC/Hardware/RAMUsage");
        CreateOutgoingParameter<int>(HardwareStatsOutgoingParameter.CpuTemp, "The CPU temp in C", "VRCOSC/Hardware/CPUTemp");
        CreateOutgoingParameter<int>(HardwareStatsOutgoingParameter.GpuTemp, "The GPU temp in C ", "VRCOSC/Hardware/GPUTemp");
        CreateOutgoingParameter<int>(HardwareStatsOutgoingParameter.RamTotal, "The total amount of RAM in GB", "VRCOSC/Hardware/RAMTotal");
        CreateOutgoingParameter<int>(HardwareStatsOutgoingParameter.RamUsed, "The used RAM in GB", "VRCOSC/Hardware/RAMUsed");
        CreateOutgoingParameter<int>(HardwareStatsOutgoingParameter.RamAvailable, "The available RAM in GB", "VRCOSC/Hardware/RAMAvailable");
    }

    protected override void OnStart()
    {
        hardwareStatsProvider = new HardwareStatsProvider();
        Log("Loading hardware monitors...");
    }

    protected override void OnUpdate()
    {
        if (hardwareStatsProvider is null) throw new NullReferenceException();

        if (!hardwareStatsProvider.CanAcceptQueries) return;

        hardwareStatsProvider.Update();

        SendParameter(HardwareStatsOutgoingParameter.CpuUsage, hardwareStatsProvider.CpuUsage / 100f);
        SendParameter(HardwareStatsOutgoingParameter.GpuUsage, hardwareStatsProvider.GpuUsage / 100f);
        SendParameter(HardwareStatsOutgoingParameter.RamUsage, hardwareStatsProvider.RamUsage / 100f);
        SendParameter(HardwareStatsOutgoingParameter.CpuTemp, hardwareStatsProvider.CpuTemp);
        SendParameter(HardwareStatsOutgoingParameter.GpuTemp, hardwareStatsProvider.GpuTemp);
        SendParameter(HardwareStatsOutgoingParameter.RamTotal, hardwareStatsProvider.RamTotal);
        SendParameter(HardwareStatsOutgoingParameter.RamUsed, hardwareStatsProvider.RamUsed);
        SendParameter(HardwareStatsOutgoingParameter.RamAvailable, hardwareStatsProvider.RamAvailable);

        if (GetSetting<bool>(HardwareStatsSetting.UseChatBox)) updateChatBox();
    }

    private void updateChatBox()
    {
        var text = GetSetting<string>(HardwareStatsSetting.ChatBoxFormat)
                   .Replace("$cpuusage$", (hardwareStatsProvider!.CpuUsage).ToString("0.00"))
                   .Replace("$gpuusage$", (hardwareStatsProvider!.GpuUsage).ToString("0.00"))
                   .Replace("$ramusage$", (hardwareStatsProvider!.RamUsage).ToString("0.00"))
                   .Replace("$cputemp$", (hardwareStatsProvider!.CpuTemp).ToString())
                   .Replace("$gputemp$", (hardwareStatsProvider!.GpuTemp).ToString())
                   .Replace("$ramtotal$", (hardwareStatsProvider!.RamTotal).ToString("0.0"))
                   .Replace("$ramused$", (hardwareStatsProvider!.RamUsed).ToString("0.0"))
                   .Replace("$ramavailable$", (hardwareStatsProvider!.RamAvailable).ToString("0.0"));

        SetChatBoxText(text);
    }

    protected override void OnStop()
    {
        hardwareStatsProvider = null;
        ClearChatBox();
    }

    private enum HardwareStatsOutgoingParameter
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
