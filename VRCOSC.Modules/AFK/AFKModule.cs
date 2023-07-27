// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.ChatBox;
using VRCOSC.Game.OSC.VRChat;
using VRCOSC.Game.Processes;

namespace VRCOSC.Modules.AFK;

public class AFKModule : ChatBoxModule
{
    public override string Title => "AFK Display";
    public override string Description => "Display text and time since going AFK";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;
    protected override TimeSpan DeltaUpdate => VRChatOscConstants.UPDATE_TIME_SPAN;

    private DateTime? afkBegan;

    protected override void CreateAttributes()
    {
        CreateVariable(AFKVariable.Duration, "Duration", "duration");
        CreateVariable(AFKVariable.Since, "Since", "since");
        CreateVariable(AFKVariable.FocusedWindow, "Focused Window", "focusedwindow");

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

    protected override void OnModuleUpdate()
    {
        if (Player.AFK is null)
        {
            ChangeStateTo(AFKState.NotAFK);
            return;
        }

        if (Player.AFK.Value && afkBegan is null)
        {
            afkBegan = DateTime.Now;
            SetVariableValue(AFKVariable.FocusedWindow, ProcessExtensions.GetActiveWindowTitle() ?? "None");
            TriggerEvent(AFKEvent.AFKStarted);
        }

        if (!Player.AFK.Value && afkBegan is not null)
        {
            afkBegan = null;
            TriggerEvent(AFKEvent.AFKStopped);
        }

        SetVariableValue(AFKVariable.Duration, afkBegan is null ? null : (DateTime.Now - afkBegan.Value).ToString(@"hh\:mm\:ss"));
        SetVariableValue(AFKVariable.Since, afkBegan?.ToString(@"hh\:mm"));
        ChangeStateTo(afkBegan is null ? AFKState.NotAFK : AFKState.AFK);
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
