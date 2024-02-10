// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;

namespace VRCOSC.Modules.Timer;

[ModuleTitle("Timer")]
[ModuleDescription("Counts to a specified time to be put in the ChatBox")]
[ModuleGroup(ModuleType.General)]
public class TimerModule : ChatBoxModule
{
    protected override void CreateAttributes()
    {
        CreateSetting(TimerSetting.Time, "Time", "The date/time to count to.\nWorks with future and past dates/times", string.Empty);

        CreateVariable(TimerVariable.Time, "Time", "time");
        CreateState(TimerState.Default, "Default", GetVariableFormat(TimerVariable.Time));
    }

    protected override void OnModuleStart()
    {
        ChangeStateTo(TimerState.Default);
    }

    [ModuleUpdate(ModuleUpdateMode.ChatBox)]
    private void onChatBoxUpdate()
    {
        if (DateTime.TryParse(GetSetting<string>(TimerSetting.Time), null, DateTimeStyles.AssumeLocal, out var time))
        {
            var diff = time - DateTime.Now;
            SetVariableValue(TimerVariable.Time, $"{diff.Days * 24 + diff.Hours:00}:{diff.Minutes:00}:{diff.Seconds:00}".Replace("-", string.Empty));
        }
        else
        {
            Log("Could not parse entered time. Example: 1/1/2024 12:00:00");
        }
    }

    private enum TimerSetting
    {
        Time
    }

    private enum TimerState
    {
        Default
    }

    private enum TimerVariable
    {
        Time
    }
}
