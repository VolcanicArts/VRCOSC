// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics.Colour;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Modules.Modules.HardwareStats;

public class HardwareStatsModule : Module
{
    public override string Title => "Hardware Stats";
    public override string Description => "Sends your hardware stats";
    public override string Author => "VolcanicArts";
    public override ColourInfo Colour => VRCOSCColour.GrayA;
    public override ModuleType ModuleType => ModuleType.General;
    protected override double DeltaUpdate => 5000d;

    private HardwareStatsProvider hardwareStatsProvider;

    protected override void CreateAttributes()
    {
        CreateOutputParameter(HardwareStatsOutputParameter.CPUUsage, "CPU Usage", "CPU usage 0-1", "/avatar/parameters/HSCPUUsage");
        CreateOutputParameter(HardwareStatsOutputParameter.GPUUsage, "GPU Usage", "GPU usage 0-1", "/avatar/parameters/HSGPUUsage");
        CreateOutputParameter(HardwareStatsOutputParameter.RAMUsage, "RAM Usage", "RAM usage 0-1", "/avatar/parameters/HSRAMUsage");
    }

    protected override void OnStart()
    {
        hardwareStatsProvider = new HardwareStatsProvider();
    }

    protected override void OnUpdate()
    {
        SendParameter(HardwareStatsOutputParameter.CPUUsage, hardwareStatsProvider.GetCpuUsage());
        SendParameter(HardwareStatsOutputParameter.GPUUsage, hardwareStatsProvider.GetGpuUsage());
        SendParameter(HardwareStatsOutputParameter.RAMUsage, hardwareStatsProvider.GetRamUsage());
    }

    protected override void OnStop()
    {
        hardwareStatsProvider.Dispose();
    }

    private enum HardwareStatsOutputParameter
    {
        CPUUsage,
        GPUUsage,
        RAMUsage
    }
}
