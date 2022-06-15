// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Modules.Modules.ComputerStats;

public class ComputerStatsModule : Module
{
    public override string Title => "Computer Stats";
    public override string Description => "Sends your system stats. Currently CPU, GPU, and RAM";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => VRCOSCColour.PurpleLight;
    public override ModuleType ModuleType => ModuleType.General;
    public override double DeltaUpdate => 5000d;

    public override void CreateAttributes()
    {
        CreateOutputParameter(ComputerStatsParameter.CPUUsage, "CPU Usage", "CPU usage 0-1", "/avatar/parameters/StatsCPUUsage");
        CreateOutputParameter(ComputerStatsParameter.GPUUsage, "GPU Usage", "GPU usage 0-1", "/avatar/parameters/StatsGPUUsage");
        CreateOutputParameter(ComputerStatsParameter.RAMUsage, "RAM Usage", "RAM usage 0-1", "/avatar/parameters/StatsRAMUsage");
    }

    public override void Update()
    {
        sendCpuUsage();
        sendGpuUsage();
        sendRamUsage();
    }

    private void sendCpuUsage()
    {
        Task.Run(async () =>
        {
            var usage = await ComputerStatsProvider.GetCpuUsage();
            SendParameter(ComputerStatsParameter.CPUUsage, usage);
        }).ConfigureAwait(false);
    }

    private void sendGpuUsage()
    {
        Task.Run(async () =>
        {
            var usage = await ComputerStatsProvider.GetGpuUsage();
            SendParameter(ComputerStatsParameter.GPUUsage, usage);
        }).ConfigureAwait(false);
    }

    private void sendRamUsage()
    {
        Task.Run(async () =>
        {
            var usage = await ComputerStatsProvider.GetRamUsage();
            SendParameter(ComputerStatsParameter.RAMUsage, usage);
        }).ConfigureAwait(false);
    }
}

public enum ComputerStatsParameter
{
    CPUUsage,
    GPUUsage,
    RAMUsage
}
