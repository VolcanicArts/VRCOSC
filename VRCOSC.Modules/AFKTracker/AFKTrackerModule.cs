// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.Processes;

namespace VRCOSC.Modules.AFKTracker;

[ModuleTitle("AFK Tracker")]
[ModuleDescription("Displays text and time since going AFK")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.General)]
[ModuleLegacy("afkmodule")]
public class AFKTrackerModule : ChatBoxModule
{
    private DateTime? afkBegan;

    protected override void CreateAttributes()
    {
        CreateVariable(AFKVariable.Duration, "Duration", "duration");
        CreateVariable(AFKVariable.Since, "Since", "since");

        CreateState(AFKState.AFK, "AFK", $"AFK for {GetVariableFormat(AFKVariable.Duration)}");
        CreateState(AFKState.NotAFK, "Not AFK", string.Empty);

        CreateEvent(AFKEvent.AFKStarted, "AFK Started", "AFK has begun", 5);
        CreateEvent(AFKEvent.AFKStopped, "AFK Stopped", "AFK has ended", 5);
    }

    protected override void OnModuleStart()
    {
        afkBegan = null;

        ChangeStateTo(AFKState.NotAFK);
    }

    [ModuleUpdate(ModuleUpdateMode.Custom, true, 1000)]
    private void moduleUpdate()
    {
        SetVariableValue(AFKVariable.FocusedWindow, ProcessExtensions.GetActiveWindowTitle() ?? "None");
        SetVariableValue(AFKVariable.Duration, afkBegan is null ? null : (DateTime.Now - afkBegan.Value).ToString(@"hh\:mm\:ss"));
        SetVariableValue(AFKVariable.Since, afkBegan?.ToString(@"hh\:mm"));
        ChangeStateTo(afkBegan is null ? AFKState.NotAFK : AFKState.AFK);
    }

    protected override void OnPlayerUpdate()
    {
        if (Player.AFK is null)
        {
            ChangeStateTo(AFKState.NotAFK);
            return;
        }

        if (Player.AFK.Value && afkBegan is null)
        {
            afkBegan = DateTime.Now;
            TriggerEvent(AFKEvent.AFKStarted);
        }

        if (!Player.AFK.Value && afkBegan is not null)
        {
            afkBegan = null;
            TriggerEvent(AFKEvent.AFKStopped);
        }
    }

    private enum AFKVariable
    {
        Duration,
        Since,
        FocusedWindow
    }

    private enum AFKState
    {
        AFK,
        NotAFK
    }

    private enum AFKEvent
    {
        AFKStarted,
        AFKStopped
    }
}
