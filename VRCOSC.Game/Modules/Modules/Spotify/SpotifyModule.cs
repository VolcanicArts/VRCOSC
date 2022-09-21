// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Spotify;

public sealed class SpotifyModule : IntegrationModule
{
    public override string Title => "Spotify";
    public override string Description => "Integration with the Spotify desktop app";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Spotify";
    public override ModuleType ModuleType => ModuleType.Integrations;
    protected override int DeltaUpdate => 5000;
    protected override string TargetProcess => "spotify";
    protected override string TargetExe => GetSetting<string>(SpotifySetting.InstallLocation);

    private string currentTitle = string.Empty;

    protected override void CreateAttributes()
    {
        CreateSetting(SpotifySetting.ShouldStart, "Should Start", "Should Spotify start on module start", false);
        CreateSetting(SpotifySetting.ShouldStop, "Should Stop", "Should Spotify stop on module stop", false);
        CreateSetting(SpotifySetting.DisplayTitle, "Display Title", "If the title of the next track should be displayed in VRChat's ChatBox", false);
        CreateSetting(SpotifySetting.InstallLocation, "Install Location", "The location of your spotify.exe file", $@"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe");

        RegisterButtonInput(SpotifyInputParameter.SpotifyPlayPause);
        RegisterButtonInput(SpotifyInputParameter.SpotifyNext);
        RegisterButtonInput(SpotifyInputParameter.SpotifyPrevious);
        RegisterButtonInput(SpotifyInputParameter.SpotifyVolumeUp);
        RegisterButtonInput(SpotifyInputParameter.SpotifyVolumeDown);

        RegisterKeyCombination(SpotifyInputParameter.SpotifyPlayPause, WindowsVKey.VK_SPACE);
        RegisterKeyCombination(SpotifyInputParameter.SpotifyNext, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_RIGHT);
        RegisterKeyCombination(SpotifyInputParameter.SpotifyPrevious, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LEFT);
        RegisterKeyCombination(SpotifyInputParameter.SpotifyVolumeUp, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_UP);
        RegisterKeyCombination(SpotifyInputParameter.SpotifyVolumeDown, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_DOWN);
    }

    protected override void OnStart()
    {
        var shouldStart = GetSetting<bool>(SpotifySetting.ShouldStart);
        if (shouldStart) StartTarget();
    }

    protected override void OnUpdate()
    {
        if (GetSetting<bool>(SpotifySetting.DisplayTitle))
        {
            var process = GetTargetProgress();
            if (process is null) return;

            var newTitle = process.MainWindowTitle;
            if (newTitle.Contains("spotify", StringComparison.InvariantCultureIgnoreCase)) newTitle = "Nothing";

            if (newTitle != currentTitle)
            {
                currentTitle = newTitle;
                SetChatBoxText($"Now Playing: {currentTitle}", true);
            }
        }
    }

    protected override void OnStop()
    {
        var shouldStop = GetSetting<bool>(SpotifySetting.ShouldStop);
        if (shouldStop) StopTarget();
    }

    protected override void OnButtonPressed(Enum key)
    {
        ExecuteKeyCombination(key);
    }

    private enum SpotifySetting
    {
        ShouldStart,
        ShouldStop,
        InstallLocation,
        DisplayTitle
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
