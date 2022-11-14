// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media;

namespace VRCOSC.Game.Modules.Modules.Media;

public sealed class MediaModule : Module
{
    public override string Title => "Media";
    public override string Description => "Integration with Windows Media";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Media";
    protected override int DeltaUpdate => 2000;
    public override ModuleType ModuleType => ModuleType.Integrations;
    protected override int ChatBoxPriority => 3;

    private readonly MediaProvider mediaProvider = new();
    private bool shouldClear;

    protected override void CreateAttributes()
    {
        CreateSetting(MediaSetting.Display, "Display", "If the song's details should be displayed in VRChat's ChatBox", true);
        CreateSetting(MediaSetting.ChatBoxFormat, "ChatBox Format", "How displaying the song's details should be formatted for the ChatBox.\nAvailable values: %title%, %artist%, %curtime%, %duration%.",
            "[%curtime%/%duration%]                            Now Playing: %artist% - %title%");
        CreateSetting(MediaSetting.ContinuousShow, "Continuous Show", "Should the ChatBox always be showing the song's details? If you want to show the current time, this should be on", true);
        CreateSetting(MediaSetting.DisplayPeriod, "Display Period", "How long should the song's details display for when overwriting the ChatBox (Milliseconds). This is only applicable when Continuous Show is off", 5000);
        CreateSetting(MediaSetting.LaunchList, "Launch List", "What programs to launch on module start", new[] { $@"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe" }, true);
        CreateSetting(MediaSetting.Exclusions, "Program Exclusions", "Which programs should be ignored if they try to take control of media? I.E, Chrome, Spotify, etc...", new[] { "chrome" }, true);

        CreateParameter<bool>(MediaParameter.Play, ParameterMode.ReadWrite, "VRCOSC/Media/Play", "True for playing. False for paused");
        CreateParameter<float>(MediaParameter.Volume, ParameterMode.ReadWrite, "VRCOSC/Media/Volume", "The volume of the process that is controlling the media", ActionMenu.Radial);
        CreateParameter<bool>(MediaParameter.Muted, ParameterMode.ReadWrite, "VRCOSC/Media/Muted", "True to mute. False to unmute");
        CreateParameter<int>(MediaParameter.Repeat, ParameterMode.ReadWrite, "VRCOSC/Media/Repeat", "0 for disabled. 1 for single. 2 for list");
        CreateParameter<bool>(MediaParameter.Shuffle, ParameterMode.ReadWrite, "VRCOSC/Media/Shuffle", "True for enabled. False for disabled");
        CreateParameter<bool>(MediaParameter.Next, ParameterMode.Read, "VRCOSC/Media/Next", "Becoming true causes the next track to play", ActionMenu.Button);
        CreateParameter<bool>(MediaParameter.Previous, ParameterMode.Read, "VRCOSC/Media/Previous", "Becoming true causes the previous track to play", ActionMenu.Button);
    }

    protected override void OnStart()
    {
        mediaProvider.OnMediaSessionOpened += OnMediaSessionOpened;
        mediaProvider.OnMediaUpdate += OnMediaUpdate;
        mediaProvider.ProcessExclusions = GetSetting<List<string>>(MediaSetting.Exclusions);
        mediaProvider.StartMediaHook();

        GetSetting<List<string>>(MediaSetting.LaunchList).ForEach(program =>
        {
            try
            {
                Process.Start(program);
            }
            catch (Exception) { }
        });

        shouldClear = false;
    }

    protected override void OnStop()
    {
        mediaProvider.StopMediaHook();
        mediaProvider.OnMediaSessionOpened -= OnMediaSessionOpened;
        mediaProvider.OnMediaUpdate -= OnMediaUpdate;
    }

    protected override void OnAvatarChange()
    {
        sendVolumeParameters();
        sendMediaParameters();
        display();
    }

    protected override void OnUpdate()
    {
        if (mediaProvider.Controller is not null) mediaProvider.State.Position = mediaProvider.Controller.GetTimelineProperties();

        sendVolumeParameters();

        if (GetSetting<bool>(MediaSetting.ContinuousShow)) display();
    }

    protected override void OnRadialPuppetChange(Enum key, float value)
    {
        switch (key)
        {
            case MediaParameter.Volume:
                mediaProvider.SetVolume(value);
                break;
        }
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case MediaParameter.Play:
                if (value)
                    mediaProvider.Controller?.TryPlayAsync();
                else
                    mediaProvider.Controller?.TryPauseAsync();

                break;

            case MediaParameter.Shuffle:
                mediaProvider.Controller?.TryChangeShuffleActiveAsync(value);
                break;

            case MediaParameter.Muted:
                mediaProvider.SetMuted(value);
                break;
        }
    }

    protected override void OnIntParameterReceived(Enum key, int value)
    {
        switch (key)
        {
            case MediaParameter.Repeat:
                mediaProvider.Controller?.TryChangeAutoRepeatModeAsync((MediaPlaybackAutoRepeatMode)value);
                break;
        }
    }

    protected override void OnButtonPressed(Enum key)
    {
        switch (key)
        {
            case MediaParameter.Next:
                mediaProvider.Controller?.TrySkipNextAsync();
                break;

            case MediaParameter.Previous:
                mediaProvider.Controller?.TrySkipPreviousAsync();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(key), key, null);
        }
    }

    private async void OnMediaSessionOpened()
    {
        // We have to wait a little bit to allow the media app that just opened to take control
        await Task.Delay(500);
        // Playing immediately will cause a media update allowing us to get the media state ASAP
        mediaProvider.Controller?.TryPlayAsync();
        sendMediaParameters();
    }

    private void OnMediaUpdate()
    {
        sendMediaParameters();
        display();
    }

    private void sendMediaParameters()
    {
        SendParameter(MediaParameter.Play, mediaProvider.State.IsPlaying);
        SendParameter(MediaParameter.Shuffle, mediaProvider.State.IsShuffle);
        SendParameter(MediaParameter.Repeat, (int)mediaProvider.State.RepeatMode);
    }

    private void sendVolumeParameters()
    {
        SendParameter(MediaParameter.Volume, mediaProvider.GetVolume());
        SendParameter(MediaParameter.Muted, mediaProvider.IsMuted());
    }

    private void display()
    {
        if (!GetSetting<bool>(MediaSetting.Display)) return;

        if (!mediaProvider.State.IsPlaying)
        {
            if (GetSetting<bool>(MediaSetting.ContinuousShow) && shouldClear) ClearChatBox();
            shouldClear = false;
            return;
        }

        shouldClear = true;

        var formattedText = GetSetting<string>(MediaSetting.ChatBoxFormat)
                            .Replace("%title%", mediaProvider.State.Title)
                            .Replace("%artist%", mediaProvider.State.Artist)
                            .Replace("%curtime%", mediaProvider.State.Position?.Position.ToString(@"mm\:ss") ?? "00:00")
                            .Replace("%duration%", mediaProvider.State.Position?.EndTime.ToString(@"mm\:ss") ?? "00:00");

        SetChatBoxText(formattedText, GetSetting<int>(MediaSetting.DisplayPeriod));
    }

    private enum MediaSetting
    {
        Display,
        ChatBoxFormat,
        DisplayPeriod,
        ContinuousShow,
        LaunchList,
        Exclusions
    }

    private enum MediaParameter
    {
        Play,
        Next,
        Previous,
        Shuffle,
        Repeat,
        Volume,
        Muted,
    }
}
