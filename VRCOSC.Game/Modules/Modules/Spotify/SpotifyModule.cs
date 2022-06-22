// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Spotify;

public class SpotifyModule : IntegrationModule
{
    public override string Title => "Spotify";
    public override string Description => "Integration with the Spotify desktop app";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Spotify";
    public override Colour4 Colour => Color4Extensions.FromHex(@"1ed760").Darken(0.5f);
    public override ModuleType ModuleType => ModuleType.Integrations;
    protected override string TargetProcess => "spotify";
    protected override string TargetExe => GetSetting<string>(SpotifySetting.InstallLocation);

    public override void CreateAttributes()
    {
        CreateSetting(SpotifySetting.ShouldStart, "Should Start", "Should Spotify start on module start", false);
        CreateSetting(SpotifySetting.ShouldStop, "Should Stop", "Should Spotify stop on module stop", false);
        CreateSetting(SpotifySetting.InstallLocation, "Install Location", "The location of your spotify.exe file", $@"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe");

        RegisterInputParameter<bool>(SpotifyInputParameter.SpotifyPlayPause, ActionMenu.Button);
        RegisterInputParameter<bool>(SpotifyInputParameter.SpotifyNext, ActionMenu.Button);
        RegisterInputParameter<bool>(SpotifyInputParameter.SpotifyPrevious, ActionMenu.Button);
        RegisterInputParameter<bool>(SpotifyInputParameter.SpotifyVolumeUp, ActionMenu.Button);
        RegisterInputParameter<bool>(SpotifyInputParameter.SpotifyVolumeDown, ActionMenu.Button);

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

    public override void Stop()
    {
        var shouldStop = GetSetting<bool>(SpotifySetting.ShouldStop);
        if (shouldStop) StopTarget();
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        ExecuteKeyCombination(key);
    }

    private enum SpotifySetting
    {
        ShouldStart,
        ShouldStop,
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
