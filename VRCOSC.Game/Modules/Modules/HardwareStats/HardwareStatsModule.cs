// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Modules.Modules.HardwareStats;

public sealed class HardwareStatsModule : Module
{
    public override string Title => "Hardware Stats";
    public override string Description => "Sends your hardware stats";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => 5000;

    private HardwareStatsProvider hardwareStatsProvider = null!;

    protected override void CreateAttributes()
    {
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.CPUUsage, "CPU Usage", "CPU usage 0-1", "/avatar/parameters/VRCOSC/Hardware/CPUUsage");
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.GPUUsage, "GPU Usage", "GPU usage 0-1", "/avatar/parameters/VRCOSC/Hardware/GPUUsage");
        CreateOutgoingParameter(HardwareStatsOutgoingParameter.RAMUsage, "RAM Usage", "RAM usage 0-1", "/avatar/parameters/VRCOSC/Hardware/RAMUsage");
    }

    protected override void OnStart()
    {
        hardwareStatsProvider = new HardwareStatsProvider();
    }

    protected override void OnUpdate()
    {
        SendParameter(HardwareStatsOutgoingParameter.CPUUsage, hardwareStatsProvider.GetCpuUsage());
        SendParameter(HardwareStatsOutgoingParameter.GPUUsage, hardwareStatsProvider.GetGpuUsage());
        SendParameter(HardwareStatsOutgoingParameter.RAMUsage, hardwareStatsProvider.GetRamUsage());
    }

    protected override void OnStop()
    {
        hardwareStatsProvider.Dispose();
    }

    private enum HardwareStatsOutgoingParameter
    {
        CPUUsage,
        GPUUsage,
        RAMUsage
    }
}
