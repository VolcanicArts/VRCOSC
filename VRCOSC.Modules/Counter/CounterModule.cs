// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.Attributes;
using VRCOSC.Game.Modules.ChatBox;
using VRCOSC.Game.OSC.VRChat;
using VRCOSC.Modules.Counter.SaveState.V1;

namespace VRCOSC.Modules.Counter;

public class CounterModule : ChatBoxModule
{
    public override string Title => "Counter";
    public override string Description => "Counts how many times parameters are triggered based on parameter change events";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;

    internal readonly Dictionary<string, CountInstance> Counts = new();

    protected override void CreateAttributes()
    {
        CreateSetting(CounterSetting.ResetOnAvatarChange, "Reset On Avatar Change", "Should the counter reset on avatar change?", false);
        CreateSetting(CounterSetting.SaveCounters, "Save Counters", "Should the counters be saved between VRCOSC restarts?", true);
        CreateSetting(CounterSetting.ParameterList, "Parameter List", "What parameters should be monitored for them becoming true?\nKeys can be reused to allow multiple parameters to add to the same counter\nCounts can be accessed in the ChatBox using: {counter.value_Key}", new List<MutableKeyValuePair>(), "Key", "Parameter Name");

        CreateVariable(CounterVariable.Value, "Value", "value");

        CreateState(CounterState.Default, "Default", GetVariableFormat(CounterVariable.Value));

        CreateEvent(CounterEvent.Changed, "Changed", GetVariableFormat(CounterVariable.Value), 5);

        RegisterSaveStateSerialiser<CounterSaveStateSerialiser>(1);
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
        Counts.Clear();

        GetSettingList<MutableKeyValuePair>(CounterSetting.ParameterList).ForEach(pair =>
        {
            if (string.IsNullOrEmpty(pair.Key.Value) || string.IsNullOrEmpty(pair.Value.Value)) return;

            Counts.TryAdd(pair.Key.Value, new CountInstance(pair.Key.Value));
            Counts[pair.Key.Value].ParameterNames.Add(pair.Value.Value);
            SetVariableValue(CounterVariable.Value, Counts[pair.Key.Value].Count.ToString(), pair.Key.Value);
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
        SetVariableValue(CounterVariable.Value, instance.Count.ToString("N0"), instance.Key);
        TriggerEvent(CounterEvent.Changed);
        saveState();
    }

    private void saveState()
    {
        if (!GetSetting<bool>(CounterSetting.SaveCounters)) return;

        SaveState();
    }

    private void loadState()
    {
        if (!GetSetting<bool>(CounterSetting.SaveCounters)) return;

        LoadState();
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
}

public class CountInstance
{
    public readonly string Key;
    public readonly List<string> ParameterNames = new();
    public int Count;

    public CountInstance(string key)
    {
        Key = key;
    }
}
