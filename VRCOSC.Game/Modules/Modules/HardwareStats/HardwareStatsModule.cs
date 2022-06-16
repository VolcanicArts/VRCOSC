// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Modules.Modules.ComputerStats;

public class HardwareStatsModule : Module
{
    public override string Title => "Hardware Stats";
    public override string Description => "Sends your hardware stats";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => VRCOSCColour.PurpleLight;
    public override ModuleType ModuleType => ModuleType.General;
    public override double DeltaUpdate => 1000d;

    private HardwareStatsProvider hardwareStatsProvider;

    public override void CreateAttributes()
    {
        CreateOutputParameter(HardwareStatsParameter.CPUUsage, "CPU Usage", "CPU usage 0-1", "/avatar/parameters/HSCPUUsage");
        CreateOutputParameter(HardwareStatsParameter.GPUUsage, "GPU Usage", "GPU usage 0-1", "/avatar/parameters/HSGPUUsage");
        CreateOutputParameter(HardwareStatsParameter.RAMUsage, "RAM Usage", "RAM usage 0-1", "/avatar/parameters/HSRAMUsage");
    }

    public override void Start()
    {
        hardwareStatsProvider = new HardwareStatsProvider();
    }

    public override void Update()
    {
        SendParameter(HardwareStatsParameter.CPUUsage, hardwareStatsProvider.GetCpuUsage());
        SendParameter(HardwareStatsParameter.GPUUsage, hardwareStatsProvider.GetGpuUsage());
        SendParameter(HardwareStatsParameter.RAMUsage, hardwareStatsProvider.GetRamUsage());
    }

    public override void Stop()
    {
        hardwareStatsProvider.Dispose();
    }
}

public enum HardwareStatsParameter
{
    CPUUsage,
    GPUUsage,
    RAMUsage
}
