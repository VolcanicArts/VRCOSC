// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Frameworks;

namespace VRCOSC.Game.Modules.Modules.Spotify;

public class SpotifyModule : IntegrationModule
{
    public override string Title => "Spotify";
    public override string Description => "Integration with the Spotify desktop app";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Color4Extensions.FromHex(@"1ed760").Darken(0.5f);
    public override ModuleType Type => ModuleType.Integrations;
    public override string TargetProcess => "spotify";
    public override string TargetExe => $@"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe";

    protected override Dictionary<Enum, (string, string, object)> Settings => new()
    {
        { SpotifySettings.ShouldStart, ("Should Start", "Should Spotify start on module run?", true) }
    };

    protected override List<Enum> InputParameters => new()
    {
        SpotifyInputParameters.SpotifyPlayPause,
        SpotifyInputParameters.SpotifyNext,
        SpotifyInputParameters.SpotifyPrevious,
        SpotifyInputParameters.SpotifyVolumeUp,
        SpotifyInputParameters.SpotifyVolumeDown
    };

    protected override Dictionary<Enum, WindowsVKey[]> KeyCombinations => new()
    {
        { SpotifyInputParameters.SpotifyPlayPause, new[] { WindowsVKey.VK_SPACE } },
        { SpotifyInputParameters.SpotifyNext, new[] { WindowsVKey.VK_LCONTROL, WindowsVKey.VK_RIGHT } },
        { SpotifyInputParameters.SpotifyPrevious, new[] { WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LEFT } },
        { SpotifyInputParameters.SpotifyVolumeUp, new[] { WindowsVKey.VK_LCONTROL, WindowsVKey.VK_UP } },
        { SpotifyInputParameters.SpotifyVolumeDown, new[] { WindowsVKey.VK_LCONTROL, WindowsVKey.VK_DOWN } }
    };

    protected override void OnStart()
    {
        var shouldStart = GetSettingAs<bool>(SpotifySettings.ShouldStart);
        if (shouldStart) StartTarget();
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        if (!value) return;

        Terminal.Log($"Received input of {key}");

        ExecuteShortcut(key);
    }
}

public enum SpotifySettings
{
    ShouldStart
}

public enum SpotifyInputParameters
{
    SpotifyPlayPause,
    SpotifyNext,
    SpotifyPrevious,
    SpotifyVolumeUp,
    SpotifyVolumeDown
}
