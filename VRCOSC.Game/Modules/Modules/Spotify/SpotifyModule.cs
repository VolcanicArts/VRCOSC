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

    private string currentTitle = null!;

    protected override void CreateAttributes()
    {
        CreateSetting(SpotifySetting.ShouldStart, "Should Start", "Should Spotify start on module start", false);
        CreateSetting(SpotifySetting.ShouldStop, "Should Stop", "Should Spotify stop on module stop", false);
        CreateSetting(SpotifySetting.DisplayTitle, "Display Title", "If the title of the next track should be displayed in VRChat's ChatBox", false);
        CreateSetting(SpotifySetting.TitleFormat, "Title Format", "How displaying the title should be formatted. `%title%` for the title", "Now Playing: %title%");
        CreateSetting(SpotifySetting.InstallLocation, "Install Location", "The location of your spotify.exe file", $@"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe");

        RegisterButtonInput(SpotifyIncomingParameter.PlayPause, "VRCOSC/Spotify/PlayPause");
        RegisterButtonInput(SpotifyIncomingParameter.Next, "VRCOSC/Spotify/Next");
        RegisterButtonInput(SpotifyIncomingParameter.Previous, "VRCOSC/Spotify/Previous");
        RegisterButtonInput(SpotifyIncomingParameter.VolumeUp, "VRCOSC/Spotify/VolumeUp");
        RegisterButtonInput(SpotifyIncomingParameter.VolumeDown, "VRCOSC/Spotify/VolumeDown");

        RegisterKeyCombination(SpotifyIncomingParameter.PlayPause, WindowsVKey.VK_SPACE);
        RegisterKeyCombination(SpotifyIncomingParameter.Next, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_RIGHT);
        RegisterKeyCombination(SpotifyIncomingParameter.Previous, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LEFT);
        RegisterKeyCombination(SpotifyIncomingParameter.VolumeUp, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_UP);
        RegisterKeyCombination(SpotifyIncomingParameter.VolumeDown, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_DOWN);
    }

    protected override void OnStart()
    {
        var shouldStart = GetSetting<bool>(SpotifySetting.ShouldStart);
        if (shouldStart) StartTarget();

        currentTitle = string.Empty;
        displayTitle();
    }

    protected override void OnUpdate()
    {
        displayTitle();
    }

    private void displayTitle()
    {
        if (GetSetting<bool>(SpotifySetting.DisplayTitle))
        {
            var process = GetTargetProgress();
            if (process is null) return;

            var newTitle = process.MainWindowTitle;

            if (newTitle.Contains("spotify", StringComparison.InvariantCultureIgnoreCase))
            {
                currentTitle = string.Empty;
                return;
            }

            if (newTitle != currentTitle)
            {
                currentTitle = newTitle;
                ChatBox.SetText(GetSetting<string>(SpotifySetting.TitleFormat).Replace("%title%", currentTitle), true, ChatBoxPriority.Override);
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
        DisplayTitle,
        TitleFormat
    }

    private enum SpotifyIncomingParameter
    {
        PlayPause,
        Next,
        Previous,
        VolumeUp,
        VolumeDown
    }
}
