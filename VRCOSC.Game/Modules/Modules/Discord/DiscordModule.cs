// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Discord;

public class DiscordModule : IntegrationModule
{
    public override string Title => "Discord";
    public override string Description => "Integration with the Discord desktop app";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Color4Extensions.FromHex(@"454FBF");
    public override ModuleType ModuleType => ModuleType.Integrations;
    public override string TargetProcess => "discord";

    public override void CreateAttributes()
    {
        RegisterInputParameter(DiscordInputParameter.DiscordMic, typeof(bool));
        RegisterInputParameter(DiscordInputParameter.DiscordDeafen, typeof(bool));

        RegisterKeyCombination(DiscordInputParameter.DiscordMic, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LSHIFT, WindowsVKey.VK_M);
        RegisterKeyCombination(DiscordInputParameter.DiscordDeafen, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LSHIFT, WindowsVKey.VK_D);
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        if (!value) return;

        Terminal.Log($"Received input of {key}");

        ExecuteShortcut(key);
    }

    private enum DiscordInputParameter
    {
        DiscordMic,
        DiscordDeafen
    }
}
