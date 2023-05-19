// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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

        CreateSetting(CounterSetting.ParameterList, new ModuleNameParameterPairAttribute
        {
            Name = "Parameter List",
            Description = "What parameters should be monitored for them becoming true?\nCounts can be accessed in the ChatBox using: {counter.value_Key}",
            Default = new List<NameParameterPair>()
        });

        CreateVariable(CounterVariable.Value, "Value", "value");

        CreateState(CounterState.Default, "Default", GetVariableFormat(CounterVariable.Value));

        CreateEvent(CounterEvent.Changed, "Changed", GetVariableFormat(CounterVariable.Value), 5);
    }

    protected override void OnModuleStart()
    {
        auditParameters();
    }

    protected override void OnAvatarChange()
    {
        if (GetSetting<bool>(CounterSetting.ResetOnAvatarChange)) auditParameters();
    }

    private void auditParameters()
    {
        counts.Clear();

        GetSettingList<NameParameterPair>(CounterSetting.ParameterList).ForEach(pair =>
        {
            if (string.IsNullOrEmpty(pair.Key.Value) || string.IsNullOrEmpty(pair.Parameter.Value)) return;

            counts.Add(pair.Parameter.Value, new CountInstance(pair.Key.Value, 0));
            SetVariableValue(CounterVariable.Value, "0", pair.Key.Value);
        });
    }

    protected override void OnAnyParameterReceived(VRChatOscData data)
    {
        var parameterName = data.ParameterName;

        if (data.ParameterValue.GetType() != typeof(bool)) return;
        if (!(bool)data.ParameterValue) return;

        if (counts.TryGetValue(parameterName, out var value))
        {
            value.Count++;
            SetVariableValue(CounterVariable.Value, value.Count.ToString("N0"), value.Key);
            TriggerEvent(CounterEvent.Changed);
        }
    }

    private enum CounterSetting
    {
        ResetOnAvatarChange,
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
