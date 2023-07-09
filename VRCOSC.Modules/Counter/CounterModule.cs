// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Attributes;
using VRCOSC.Game.Modules.ChatBox;
using VRCOSC.Game.OSC.VRChat;
using VRCOSC.Modules.Counter.SaveState.V1;

namespace VRCOSC.Modules.Counter;

[ModuleTitle("Counter")]
[ModuleDescription("Counts how many times parameters are triggered based on parameter change events")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.General)]
public class CounterModule : ChatBoxModule
{
    protected override bool EnablePersistence => GetSetting<bool>(CounterSetting.SaveCounters);

    [ModulePersistent("counts")]
    public Dictionary<string, CountInstance> Counts { get; set; } = new();

    protected override void CreateAttributes()
    {
        CreateSetting(CounterSetting.ResetOnAvatarChange, "Reset On Avatar Change", "Should the counter reset on avatar change?", false);
        CreateSetting(CounterSetting.SaveCounters, "Save Counters", "Should the counters be saved between module restarts?", true);

        CreateSetting(CounterSetting.ParameterList, "Parameter List", "What parameters should be monitored for changes?\nKeys can be reused to allow multiple parameters to add to the same counter\nCounts can be accessed in the ChatBox using: {counter.value_Key}", new List<MutableKeyValuePair> { new() { Key = { Value = "Example" }, Value = { Value = "ExampleParameterName" } } }, "Key",
            "Parameter Name");

        CreateVariable(CounterVariable.Value, "Value", "value");
        CreateVariable(CounterVariable.ValueToday, "Value Today", "valuetoday");

        CreateState(CounterState.Default, "Default", $"Today: {GetVariableFormat(CounterVariable.ValueToday, "Example")}/vTotal: {GetVariableFormat(CounterVariable.Value, "Example")}");

        CreateEvent(CounterEvent.Changed, "Changed", $"Today: {GetVariableFormat(CounterVariable.ValueToday, "Example")}/vTotal: {GetVariableFormat(CounterVariable.Value, "Example")}", 5);

        RegisterLegacyPersistanceSerialiser<CounterSaveStateSerialiser>();
    }

    protected override void OnModuleStart()
    {
        auditParameters();
        Counts.Values.ForEach(instance => SetVariableValue(CounterVariable.Value, instance.Count.ToString("N0"), instance.Key));
    }

    protected override void OnAvatarChange()
    {
        if (GetSetting<bool>(CounterSetting.ResetOnAvatarChange))
        {
            Counts.ForEach(pair =>
            {
                pair.Value.Count = 0;
                pair.Value.CountToday = 0;
            });
        }
    }

    private void auditParameters()
    {
        GetSettingList<MutableKeyValuePair>(CounterSetting.ParameterList).ForEach(pair =>
        {
            if (string.IsNullOrEmpty(pair.Key.Value) || string.IsNullOrEmpty(pair.Value.Value)) return;

            Counts.TryAdd(pair.Key.Value, new CountInstance(pair.Key.Value));
            Counts[pair.Key.Value].ParameterNames.Add(pair.Value.Value);

            SetVariableValue(CounterVariable.Value, Counts[pair.Key.Value].Count.ToString("N0"), pair.Key.Value);
            SetVariableValue(CounterVariable.ValueToday, Counts[pair.Key.Value].CountToday.ToString("N0"), pair.Key.Value);
        });

        Counts.ForEach(pair =>
        {
            if (GetSettingList<MutableKeyValuePair>(CounterSetting.ParameterList).All(instance => instance.Key.Value != pair.Key))
            {
                Counts.Remove(pair.Key);
            }
        });
    }

    protected override void OnAnyParameterReceived(VRChatOscData data)
    {
        var instance = Counts.Values.SingleOrDefault(instance => instance.ParameterNames.Contains(data.ParameterName));
        if (instance is null) return;

        if (data.IsValueType<float>() && data.ValueAs<float>() > 0.9f) counterChanged(instance);
        if (data.IsValueType<int>() && data.ValueAs<int>() != 0) counterChanged(instance);
        if (data.IsValueType<bool>() && data.ValueAs<bool>()) counterChanged(instance);
    }

    private void counterChanged(CountInstance instance)
    {
        instance.Count++;
        instance.CountToday++;
        SetVariableValue(CounterVariable.Value, instance.Count.ToString("N0"), instance.Key);
        SetVariableValue(CounterVariable.ValueToday, instance.CountToday.ToString("N0"), instance.Key);
        TriggerEvent(CounterEvent.Changed);
    }

    private enum CounterSetting
    {
        ResetOnAvatarChange,
        SaveCounters,
        ParameterList
    }

    private enum CounterVariable
    {
        Value,
        ValueToday
    }

    private enum CounterState
    {
        Default
    }

    private enum CounterEvent
    {
        Changed
    }
}

public class CountInstance
{
    [JsonProperty("key")]
    public readonly string Key = null!;

    [JsonProperty("count")]
    public int Count;

    [JsonIgnore]
    public readonly List<string> ParameterNames = new();

    [JsonIgnore]
    public int CountToday;

    [JsonConstructor]
    public CountInstance()
    {
    }

    public CountInstance(string key)
    {
        Key = key;
    }
}
