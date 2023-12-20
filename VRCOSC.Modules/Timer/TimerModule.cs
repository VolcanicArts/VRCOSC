// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;

namespace VRCOSC.Modules.Timer;

[ModuleTitle("Timer")]
[ModuleDescription("Counts down to a specified time to be put in the ChatBox")]
[ModuleGroup(ModuleType.General)]
public class TimerModule : ChatBoxModule
{
    protected override void CreateAttributes()
    {
        CreateSetting(TimerSetting.Time, "Time", "The date/time to countdown to", string.Empty);

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
            var timeNow = DateTime.Now;
            var difference = time - timeNow;
            var differenceNoMilli = new TimeSpan(difference.Days, difference.Hours, difference.Minutes, difference.Seconds);
            SetVariableValue(TimerVariable.Time, differenceNoMilli.ToString("g"));
        }
        else
        {
            Log("Could not parse entered time");
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
