// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Frameworks;

namespace VRCOSC.Game.Modules.Modules.Discord;

public class DiscordModule : IntegrationModule
{
    public override string Title => "Discord";
    public override string Description => "Integration with the Discord desktop app";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Color4Extensions.FromHex(@"454FBF");
    public override ModuleType Type => ModuleType.Integrations;
    public override string TargetProcess => "discord";

    protected override Dictionary<Enum, WindowsVKey[]> KeyCombinations => new()
    {
        { DiscordInputParameters.DiscordMic, new[] { WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LSHIFT, WindowsVKey.VK_M } },
        { DiscordInputParameters.DiscordDeafen, new[] { WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LSHIFT, WindowsVKey.VK_D } }
    };

    public override void CreateAttributes()
    {
        RegisterInputParameter(DiscordInputParameters.DiscordMic, typeof(bool));
        RegisterInputParameter(DiscordInputParameters.DiscordDeafen, typeof(bool));
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        if (!value) return;

        Terminal.Log($"Received input of {key}");

        ExecuteShortcut(key);
    }
}

public enum DiscordInputParameters
{
    DiscordMic,
    DiscordDeafen
}
