// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using VRCOSC.Game.Modules.Attributes;
using VRCOSC.Game.Modules.ChatBox;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Modules.Counter;

public class CounterModule : ChatBoxModule
{
    public override string Title => "Counter";
    public override string Description => "Counts how many times parameters are triggered based on boolean events";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;

    private readonly Dictionary<string, CountInstance> counts = new();

    private class CountInstance
    {
        public readonly string Key;
        public int Count;

        public CountInstance(string key, int count)
        {
            Key = key;
            Count = count;
        }
    }

    protected override void CreateAttributes()
    {
        CreateSetting(CounterSetting.ResetOnAvatarChange, "Reset On Avatar Change", "Should the counter reset on avatar change?", false);
        CreateSetting(CounterSetting.SaveCounters, "Save Counters", "Should the counters be saved between VRCOSC restarts?", true);
        CreateSetting(CounterSetting.ParameterList, "Parameter List", "What parameters should be monitored for them becoming true?\nCounts can be accessed in the ChatBox using: {counter.value_Key}", new List<MutableKeyValuePair>(), "Key", "Parameter Name");

        CreateVariable(CounterVariable.Value, "Value", "value");

        CreateState(CounterState.Default, "Default", GetVariableFormat(CounterVariable.Value));

        CreateEvent(CounterEvent.Changed, "Changed", GetVariableFormat(CounterVariable.Value), 5);
    }

    protected override void OnModuleStart()
    {
        auditParameters();
        loadState();
    }

    protected override void OnAvatarChange()
    {
        if (GetSetting<bool>(CounterSetting.ResetOnAvatarChange))
        {
            auditParameters();
            saveState();
        }
    }

    protected override void OnModuleStop()
    {
        saveState();
    }

    private void auditParameters()
    {
        counts.Clear();

        GetSettingList<MutableKeyValuePair>(CounterSetting.ParameterList).ForEach(pair =>
        {
            if (string.IsNullOrEmpty(pair.Key.Value) || string.IsNullOrEmpty(pair.Value.Value)) return;

            counts.Add(pair.Value.Value, new CountInstance(pair.Key.Value, 0));
            SetVariableValue(CounterVariable.Value, "0", pair.Key.Value);
        });
    }

    protected override void OnAnyParameterReceived(VRChatOscData data)
    {
        if (!counts.TryGetValue(data.ParameterName, out var value)) return;

        if (data.IsValueType<float>() && data.ValueAs<float>() > 0.9f) counterChanged(value);
        if (data.IsValueType<int>() && data.ValueAs<int>() != 0) counterChanged(value);
        if (data.IsValueType<bool>() && data.ValueAs<bool>()) counterChanged(value);
    }

    private void counterChanged(CountInstance instance)
    {
        instance.Count++;
        SetVariableValue(CounterVariable.Value, instance.Count.ToString("N0"), instance.Key);
        TriggerEvent(CounterEvent.Changed);
        saveState();
    }

    private void saveState()
    {
        if (!GetSetting<bool>(CounterSetting.SaveCounters)) return;

        var saveState = new CounterSaveState();
        saveState.Instances.AddRange(counts.Values.Select(instance => new CounterInstanceSaveState(instance)));
        SaveState(saveState);
    }

    private void loadState()
    {
        if (!GetSetting<bool>(CounterSetting.SaveCounters)) return;

        LoadState<CounterSaveState>()?.Instances.ForEach(loadedInstance =>
        {
            var instance = counts.SingleOrDefault(instance => instance.Value.Key == loadedInstance.Key).Value;

            if (instance is not null)
            {
                instance.Count = loadedInstance.Count;
            }
        });
    }

    private enum CounterSetting
    {
        ResetOnAvatarChange,
        SaveCounters,
        ParameterList
    }

    private enum CounterVariable
    {
        Value
    }

    private enum CounterState
    {
        Default
    }

    private enum CounterEvent
    {
        Changed
    }

    private class CounterSaveState
    {
        [JsonProperty("instances")]
        public List<CounterInstanceSaveState> Instances = new();
    }

    private class CounterInstanceSaveState
    {
        [JsonProperty("key")]
        public string Key = null!;

        [JsonProperty("count")]
        public int Count;

        [JsonConstructor]
        public CounterInstanceSaveState()
        {
        }

        public CounterInstanceSaveState(CountInstance instance)
        {
            Key = instance.Key;
            Count = instance.Count;
        }
    }
}
