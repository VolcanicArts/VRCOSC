// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
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
    private const string progress_line = "\u2501";
    private const string progress_dot = "\u25CF";
    private const string progress_start = "\u2523";
    private const string progress_end = "\u252B";
    private const int progress_resolution = 10;

    protected override bool EnablePersistence => GetSetting<bool>(CounterSetting.SaveCounters);

    [ModulePersistent("counts")]
    private Dictionary<string, CountInstance> counts { get; set; } = new();

    protected override void CreateAttributes()
    {
        CreateSetting(CounterSetting.ResetOnAvatarChange, "Reset On Avatar Change", "Should the counter reset on avatar change?", false);
        CreateSetting(CounterSetting.SaveCounters, "Save Counters", "Should the counters be saved between module restarts?", true);

        CreateSetting(CounterSetting.ParameterList, "Parameter List", "What parameters should be monitored for changes?\nKeys can be reused to allow multiple parameters to add to the same counter\nCounts can be accessed in the ChatBox using: {counter.value_Key}", new List<MutableKeyValuePair> { new() { Key = { Value = "Example" }, Value = { Value = "ExampleParameterName" } } }, "Key",
            "Parameter Name");

        CreateSetting(CounterSetting.Milestones, new CounterMilestoneInstanceListAttribute
        {
            Name = "Milestones",
            Description = "Set `parameter name` to true when `counter key` reaches `required count`\nThese will be set when the module starts if a counter has already reached the milestone\nParameter names aren't required if you just want to use the ChatBox milestone variables\nYou can access milestones using `_Key` after the variable name",
            Default = new List<CounterMilestoneInstance>()
        });

        CreateVariable(CounterVariable.Value, "Value", "value");
        CreateVariable(CounterVariable.ValueToday, "Value Today", "valuetoday");
        CreateVariable(CounterVariable.MilestonePrevious, "Milestone Previous", "milestoneprevious");
        CreateVariable(CounterVariable.MilestoneCurrent, "Milestone Current", "milestonecurrent");
        CreateVariable(CounterVariable.MilestoneProgress, "Milestone Progress", "milestoneprogress");

        CreateState(CounterState.Default, "Default", $"Today: {GetVariableFormat(CounterVariable.ValueToday, "Example")}/vTotal: {GetVariableFormat(CounterVariable.Value, "Example")}");

        CreateEvent(CounterEvent.Changed, "Changed", $"Today: {GetVariableFormat(CounterVariable.ValueToday, "Example")}/vTotal: {GetVariableFormat(CounterVariable.Value, "Example")}", 5);
    }

    protected override void OnModuleStart()
    {
        ChangeStateTo(CounterState.Default);
        auditParameters();

        counts.ForEach(pair =>
        {
            SetVariableValue(CounterVariable.Value, pair.Value.Count.ToString("N0"), pair.Key);
            checkMilestones(pair);
        });
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

        checkMilestones(pair);
    }

    private void checkMilestones(KeyValuePair<string, CountInstance> pair)
    {
        var milestones = GetSettingList<CounterMilestoneInstance>(CounterSetting.Milestones).Where(instance => instance.CounterKey.Value == pair.Key).ToList();

        if (!milestones.Any())
        {
            SetVariableValue(CounterVariable.MilestonePrevious, string.Empty, pair.Key);
            SetVariableValue(CounterVariable.MilestoneCurrent, string.Empty, pair.Key);
            SetVariableValue(CounterVariable.MilestoneProgress, string.Empty, pair.Key);
            return;
        }

        var instances = milestones.Where(instance => pair.Value.Count >= instance.RequiredCount.Value && !string.IsNullOrEmpty(instance.ParameterName.Value));
        instances.ForEach(instance => SendParameter(instance.ParameterName.Value, true));

        var milestoneCurrent = milestones.LastOrDefault(instance => pair.Value.Count < instance.RequiredCount.Value);
        var milestonePrevious = milestones.FirstOrDefault(instance => pair.Value.Count >= instance.RequiredCount.Value);

        SetVariableValue(CounterVariable.MilestoneCurrent, milestoneCurrent is not null ? milestoneCurrent.RequiredCount.Value.ToString() : float.PositiveInfinity.ToString(CultureInfo.InvariantCulture), pair.Key);
        SetVariableValue(CounterVariable.MilestonePrevious, milestonePrevious is not null ? milestonePrevious.RequiredCount.Value.ToString() : "0", pair.Key);

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
        MilestoneProgress,
        MilestonePrevious,
        MilestoneCurrent
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
