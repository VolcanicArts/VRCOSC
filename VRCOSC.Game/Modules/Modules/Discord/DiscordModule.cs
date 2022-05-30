// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Frameworks;

namespace VRCOSC.Game.Modules.Modules.Discord;

public class Discord : IntegrationModule
{
    public override string Title => "Discord";
    public override string Description => "Integration with the Discord desktop app";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Color4Extensions.FromHex(@"454FBF");
    public override ModuleType Type => ModuleType.Integrations;

    public override IReadOnlyCollection<Enum> InputParameters => new List<Enum>
    {
        DiscordInputParameters.DiscordMic,
        DiscordInputParameters.DiscordDeafen
    };

    protected override string TargetProcess => "discord";

    protected override IReadOnlyDictionary<Enum, int[]> KeyCombinations => new Dictionary<Enum, int[]>()
    {
        { DiscordInputParameters.DiscordMic, new[] { 0xA2, 0xA0, 0x4D } },
        { DiscordInputParameters.DiscordDeafen, new[] { 0xA2, 0xA0, 0x44 } }
    };

    protected override void OnParameterReceived(Enum key, object value)
    {
        var buttonPressed = (bool)value;
        if (!buttonPressed) return;

        Terminal.Log($"Received input of {key}");

        ExecuteTask(key).ConfigureAwait(false);
    }
}

public enum DiscordInputParameters
{
    DiscordMic,
    DiscordDeafen
}
