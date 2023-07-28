// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Attributes;
using VRCOSC.Game.Modules.Avatar;

namespace VRCOSC.Modules.Counter;

[ModuleTitle("Counter")]
[ModuleDescription("Counts how many times parameters are triggered based on parameter change events")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.General)]
public class CounterModule : ChatBoxModule
{
    protected override bool EnablePersistence => GetSetting<bool>(CounterSetting.SaveCounters);

    [ModulePersistent("counts")]
    private Dictionary<string, CountInstance> counts { get; set; } = new();

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
    }

    protected override void OnModuleStart()
    {
        ChangeStateTo(CounterState.Default);
        auditParameters();
        counts.ForEach(pair => SetVariableValue(CounterVariable.Value, pair.Value.Count.ToString("N0"), pair.Key));
    }

    protected override void OnAvatarChange()
    {
        if (GetSetting<bool>(CounterSetting.ResetOnAvatarChange))
        {
            counts.ForEach(pair =>
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

            counts.TryAdd(pair.Key.Value, new CountInstance());
            counts[pair.Key.Value].ParameterNames.Add(pair.Value.Value);

            SetVariableValue(CounterVariable.Value, counts[pair.Key.Value].Count.ToString("N0"), pair.Key.Value);
            SetVariableValue(CounterVariable.ValueToday, counts[pair.Key.Value].CountToday.ToString("N0"), pair.Key.Value);
        });

        counts.ForEach(pair =>
        {
            if (GetSettingList<MutableKeyValuePair>(CounterSetting.ParameterList).All(instance => instance.Key.Value != pair.Key))
            {
                counts.Remove(pair.Key);
            }
        });
    }

    protected override void OnAnyParameterReceived(ReceivedParameter parameter)
    {
        var candidates = counts.Where(pair => pair.Value.ParameterNames.Contains(parameter.Name)).ToList();
        if (!candidates.Any()) return;

        var pair = candidates[0];

        if (parameter.IsValueType<float>() && parameter.ValueAs<float>() > 0.9f) counterChanged(pair);
        if (parameter.IsValueType<int>() && parameter.ValueAs<int>() != 0) counterChanged(pair);
        if (parameter.IsValueType<bool>() && parameter.ValueAs<bool>()) counterChanged(pair);
    }

    private void counterChanged(KeyValuePair<string, CountInstance> pair)
    {
        pair.Value.Count++;
        pair.Value.CountToday++;
        SetVariableValue(CounterVariable.Value, pair.Value.Count.ToString("N0"), pair.Key);
        SetVariableValue(CounterVariable.ValueToday, pair.Value.CountToday.ToString("N0"), pair.Key);
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
}
