// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Spotify;

public sealed class SpotifyModule : IntegrationModule
{
    private const int chatbox_override_time = 5000;

    public override string Title => "Spotify";
    public override string Description => "THIS MODULE IS DEPRECATED. OS Media integration has been implemented. Use Media instead";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.Integrations;
    protected override int DeltaUpdate => GetSetting<bool>(SpotifySetting.DisplayTitle) ? 5000 : int.MaxValue;
    protected override string TargetProcess => "spotify";
    protected override string TargetExe => GetSetting<string>(SpotifySetting.InstallLocation);

    private string currentTitle = null!;

    protected override void CreateAttributes()
    {
        CreateSetting(SpotifySetting.ShouldStart, "Should Start", "Should Spotify start on module start", false);
        CreateSetting(SpotifySetting.ShouldStop, "Should Stop", "Should Spotify stop on module stop", false);
        CreateSetting(SpotifySetting.DisplayTitle, "Display Title", "If the title of the next track should be displayed in VRChat's ChatBox", false);
        CreateSetting(SpotifySetting.TitleFormat, "Title Format", "How displaying the title should be formatted.\nAvailable values: %title%, %author%.", "Now Playing: %author% - %title%");
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
        var process = GetTargetProgress();
        if (process is null) return;

        var newTitle = process.MainWindowTitle;

        if (newTitle.Contains("spotify", StringComparison.InvariantCultureIgnoreCase))
        {
            currentTitle = string.Empty;
            return;
        }

        if (newTitle == currentTitle) return;

        currentTitle = newTitle;

        if (currentTitle.Contains('-'))
        {
            var titleData = currentTitle.Split(new[] { '-' }, 2);
            var author = titleData[0].Trim();
            var title = titleData[1].Trim();
            var formattedText = GetSetting<string>(SpotifySetting.TitleFormat).Replace("%title%", title).Replace("%author%", author);
            ChatBox.SetText(formattedText, true, ChatBoxPriority.Override, chatbox_override_time);
        }
        else
        {
            ChatBox.SetText(currentTitle, true, ChatBoxPriority.Override, chatbox_override_time);
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
