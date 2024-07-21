// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using VRCOSC.App.Utils;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.SDK.Providers.Hardware;

public sealed class HardwareStatsProvider
{
    private readonly Computer computer = new()
    {
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true
    };

    private readonly Regex hardwareIDRegex = new(".+/([0-9])");
    private readonly Regex sensorIDRegex = new(".+/([0-9])/.+");

    public bool CanAcceptQueries { get; private set; }

    public readonly Dictionary<int, CPU> CPUs = new();
    public readonly Dictionary<int, GPU> GPUs = new();
    public RAM? RAM { get; private set; }

    public void Init() => Task.Run(() =>
    {
        computer.Open();
        CanAcceptQueries = true;
    });

    public void Shutdown() => Task.Run(() =>
    {
        CanAcceptQueries = false;
        CPUs.Clear();
        GPUs.Clear();
        RAM = null;

        computer.Close();
    });

    public Task Update() => Task.Run(() =>
    {
        computer.Hardware.ForEach(hardware =>
        {
            updateHardware(hardware);
            auditHardware(hardware);

            hardware.Sensors.ForEach(sensor =>
            {
                var identifier = sensor.Identifier.ToString()!;

                if (identifier.Contains("ram", StringComparison.InvariantCultureIgnoreCase))
                {
                    RAM!.Update(sensor);
                    return;
                }

                var sensorIdMatch = sensorIDRegex.Match(identifier);
                if (!sensorIdMatch.Success) return;

                var sensorId = int.Parse(sensorIdMatch.Groups[1].Value);

                if (identifier.Contains("cpu", StringComparison.InvariantCultureIgnoreCase))
                {
                    CPUs[sensorId].Update(sensor);
                    return;
                }

                if (identifier.Contains("gpu", StringComparison.InvariantCultureIgnoreCase))
                {
                    GPUs[sensorId].Update(sensor);
                }
            });
        });
    });

    private void updateHardware(IHardware hardware)
    {
        hardware.Update();
        hardware.SubHardware.ForEach(updateHardware);
    }

    private void auditHardware(IHardware hardware)
    {
        var identifier = hardware.Identifier.ToString()!;

        if (identifier.Contains("ram", StringComparison.InvariantCultureIgnoreCase))
        {
            RAM ??= new RAM();
            return;
        }

        var hardwareIDMatch = hardwareIDRegex.Match(identifier);
        if (!hardwareIDMatch.Success) return;

        var hardwareID = int.Parse(hardwareIDMatch.Groups[1].Value);

        if (identifier.Contains("cpu", StringComparison.InvariantCultureIgnoreCase))
        {
            if (identifier.Contains("intel", StringComparison.InvariantCultureIgnoreCase))
            {
                CPUs.TryAdd(hardwareID, new IntelCPU());
                return;
            }

            if (identifier.Contains("amd", StringComparison.InvariantCultureIgnoreCase))
            {
                CPUs.TryAdd(hardwareID, new AMDCPU());
                return;
            }
        }

        if (identifier.Contains("gpu", StringComparison.InvariantCultureIgnoreCase))
        {
            if (identifier.Contains("nvidia", StringComparison.InvariantCultureIgnoreCase))
            {
                GPUs.TryAdd(hardwareID, new NvidiaGPU());
                return;
            }

            if (identifier.Contains("amd", StringComparison.InvariantCultureIgnoreCase))
            {
                GPUs.TryAdd(hardwareID, new AMDGPU());
            }
        }
    }
}
