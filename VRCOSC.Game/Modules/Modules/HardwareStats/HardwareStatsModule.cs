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

    private HardwareStatsProvider? hardwareStatsProvider;

    protected override void CreateAttributes()
    {
        CreateSetting(HardwareStatsSetting.UseChatBox, "Use ChatBox", "Enable HardwareStats to show values in the ChatBox", true);
        CreateSetting(HardwareStatsSetting.ChatBoxFormat, "ChatBox Format", "How the information should be displayed in the ChatBox", "CPU: $cpuusage$% | GPU: $gpuusage$% | RAM: $ramusage$%");

        CreateOutgoingParameter(HardwareStatsOutgoingParameter.CpuUsage, "CPU Usage", "The CPU usage normalised", "/avatar/parameters/VRCOSC/Hardware/CPUUsage");
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.GpuUsage, "GPU Usage", "The GPU usage normalised", "/avatar/parameters/VRCOSC/Hardware/GPUUsage");
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.RamUsage, "RAM Usage", "The RAM usage normalised", "/avatar/parameters/VRCOSC/Hardware/RAMUsage");
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.CpuTemp, "CPU Temp", "The CPU temp in C", "/avatar/parameters/VRCOSC/Hardware/CPUTemp");
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.GpuTemp, "GPU Temp", "The GPU temp in C ", "/avatar/parameters/VRCOSC/Hardware/GPUTemp");
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.RamTotal, "RAM Total", "The total amount of RAM in your system in GB", "/avatar/parameters/VRCOSC/Hardware/RAMTotal");
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.RamUsed, "RAM Used", "The used RAM in GB", "/avatar/parameters/VRCOSC/Hardware/RAMUsed");
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.RamAvailable, "RAM Available", "The available RAM in GB", "/avatar/parameters/VRCOSC/Hardware/RAMAvailable");
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
                   .Replace("$cpuusage$", (hardwareStatsProvider!.CpuUsage).ToString("#.##"))
                   .Replace("$gpuusage$", (hardwareStatsProvider!.GpuUsage).ToString("#.##"))
                   .Replace("$ramusage$", (hardwareStatsProvider!.RamUsage).ToString("#.##"))
                   .Replace("$cputemp$", (hardwareStatsProvider!.CpuTemp).ToString())
                   .Replace("$gputemp$", (hardwareStatsProvider!.GpuTemp).ToString())
                   .Replace("$ramtotal$", (hardwareStatsProvider!.RamTotal).ToString("#.##"))
                   .Replace("$ramused$", (hardwareStatsProvider!.RamUsed).ToString("#.##"))
                   .Replace("$ramavailable$", (hardwareStatsProvider!.RamAvailable).ToString("#.##"));

        SetChatBoxText(text);
    }

    protected override void OnStop()
    {
        hardwareStatsProvider = null;
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
