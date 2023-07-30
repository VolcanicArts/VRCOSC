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

public class CounterModule : ChatBoxModule
{
    private const string progress_line = "\u2501";
    private const string progress_dot = "\u25CF";
    private const string progress_start = "\u2523";
    private const string progress_end = "\u252B";
    private const int progress_resolution = 10;

    public override string Title => "Counter";
    public override string Description => "Counts how many times parameters are triggered based on parameter change events";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;
    protected override bool EnablePersistence => GetSetting<bool>(CounterSetting.SaveCounters);

    [ModulePersistent("counts")]
    public Dictionary<string, CountInstance> Counts { get; set; } = new();

    protected override void CreateAttributes()
    {
        CreateSetting(CounterSetting.ResetOnAvatarChange, "Reset On Avatar Change", "Should the counter reset on avatar change?", false);
        CreateSetting(CounterSetting.SaveCounters, "Save Counters", "Should the counters be saved between module restarts?", true);

        CreateSetting(CounterSetting.ParameterList, "Parameter List", "What parameters should be monitored for changes?\nKeys can be reused to allow multiple parameters to add to the same counter\nCounts can be accessed in the ChatBox using: {counter.value_Key}", new List<MutableKeyValuePair> { new() { Key = { Value = "Example" }, Value = { Value = "ExampleParameterName" } } }, "Key",
            "Parameter Name");

        CreateSetting(CounterSetting.Milestones, new CounterMilestoneInstanceListAttribute
        {
            Name = "Milestones",
            Description = "Set `parameter name` to true when `counter key` reaches `required count`\nThese will be set when the module starts if a counter has already reached the milestone\nParameter names aren't required if you just want to use the ChatBox milestone variables",
            Default = new List<CounterMilestoneInstance>()
        });

        CreateVariable(CounterVariable.Value, "Value", "value");
        CreateVariable(CounterVariable.ValueToday, "Value Today", "valuetoday");
        CreateVariable(CounterVariable.Milestone, "Milestone", "milestone");
        CreateVariable(CounterVariable.MilestoneProgress, "Milestone Progress", "milestoneprogress");

        CreateState(CounterState.Default, "Default", $"Today: {GetVariableFormat(CounterVariable.ValueToday, "Example")}/vTotal: {GetVariableFormat(CounterVariable.Value, "Example")}");

        CreateEvent(CounterEvent.Changed, "Changed", $"Today: {GetVariableFormat(CounterVariable.ValueToday, "Example")}/vTotal: {GetVariableFormat(CounterVariable.Value, "Example")}", 5);

        RegisterLegacyPersistanceSerialiser<CounterSaveStateSerialiser>();
    }

    protected override void OnModuleStart()
    {
        ChangeStateTo(CounterState.Default);
        auditParameters();

        Counts.ForEach(pair =>
        {
            SetVariableValue(CounterVariable.Value, pair.Value.Count.ToString("N0"), pair.Key);
            checkMilestones(pair);
        });
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

            Counts.TryAdd(pair.Key.Value, new CountInstance());
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

    protected override void OnAnyParameterReceived(VRChatOscMessage message)
    {
        var candidates = Counts.Where(pair => pair.Value.ParameterNames.Contains(message.ParameterName)).ToList();
        if (!candidates.Any()) return;

        var pair = candidates[0];

        if (message.IsValueType<float>() && message.ValueAs<float>() > 0.9f) counterChanged(pair);
        if (message.IsValueType<int>() && message.ValueAs<int>() != 0) counterChanged(pair);
        if (message.IsValueType<bool>() && message.ValueAs<bool>()) counterChanged(pair);
    }

    private void counterChanged(KeyValuePair<string, CountInstance> pair)
    {
        pair.Value.Count++;
        pair.Value.CountToday++;
        SetVariableValue(CounterVariable.Value, pair.Value.Count.ToString("N0"), pair.Key);
        SetVariableValue(CounterVariable.ValueToday, pair.Value.CountToday.ToString("N0"), pair.Key);
        TriggerEvent(CounterEvent.Changed);

        checkMilestones(pair);
    }

    private void checkMilestones(KeyValuePair<string, CountInstance> pair)
    {
        var milestones = GetSettingList<CounterMilestoneInstance>(CounterSetting.Milestones).Where(instance => instance.CounterKey.Value == pair.Key).ToList();

        if (!milestones.Any())
        {
            SetVariableValue(CounterVariable.Milestone, string.Empty, pair.Key);
            SetVariableValue(CounterVariable.MilestoneProgress, string.Empty, pair.Key);
            return;
        }

        var instances = milestones.Where(instance => pair.Value.Count >= instance.RequiredCount.Value && !string.IsNullOrEmpty(instance.ParameterName.Value));
        instances.ForEach(instance => SendParameter(instance.ParameterName.Value, true));

        var milestone = milestones.LastOrDefault(instance => pair.Value.Count < instance.RequiredCount.Value);

        SetVariableValue(CounterVariable.Milestone, milestone is not null ? milestone.RequiredCount.Value.ToString() : string.Empty, pair.Key);

        var lowerboundIndex = milestones.FindLastIndex(instance => pair.Value.Count >= instance.RequiredCount.Value);

        var progressLowerbound = 0f;
        var progressUpperbound = 0f;

        if (lowerboundIndex == -1)
        {
            progressUpperbound = milestones[0].RequiredCount.Value;
        }
        else
        {
            progressLowerbound = milestones[lowerboundIndex].RequiredCount.Value;

            if (milestones.Count - 1 < lowerboundIndex + 1)
            {
                SetVariableValue(CounterVariable.MilestoneProgress, string.Empty, pair.Key);
                return;
            }

            progressUpperbound = milestones[lowerboundIndex + 1].RequiredCount.Value;

            if (pair.Value.Count >= progressUpperbound)
            {
                SetVariableValue(CounterVariable.MilestoneProgress, string.Empty, pair.Key);
                return;
            }
        }

        Console.WriteLine(progressLowerbound);
        Console.WriteLine(progressUpperbound);

        var progress = (pair.Value.Count - progressLowerbound) / (progressUpperbound - progressLowerbound);

        SetVariableValue(CounterVariable.MilestoneProgress, getProgressVisual(progress), pair.Key);
    }

    private string getProgressVisual(float percentage)
    {
        var progressPercentage = progress_resolution * percentage;
        var dotPosition = (int)(MathF.Floor(progressPercentage * 10f) / 10f);

        var visual = progress_start;

        for (var i = 0; i < progress_resolution; i++)
        {
            visual += i == dotPosition ? progress_dot : progress_line;
        }

        visual += progress_end;

        return visual;
    }

    private enum CounterSetting
    {
        ResetOnAvatarChange,
        SaveCounters,
        ParameterList,
        Milestones
    }

    private enum CounterVariable
    {
        Value,
        ValueToday,
        Milestone,
        MilestoneProgress
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
