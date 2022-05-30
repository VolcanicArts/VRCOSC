// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Frameworks;

namespace VRCOSC.Game.Modules.Modules.Spotify;

public class SpotifyModule : IntegrationModule
{
    public override string Title => "Spotify";
    public override string Description => "Integration with the Spotify desktop app";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Colour4.Green.Darken(0.5f);
    public override ModuleType Type => ModuleType.Integrations;

    public override IReadOnlyCollection<Enum> InputParameters => new List<Enum>
    {
        SpotifyInputParameters.SpotifyPlayPause,
        SpotifyInputParameters.SpotifyNext,
        SpotifyInputParameters.SpotifyPrevious,
        SpotifyInputParameters.SpotifyVolumeUp,
        SpotifyInputParameters.SpotifyVolumeDown
    };

    protected override string TargetProcess => "spotify";

    protected override IReadOnlyDictionary<Enum, int[]> KeyCombinations => new Dictionary<Enum, int[]>()
    {
        { SpotifyInputParameters.SpotifyPlayPause, new[] { 0x20 } },
        { SpotifyInputParameters.SpotifyNext, new[] { 0xA2, 0x27 } },
        { SpotifyInputParameters.SpotifyPrevious, new[] { 0xA2, 0x25 } },
        { SpotifyInputParameters.SpotifyVolumeUp, new[] { 0xA2, 0x26 } },
        { SpotifyInputParameters.SpotifyVolumeDown, new[] { 0xA2, 0x28 } }
    };

    protected override void OnParameterReceived(Enum key, object value)
    {
        var buttonPressed = (bool)value;
        if (!buttonPressed) return;

        Terminal.Log($"Received input of {key}");

        ExecuteTask((SpotifyInputParameters)key).ConfigureAwait(false);
    }
}

public enum SpotifyInputParameters
{
    SpotifyPlayPause,
    SpotifyNext,
    SpotifyPrevious,
    SpotifyVolumeUp,
    SpotifyVolumeDown
}
