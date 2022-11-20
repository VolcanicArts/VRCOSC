// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Deprecated.Spotify;

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
    protected override int ChatBoxPriority => 2;

    private string currentTitle = null!;

    protected override void CreateAttributes()
    {
        CreateSetting(SpotifySetting.ShouldStart, "Should Start", "Should Spotify start on module start", false);
        CreateSetting(SpotifySetting.ShouldStop, "Should Stop", "Should Spotify stop on module stop", false);
        CreateSetting(SpotifySetting.DisplayTitle, "Display Title", "If the title of the next track should be displayed in VRChat's ChatBox", false);
        CreateSetting(SpotifySetting.TitleFormat, "Title Format", "How displaying the title should be formatted.\nAvailable values: %title%, %author%.", "Now Playing: %author% - %title%");
        CreateSetting(SpotifySetting.InstallLocation, "Install Location", "The location of your spotify.exe file", $@"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe");

        CreateParameter<bool>(SpotifyParameter.PlayPause, ParameterMode.Read, "VRCOSC/Spotify/PlayPause", "True/False for Play/Pause", ActionMenu.Button);
        CreateParameter<bool>(SpotifyParameter.Next, ParameterMode.Read, "VRCOSC/Spotify/Next", "Becomes true to go forward to the next song", ActionMenu.Button);
        CreateParameter<bool>(SpotifyParameter.Previous, ParameterMode.Read, "VRCOSC/Spotify/Previous", "Becomes true to go back to the previous song", ActionMenu.Button);
        CreateParameter<bool>(SpotifyParameter.VolumeUp, ParameterMode.Read, "VRCOSC/Spotify/VolumeUp", "Becomes true to increase the volume", ActionMenu.Button);
        CreateParameter<bool>(SpotifyParameter.VolumeDown, ParameterMode.Read, "VRCOSC/Spotify/VolumeDown", "Becomes true to decrease the volume", ActionMenu.Button);

        RegisterKeyCombination(SpotifyParameter.PlayPause, WindowsVKey.VK_SPACE);
        RegisterKeyCombination(SpotifyParameter.Next, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_RIGHT);
        RegisterKeyCombination(SpotifyParameter.Previous, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LEFT);
        RegisterKeyCombination(SpotifyParameter.VolumeUp, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_UP);
        RegisterKeyCombination(SpotifyParameter.VolumeDown, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_DOWN);
    }

    protected override Task OnStart()
    {
        var shouldStart = GetSetting<bool>(SpotifySetting.ShouldStart);
        if (shouldStart) StartTarget();

        currentTitle = string.Empty;
        displayTitle();

        return Task.CompletedTask;
    }

    protected override Task OnUpdate()
    {
        displayTitle();

        return Task.CompletedTask;
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
            SetChatBoxText(formattedText, chatbox_override_time);
        }
        else
        {
            SetChatBoxText(currentTitle, chatbox_override_time);
        }
    }

    protected override Task OnStop()
    {
        var shouldStop = GetSetting<bool>(SpotifySetting.ShouldStop);
        if (shouldStop) StopTarget();

        return Task.CompletedTask;
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

    private enum SpotifyParameter
    {
        PlayPause,
        Next,
        Previous,
        VolumeUp,
        VolumeDown
    }
}
