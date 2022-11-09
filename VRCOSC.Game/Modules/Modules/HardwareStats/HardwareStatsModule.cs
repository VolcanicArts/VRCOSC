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

        CreateParameter<float>(HardwareStatsOutgoingParameter.CpuUsage, ParameterMode.Write, "VRCOSC/Hardware/CPUUsage", "The CPU usage normalised");
        CreateParameter<float>(HardwareStatsOutgoingParameter.GpuUsage, ParameterMode.Write, "VRCOSC/Hardware/GPUUsage", "The GPU usage normalised");
        CreateParameter<float>(HardwareStatsOutgoingParameter.RamUsage, ParameterMode.Write, "VRCOSC/Hardware/RAMUsage", "The RAM usage normalised");
        CreateParameter<int>(HardwareStatsOutgoingParameter.CpuTemp, ParameterMode.Write, "VRCOSC/Hardware/CPUTemp", "The CPU temp in C");
        CreateParameter<int>(HardwareStatsOutgoingParameter.GpuTemp, ParameterMode.Write, "VRCOSC/Hardware/GPUTemp", "The GPU temp in C ");
        CreateParameter<int>(HardwareStatsOutgoingParameter.RamTotal, ParameterMode.Write, "VRCOSC/Hardware/RAMTotal", "The total amount of RAM in GB");
        CreateParameter<int>(HardwareStatsOutgoingParameter.RamUsed, ParameterMode.Write, "VRCOSC/Hardware/RAMUsed", "The used RAM in GB");
        CreateParameter<int>(HardwareStatsOutgoingParameter.RamAvailable, ParameterMode.Write, "VRCOSC/Hardware/RAMAvailable", "The available RAM in GB");
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
