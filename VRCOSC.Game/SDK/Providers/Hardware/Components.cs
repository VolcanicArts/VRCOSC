// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using LibreHardwareMonitor.Hardware;

namespace VRCOSC.Game.SDK.Providers.Hardware;

public class SensorInfoList
{
    public readonly List<SensorInfo> InfoList = new();

    public SensorInfoList(SensorType type, params string[] names)
    {
        foreach (var name in names) InfoList.Add(new SensorInfo(type, name));
    }
}

public class SensorInfo
{
    public readonly string Name;
    public readonly SensorType Type;

    public SensorInfo(SensorType type, string name)
    {
        Type = type;
        Name = name;
    }
}

public abstract class HardwareComponent
{
    protected virtual SensorInfo LoadInfo => throw new NotImplementedException();

    public readonly int Id;

    public float Usage { get; private set; }

    protected HardwareComponent(int id)
    {
        Id = id;
    }

    protected bool GetIntValue(ISensor sensor, SensorInfoList infoList, out int value)
    {
        foreach (var info in infoList.InfoList)
        {
            if (!GetIntValue(sensor, info, out var intValue)) continue;

            value = intValue;
            return true;
        }

        value = 0;
        return false;
    }

    protected bool GetIntValue(ISensor sensor, SensorInfo info, out int value)
    {
        if (GetFloatValue(sensor, info, out var valueFloat))
        {
            value = (int)Math.Round(valueFloat);
            return true;
        }

        value = 0;
        return false;
    }

    protected bool GetFloatValue(ISensor sensor, SensorInfo info, out float value)
    {
        if (sensor.Name == info.Name && sensor.SensorType == info.Type)
        {
            value = sensor.Value ?? 0f;
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
    protected virtual SensorInfoList TemperatureInfo => throw new NotImplementedException();

    public int Power { get; private set; }
    public int Temperature { get; private set; }

    protected CPU(int id)
        : base(id)
    {
    }

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
    protected override SensorInfoList TemperatureInfo => new(SensorType.Temperature, "CPU Package");

    public IntelCPU(int id)
        : base(id)
    {
    }
}

// ReSharper disable once InconsistentNaming
public class AMDCPU : CPU
{
    protected override SensorInfo PowerInfo => new(SensorType.Power, "Package");
    protected override SensorInfoList TemperatureInfo => new(SensorType.Temperature, "Core (Tdie)", "Core (Tctl/Tdie)", "CPU Cores");

    public AMDCPU(int id)
        : base(id)
    {
    }
}

public class GPU : HardwareComponent
{
    protected override SensorInfo LoadInfo => new(SensorType.Load, "GPU Core");
    private readonly SensorInfo powerInfo = new(SensorType.Power, "GPU Package");
    private readonly SensorInfo temperatureInfo = new(SensorType.Temperature, "GPU Core");
    private readonly SensorInfo memoryFreeInfo = new(SensorType.SmallData, "GPU Memory Free");
    private readonly SensorInfo memoryUsedINfo = new(SensorType.SmallData, "GPU Memory Used");
    private readonly SensorInfo memoryTotalInfo = new(SensorType.SmallData, "GPU Memory Total");

    public int Power { get; private set; }
    public int Temperature { get; private set; }
    public float MemoryFree { get; private set; }
    public float MemoryUsed { get; private set; }
    public float MemoryTotal { get; private set; }
    public float MemoryUsage => MemoryUsed / MemoryTotal;

    public GPU(int id)
        : base(id)
    {
    }

    public override void Update(ISensor sensor)
    {
        base.Update(sensor);
        if (GetIntValue(sensor, powerInfo, out var powerValue)) Power = powerValue;
        if (GetIntValue(sensor, temperatureInfo, out var temperatureValue)) Temperature = temperatureValue;
        if (GetFloatValue(sensor, memoryFreeInfo, out var memoryFreeValue)) MemoryFree = memoryFreeValue;
        if (GetFloatValue(sensor, memoryUsedINfo, out var memoryUsedValue)) MemoryUsed = memoryUsedValue;
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

    public RAM()
        : base(0)
    {
    }

    public override void Update(ISensor sensor)
    {
        base.Update(sensor);
        if (GetFloatValue(sensor, memoryUsedInfo, out var memoryUsedValue)) Used = memoryUsedValue;
        if (GetFloatValue(sensor, memoryAvailableInfo, out var memoryAvailableValue)) Available = memoryAvailableValue;
    }
}
