// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules.Modules.HardwareStats;

public sealed class HardwareStatsModule : Module
{
    public override string Title => "Hardware Stats";
    public override string Description => "Sends your hardware stats";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => 2000;

    private HardwareStatsProvider? hardwareStatsProvider;

    protected override void CreateAttributes()
    {
        CreateSetting(HardwareStatsSetting.UseChatBox, "Use ChatBox", "Enable HardwareStats to show values in the ChatBox", true);
        CreateSetting(HardwareStatsSetting.ChatBoxFormat, "ChatBox Format", "How the information should be displayed in the ChatBox", "CPU: $cpuusage$% | GPU: $gpuusage$% | RAM: $ramusage$%");

        CreateOutgoingParameter(HardwareStatsOutgoingParameter.CPUUsage, "CPU Usage", "CPU usage 0-1", "/avatar/parameters/VRCOSC/Hardware/CPUUsage");
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.GPUUsage, "GPU Usage", "GPU usage 0-1", "/avatar/parameters/VRCOSC/Hardware/GPUUsage");
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.RAMUsage, "RAM Usage", "RAM usage 0-1", "/avatar/parameters/VRCOSC/Hardware/RAMUsage");
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

        SendParameter(HardwareStatsOutgoingParameter.CPUUsage, hardwareStatsProvider.CpuUsage);
        SendParameter(HardwareStatsOutgoingParameter.GPUUsage, hardwareStatsProvider.GpuUsage);
        SendParameter(HardwareStatsOutgoingParameter.RAMUsage, hardwareStatsProvider.RamUsage);

        if (GetSetting<bool>(HardwareStatsSetting.UseChatBox)) updateChatBox();
    }

    private void updateChatBox()
    {
        var text = GetSetting<string>(HardwareStatsSetting.ChatBoxFormat)
                   .Replace("$cpuusage$", (hardwareStatsProvider!.CpuUsage).ToString("#.##"))
                   .Replace("$gpuusage$", (hardwareStatsProvider!.GpuUsage).ToString("#.##"))
                   .Replace("$ramusage$", (hardwareStatsProvider!.RamUsage).ToString("#.##"));

        SetChatBoxText(text);
    }

    protected override void OnStop()
    {
        hardwareStatsProvider = null;
    }

    private enum HardwareStatsOutgoingParameter
    {
        CPUUsage,
        GPUUsage,
        RAMUsage
    }

    private enum HardwareStatsSetting
    {
        UseChatBox,
        ChatBoxFormat
    }
}
