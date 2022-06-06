// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
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
    public override ModuleType Type => ModuleType.General;
    public override double DeltaUpdate => 5000d;

    protected override Dictionary<Enum, (string, string, string)> OutputParameters => new()
    {
        { ComputerStatsParameter.CPUUsage, ("CPU Usage", "The current usage of your CPU from 0 to 1", "/avatar/parameters/CPUUsage") },
        { ComputerStatsParameter.GPUUsage, ("GPU Usage", "The current usage of your GPU from 0 to 1", "/avatar/parameters/GPUUsage") },
        { ComputerStatsParameter.RAMUsage, ("RAM Usage", "The current usage of your RAM from 0 to 1", "/avatar/parameters/RAMUsage") }
    };

    protected override void OnUpdate()
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
            Terminal.Log($"Current CPU Usage: {usage * 100f}%");
            SendParameter(ComputerStatsParameter.CPUUsage, usage);
        }).ConfigureAwait(false);
    }

    private void sendGpuUsage()
    {
        Task.Run(async () =>
        {
            var usage = await ComputerStatsProvider.GetGpuUsage();
            Terminal.Log($"Current GPU Usage: {usage * 100f}%");
            SendParameter(ComputerStatsParameter.GPUUsage, usage);
        }).ConfigureAwait(false);
    }

    private void sendRamUsage()
    {
        Task.Run(async () =>
        {
            var usage = await ComputerStatsProvider.GetRamUsage();
            Terminal.Log($"Current RAM Usage: {usage * 100f}%");
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
