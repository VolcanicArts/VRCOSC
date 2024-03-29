// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.Providers.Hardware;

namespace VRCOSC.Modules.HardwareStats;

[ModuleTitle("Hardware Stats")]
[ModuleDescription("Sends hardware stats as avatar parameters and allows for displaying them in the ChatBox")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.General)]
public sealed class HardwareStatsModule : ChatBoxModule
{
    private HardwareStatsProvider? hardwareStatsProvider;

    protected override void CreateAttributes()
    {
        CreateSetting(HardwareStatsSetting.SelectedCPU, "Selected CPU", "Enter the (0th based) index of the CPU you want to track", 0);
        CreateSetting(HardwareStatsSetting.SelectedGPU, "Selected GPU", "Enter the (0th based) index of the GPU you want to track", 0);
        CreateSetting(HardwareStatsSetting.ChatBoxFormatting, "ChatBox Formatting", "How should numbers be formatted in the chatbox?", "0.0");

        CreateParameter<float>(HardwareStatsParameter.CpuUsage, ParameterMode.Write, "VRCOSC/Hardware/CPUUsage", "CPU Usage", "The CPU usage normalised");
        CreateParameter<int>(HardwareStatsParameter.CpuPower, ParameterMode.Write, "VRCOSC/Hardware/CPUPower", "CPU Power", "The power usage of your CPU in Watts");
        CreateParameter<int>(HardwareStatsParameter.CpuTemp, ParameterMode.Write, "VRCOSC/Hardware/CPUTemp", "CPU Temp", "The CPU temp in C");
        CreateParameter<float>(HardwareStatsParameter.CpuTempNormalised, ParameterMode.Write, "VRCOSC/Hardware/CPUTempNormalised", "CPU Temp Normalised", "The CPU temp mapping 0-100c as 0-1");
        CreateParameter<float>(HardwareStatsParameter.GpuUsage, ParameterMode.Write, "VRCOSC/Hardware/GPUUsage", "GPU Usage", "The GPU usage normalised");
        CreateParameter<int>(HardwareStatsParameter.GpuPower, ParameterMode.Write, "VRCOSC/Hardware/GPUPower", "GPU Power", "The power usage of your GPU in Watts");
        CreateParameter<int>(HardwareStatsParameter.GpuTemp, ParameterMode.Write, "VRCOSC/Hardware/GPUTemp", "GPU Temp", "The GPU temp in C ");
        CreateParameter<float>(HardwareStatsParameter.GpuTempNormalised, ParameterMode.Write, "VRCOSC/Hardware/GPUTempNormalised", "GPU Temp Normalised", "The GPU temp mapping 0-100c as 0-1");
        CreateParameter<float>(HardwareStatsParameter.RamUsage, ParameterMode.Write, "VRCOSC/Hardware/RAMUsage", "RAM Usage", "The RAM usage normalised");
        CreateParameter<int>(HardwareStatsParameter.RamTotal, ParameterMode.Write, "VRCOSC/Hardware/RAMTotal", "RAM Total", "The total amount of RAM in GB");
        CreateParameter<int>(HardwareStatsParameter.RamUsed, ParameterMode.Write, "VRCOSC/Hardware/RAMUsed", "RAM Used", "The used RAM in GB");
        CreateParameter<int>(HardwareStatsParameter.RamAvailable, ParameterMode.Write, "VRCOSC/Hardware/RAMAvailable", "RAM Available", "The available RAM in GB");
        CreateParameter<float>(HardwareStatsParameter.VRamUsage, ParameterMode.Write, "VRCOSC/Hardware/VRamUsage", "VRAM Usage", "The amount of used VRAM normalised");
        CreateParameter<float>(HardwareStatsParameter.VRamFree, ParameterMode.Write, "VRCOSC/Hardware/VRamFree", "VRAM Free", "The amount of free VRAM in GB");
        CreateParameter<float>(HardwareStatsParameter.VRamUsed, ParameterMode.Write, "VRCOSC/Hardware/VRamUsed", "VRAM Used", "The amount of used VRAM in GB");
        CreateParameter<float>(HardwareStatsParameter.VRamTotal, ParameterMode.Write, "VRCOSC/Hardware/VRamTotal", "VRAM Total", "The amount of total VRAM in GB");

        CreateVariable(HardwareStatsParameter.CpuUsage, "CPU Usage (%)", "cpuusage");
        CreateVariable(HardwareStatsParameter.CpuPower, "CPU Power (W)", "cpupower");
        CreateVariable(HardwareStatsParameter.CpuTemp, "CPU Temp (C)", "cputemp");
        CreateVariable(HardwareStatsParameter.GpuUsage, "GPU Usage (%)", "gpuusage");
        CreateVariable(HardwareStatsParameter.GpuPower, "GPU Power (W)", "gpupower");
        CreateVariable(HardwareStatsParameter.GpuTemp, "GPU Temp (C)", "gputemp");
        CreateVariable(HardwareStatsParameter.RamUsage, "RAM Usage (%)", "ramusage");
        CreateVariable(HardwareStatsParameter.RamTotal, "RAM Total (GB)", "ramtotal");
        CreateVariable(HardwareStatsParameter.RamUsed, "RAM Used (GB)", "ramused");
        CreateVariable(HardwareStatsParameter.RamAvailable, "RAM Available (GB)", "ramavailable");
        CreateVariable(HardwareStatsParameter.VRamUsage, "VRAM Usage (%)", "vramusage");
        CreateVariable(HardwareStatsParameter.VRamUsed, "VRAM Used (GB)", "vramused");
        CreateVariable(HardwareStatsParameter.VRamFree, "VRAM Free (GB)", "vramfree");
        CreateVariable(HardwareStatsParameter.VRamTotal, "VRAM Total (GB)", "vramtotal");

        CreateState(HardwareStatsState.Default, "Default", $"CPU: {GetVariableFormat(HardwareStatsParameter.CpuUsage)}% | GPU: {GetVariableFormat(HardwareStatsParameter.GpuUsage)}%/vRAM: {GetVariableFormat(HardwareStatsParameter.RamUsed)}GB/{GetVariableFormat(HardwareStatsParameter.RamTotal)}GB");
    }

    protected override void OnModuleStart()
    {
        hardwareStatsProvider ??= new HardwareStatsProvider();
        hardwareStatsProvider.Init();
        ChangeStateTo(HardwareStatsState.Default);
    }

    [ModuleUpdate(ModuleUpdateMode.Custom, true, 500)]
    private async void updateParameters()
    {
        if (hardwareStatsProvider is null || !hardwareStatsProvider.CanAcceptQueries)
        {
            SetAllVariableValues<HardwareStatsParameter>("0");
            return;
        }

        await hardwareStatsProvider.Update();

        if (!hardwareStatsProvider.CPUs.TryGetValue(GetSetting<int>(HardwareStatsSetting.SelectedCPU), out var cpu))
        {
            Log($"CPU of id {GetSetting<int>(HardwareStatsSetting.SelectedCPU)} isn't available. If you have multiple, try changing the index");
            return;
        }

        if (!hardwareStatsProvider.GPUs.TryGetValue(GetSetting<int>(HardwareStatsSetting.SelectedGPU), out var gpu))
        {
            Log($"GPU of id {GetSetting<int>(HardwareStatsSetting.SelectedGPU)} isn't available. If you have multiple, try changing the index");
            return;
        }

        var ram = hardwareStatsProvider.RAM;

        if (ram is null)
        {
            Log("Could not connect to RAM. This is impossible, so well done!");
            return;
        }

        SendParameter(HardwareStatsParameter.CpuUsage, cpu.Usage / 100f);
        SendParameter(HardwareStatsParameter.CpuPower, cpu.Power);
        SendParameter(HardwareStatsParameter.CpuTemp, cpu.Temperature);
        SendParameter(HardwareStatsParameter.CpuTempNormalised, cpu.Temperature / 100f);
        SendParameter(HardwareStatsParameter.GpuUsage, gpu.Usage / 100f);
        SendParameter(HardwareStatsParameter.GpuPower, gpu.Power);
        SendParameter(HardwareStatsParameter.GpuTemp, gpu.Temperature);
        SendParameter(HardwareStatsParameter.GpuTempNormalised, gpu.Temperature / 100f);
        SendParameter(HardwareStatsParameter.RamUsage, ram.Usage / 100f);
        SendParameter(HardwareStatsParameter.RamTotal, (int)ram.Total);
        SendParameter(HardwareStatsParameter.RamUsed, (int)ram.Used);
        SendParameter(HardwareStatsParameter.RamAvailable, (int)ram.Available);
        SendParameter(HardwareStatsParameter.VRamUsage, gpu.MemoryUsage);
        SendParameter(HardwareStatsParameter.VRamFree, gpu.MemoryFree / 1000f);
        SendParameter(HardwareStatsParameter.VRamUsed, gpu.MemoryUsed / 1000f);
        SendParameter(HardwareStatsParameter.VRamTotal, gpu.MemoryTotal / 1000f);

        var format = GetSetting<string>(HardwareStatsSetting.ChatBoxFormatting);
        SetVariableValue(HardwareStatsParameter.CpuUsage, cpu.Usage.ToString(format));
        SetVariableValue(HardwareStatsParameter.CpuPower, cpu.Power.ToString());
        SetVariableValue(HardwareStatsParameter.CpuTemp, cpu.Temperature.ToString());
        SetVariableValue(HardwareStatsParameter.GpuUsage, gpu.Usage.ToString(format));
        SetVariableValue(HardwareStatsParameter.GpuPower, gpu.Power.ToString());
        SetVariableValue(HardwareStatsParameter.GpuTemp, gpu.Temperature.ToString());
        SetVariableValue(HardwareStatsParameter.RamUsage, ram.Usage.ToString(format));
        SetVariableValue(HardwareStatsParameter.RamTotal, ram.Total.ToString(format));
        SetVariableValue(HardwareStatsParameter.RamUsed, ram.Used.ToString(format));
        SetVariableValue(HardwareStatsParameter.RamAvailable, ram.Available.ToString(format));
        SetVariableValue(HardwareStatsParameter.VRamUsage, (gpu.MemoryUsage * 100f).ToString(format));
        SetVariableValue(HardwareStatsParameter.VRamFree, (gpu.MemoryFree / 1000f).ToString(format));
        SetVariableValue(HardwareStatsParameter.VRamUsed, (gpu.MemoryUsed / 1000f).ToString(format));
        SetVariableValue(HardwareStatsParameter.VRamTotal, (gpu.MemoryTotal / 1000f).ToString(format));
    }

    protected override void OnModuleStop()
    {
        hardwareStatsProvider?.Shutdown();
    }

    private enum HardwareStatsSetting
    {
        SelectedCPU,
        SelectedGPU,
        ChatBoxFormatting
    }

    private enum HardwareStatsParameter
    {
        CpuUsage,
        CpuPower,
        CpuTemp,
        CpuTempNormalised,
        GpuUsage,
        GpuPower,
        GpuTemp,
        GpuTempNormalised,
        RamUsage,
        RamTotal,
        RamUsed,
        RamAvailable,
        VRamUsage,
        VRamFree,
        VRamUsed,
        VRamTotal
    }

    private enum HardwareStatsState
    {
        Default
    }
}
