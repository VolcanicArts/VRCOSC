// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.ChatBox;

namespace VRCOSC.Modules.AFK;

public class AFKModule : ChatBoxModule
{
    public override string Title => "AFK Display";
    public override string Description => "Display text and time since going AFK";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;
    protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(1.5f);

    private DateTime? afkBegan;

    protected override void CreateAttributes()
    {
        CreateVariable(AFKModuleVariable.Duration, "Duration", "duration");
        CreateVariable(AFKModuleVariable.Since, "Since", "since");

        CreateState(AFKModuleState.AFK, "AFK", $"AFK for {GetVariableFormat(AFKModuleVariable.Duration)}");
        CreateState(AFKModuleState.NotAFK, "Not AFK", string.Empty);
    }

    protected override void OnModuleStart()
    {
        afkBegan = null;

        ChangeStateTo(AFKModuleState.NotAFK);
    }

    protected override void OnModuleUpdate()
    {
        if (Player.AFK is null)
        {
            ChangeStateTo(AFKModuleState.NotAFK);
            return;
        }

        if (Player.AFK.Value && afkBegan is null)
        {
            afkBegan = DateTime.Now;
        }

        if (!Player.AFK.Value && afkBegan is not null)
        {
            afkBegan = null;
        }

        SetVariableValue(AFKModuleVariable.Duration, afkBegan is null ? null : (DateTime.Now - afkBegan.Value).ToString(@"hh\:mm\:ss"));
        SetVariableValue(AFKModuleVariable.Since, afkBegan?.ToString(@"hh\:mm"));
        ChangeStateTo(afkBegan is null ? AFKModuleState.NotAFK : AFKModuleState.AFK);
    }

    private enum AFKModuleVariable
    {
        Duration,
        Since
    }

    private enum AFKModuleState
    {
        AFK,
        NotAFK
    }
}
