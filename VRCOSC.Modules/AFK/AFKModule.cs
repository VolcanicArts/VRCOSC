// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.AFK;

public class AFKModule : ChatBoxModule
{
    public override string Title => "AFK Display";
    public override string Description => "Display text and time since going AFK";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;
    protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(1.5f);
    protected override int ChatBoxPriority => 4;
    protected override IEnumerable<string> ChatBoxFormatValues => new[] { "%duration%" };
    protected override string DefaultChatBoxFormat => "AFK for %duration%";

    private bool isAFK;
    private DateTime? afkBegan;

    protected override void OnModuleStart()
    {
        isAFK = false;
        afkBegan = null;
    }

    protected override string? GetChatBoxText()
    {
        if (afkBegan is null) return null;

        return GetSetting<string>(ChatBoxSetting.ChatBoxFormat)
            .Replace("%duration%", (DateTime.Now - afkBegan.Value).ToString(@"hh\:mm\:ss"));
    }

    protected override void OnModuleUpdate()
    {
        if (Player.AFK is null) return;

        if (Player.AFK.Value && !isAFK)
        {
            afkBegan = DateTime.Now;
            isAFK = true;
        }

        if (!Player.AFK.Value && isAFK)
        {
            afkBegan = null;
            isAFK = false;
        }
    }
}
