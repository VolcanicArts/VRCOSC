// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
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
    public override ModuleType ModuleType => ModuleType.Integrations;
    public override string TargetProcess => "spotify";
    public override string TargetExe => GetSetting<string>(SpotifySetting.InstallLocation);

    public override void CreateAttributes()
    {
        CreateSetting(SpotifySetting.ShouldStart, "Should Start", "Should Spotify start on module run", false);
        CreateSetting(SpotifySetting.InstallLocation, "Install Location", "The location of your spotify.exe file", $@"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe");

        RegisterInputParameter(SpotifyInputParameter.SpotifyPlayPause, typeof(bool));
        RegisterInputParameter(SpotifyInputParameter.SpotifyNext, typeof(bool));
        RegisterInputParameter(SpotifyInputParameter.SpotifyPrevious, typeof(bool));
        RegisterInputParameter(SpotifyInputParameter.SpotifyVolumeUp, typeof(bool));
        RegisterInputParameter(SpotifyInputParameter.SpotifyVolumeDown, typeof(bool));

        RegisterKeyCombination(SpotifyInputParameter.SpotifyPlayPause, WindowsVKey.VK_SPACE);
        RegisterKeyCombination(SpotifyInputParameter.SpotifyNext, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_RIGHT);
        RegisterKeyCombination(SpotifyInputParameter.SpotifyPrevious, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LEFT);
        RegisterKeyCombination(SpotifyInputParameter.SpotifyVolumeUp, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_UP);
        RegisterKeyCombination(SpotifyInputParameter.SpotifyVolumeDown, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_DOWN);
    }

    public override void Start()
    {
        var shouldStart = GetSetting<bool>(SpotifySetting.ShouldStart);
        if (shouldStart) StartTarget();
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        if (!value) return;

        Terminal.Log($"Received input of {key}");

        ExecuteShortcut(key);
    }

    private enum SpotifySetting
    {
        ShouldStart,
        InstallLocation
    }

    private enum SpotifyInputParameter
    {
        SpotifyPlayPause,
        SpotifyNext,
        SpotifyPrevious,
        SpotifyVolumeUp,
        SpotifyVolumeDown
    }
}
