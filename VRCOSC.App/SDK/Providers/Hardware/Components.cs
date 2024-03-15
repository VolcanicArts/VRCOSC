// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using LibreHardwareMonitor.Hardware;

namespace VRCOSC.App.SDK.Providers.Hardware;

public record SensorPair(SensorType Type, string Name);

public class SensorInfo
{
    public readonly List<SensorPair> Pairs = new();

    public SensorInfo(SensorType type, params string[] names)
    {
        foreach (var name in names) Pairs.Add(new SensorPair(type, name));
    }
}

public abstract class HardwareComponent
{
    protected virtual SensorInfo LoadInfo => throw new NotImplementedException();

    public float Usage { get; private set; }

    protected static bool GetIntValue(ISensor sensor, SensorInfo info, out int value)
    {
        if (GetFloatValue(sensor, info, out var floatValue))
        {
            value = (int)MathF.Round(floatValue);
            return true;
        }

        value = 0;
        return false;
    }

    protected static bool GetFloatValue(ISensor sensor, SensorInfo info, out float value)
    {
        foreach (var pair in info.Pairs)
        {
            var innerValue = sensor.Value.GetValueOrDefault(0f);

            if (sensor.SensorType != pair.Type || sensor.Name != pair.Name || innerValue == 0f) continue;

            value = innerValue;
            return true;
        }

        value = 0f;
        return false;
    }

    public virtual void Update(ISensor sensor)
    {
        if (GetFloatValue(sensor, LoadInfo, out var loadValue)) Usage = loadValue;
    }
}

public abstract class CPU : HardwareComponent
{
    protected override SensorInfo LoadInfo => new(SensorType.Load, "CPU Total");
    protected virtual SensorInfo PowerInfo => throw new NotImplementedException();
    protected virtual SensorInfo TemperatureInfo => throw new NotImplementedException();

    public int Power { get; private set; }
    public int Temperature { get; private set; }

    public override void Update(ISensor sensor)
    {
        base.Update(sensor);
        if (GetIntValue(sensor, PowerInfo, out var powerValue)) Power = powerValue;
        if (GetIntValue(sensor, TemperatureInfo, out var temperatureValue)) Temperature = temperatureValue;
    }
}

public class IntelCPU : CPU
{
    protected override SensorInfo PowerInfo => new(SensorType.Power, "CPU Package");
    protected override SensorInfo TemperatureInfo => new(SensorType.Temperature, "CPU Package");
}

// ReSharper disable once InconsistentNaming
public class AMDCPU : CPU
{
    protected override SensorInfo PowerInfo => new(SensorType.Power, "Package");
    protected override SensorInfo TemperatureInfo => new(SensorType.Temperature, "Core (Tdie)", "Core (Tctl/Tdie)", "CPU Cores");
}

public class GPU : HardwareComponent
{
    protected override SensorInfo LoadInfo => new(SensorType.Load, "GPU Core");
    private readonly SensorInfo powerInfo = new(SensorType.Power, "GPU Package");
    private readonly SensorInfo temperatureInfo = new(SensorType.Temperature, "GPU Core");
    private readonly SensorInfo memoryFreeInfo = new(SensorType.SmallData, "GPU Memory Free");
    private readonly SensorInfo memoryUsedInfo = new(SensorType.SmallData, "GPU Memory Used", "D3D Dedicated Memory Used");
    private readonly SensorInfo memoryTotalInfo = new(SensorType.SmallData, "GPU Memory Total");

    public int Power { get; private set; }
    public int Temperature { get; private set; }
    public float MemoryFree { get; private set; }
    public float MemoryUsed { get; private set; }
    public float MemoryTotal { get; private set; }
    public float MemoryUsage => MemoryUsed / MemoryTotal;

    public override void Update(ISensor sensor)
    {
        base.Update(sensor);
        if (GetIntValue(sensor, powerInfo, out var powerValue)) Power = powerValue;
        if (GetIntValue(sensor, temperatureInfo, out var temperatureValue)) Temperature = temperatureValue;
        if (GetFloatValue(sensor, memoryFreeInfo, out var memoryFreeValue)) MemoryFree = memoryFreeValue;
        if (GetFloatValue(sensor, memoryUsedInfo, out var memoryUsedValue)) MemoryUsed = memoryUsedValue;
        if (GetFloatValue(sensor, memoryTotalInfo, out var memoryTotalValue)) MemoryTotal = memoryTotalValue;
    }
}

public class RAM : HardwareComponent
{
    protected override SensorInfo LoadInfo => new(SensorType.Load, "Memory");
    private readonly SensorInfo memoryUsedInfo = new(SensorType.Data, "Memory Used");
    private readonly SensorInfo memoryAvailableInfo = new(SensorType.Data, "Memory Available");

    public float Used { get; private set; }
    public float Available { get; private set; }
    public float Total => Used + Available;

    public override void Update(ISensor sensor)
    {
        base.Update(sensor);
        if (GetFloatValue(sensor, memoryUsedInfo, out var memoryUsedValue)) Used = memoryUsedValue;
        if (GetFloatValue(sensor, memoryAvailableInfo, out var memoryAvailableValue)) Available = memoryAvailableValue;
    }
}
