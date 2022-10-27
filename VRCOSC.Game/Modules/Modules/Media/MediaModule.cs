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

    private readonly MediaProvider mediaProvider = new();
    private bool shouldClear;

    protected override void CreateAttributes()
    {
        CreateSetting(MediaSetting.Display, "Display", "If the song's details should be displayed in VRChat's ChatBox", true);
        CreateSetting(MediaSetting.ChatBoxFormat, "ChatBox Format", "How displaying the song's details should be formatted for the ChatBox.\nAvailable values: %title%, %artist%, %curtime%, %duration%.",
            "[%curtime%/%duration%]                            Now Playing: %artist% - %title%");
        CreateSetting(MediaSetting.ContinuousShow, "Continuous Show", "Should the ChatBox always be showing the song's details? If you want to show the current time, this should be on", true);
        CreateSetting(MediaSetting.DisplayPeriod, "Display Period", "How long should the song's details display for when overwriting the ChatBox (Milliseconds). This is only applicable when Continuous Show is off", 5000);
        CreateSetting(MediaSetting.LaunchList, "Launch List", "What programs to launch on module start", new[] { $@"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe" });
        CreateSetting(MediaSetting.Exclusions, "Program Exclusions", "Which programs should be ignored if they try to take control of media? I.E, Chrome, Spotify, etc...", new[] { "chrome" });

        CreateOutgoingParameter(MediaOutgoingParameter.Repeat, "Repeat Mode", "The repeat mode of the current controller", "/avatar/parameters/VRCOSC/Media/Repeat");
        CreateOutgoingParameter(MediaOutgoingParameter.Shuffle, "Shuffle", "Whether shuffle is enabled in the current controller", "/avatar/parameters/VRCOSC/Media/Shuffle");
        CreateOutgoingParameter(MediaOutgoingParameter.Play, "Play", "Whether the song is currently playing or not", "/avatar/parameters/VRCOSC/Media/Play");
        CreateOutgoingParameter(MediaOutgoingParameter.Volume, "Volume", "The volume of the process that is controlling the media", "/avatar/parameters/VRCOSC/Media/Volume");
        CreateOutgoingParameter(MediaOutgoingParameter.Muted, "Mute", "Whether the volume of the process that is controlling the media is muted", "/avatar/parameters/VRCOSC/Media/Muted");

        RegisterButtonInput(MediaIncomingParameter.Next, "VRCOSC/Media/Next");
        RegisterButtonInput(MediaIncomingParameter.Previous, "VRCOSC/Media/Previous");
        RegisterRadialInput(MediaIncomingParameter.Volume, "VRCOSC/Media/Volume");
        RegisterIncomingParameter<bool>(MediaIncomingParameter.Play, "VRCOSC/Media/Play");
        RegisterIncomingParameter<int>(MediaIncomingParameter.Repeat, "VRCOSC/Media/Repeat");
        RegisterIncomingParameter<bool>(MediaIncomingParameter.Shuffle, "VRCOSC/Media/Shuffle");
        RegisterIncomingParameter<bool>(MediaIncomingParameter.Muted, "VRCOSC/Media/Muted");
    }

    protected override void OnStart()
    {
        base.OnStart();

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

    protected override void OnRadialPuppetChange(Enum key, VRChatRadialPuppet radialData)
    {
        switch (key)
        {
            case MediaIncomingParameter.Volume:
                mediaProvider.SetVolume(radialData.Value);
                break;
        }
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case MediaIncomingParameter.Play:
                if (value)
                    mediaProvider.Controller?.TryPlayAsync();
                else
                    mediaProvider.Controller?.TryPauseAsync();

                break;

            case MediaIncomingParameter.Shuffle:
                mediaProvider.Controller?.TryChangeShuffleActiveAsync(value);
                break;

            case MediaIncomingParameter.Muted:
                mediaProvider.SetMuted(value);
                break;
        }
    }

    protected override void OnIntParameterReceived(Enum key, int value)
    {
        switch (key)
        {
            case MediaIncomingParameter.Repeat:
                mediaProvider.Controller?.TryChangeAutoRepeatModeAsync((MediaPlaybackAutoRepeatMode)value);
                break;
        }
    }

    protected override void OnButtonPressed(Enum key)
    {
        switch (key)
        {
            case MediaIncomingParameter.Next:
                mediaProvider.Controller?.TrySkipNextAsync();
                break;

            case MediaIncomingParameter.Previous:
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
        SendParameter(MediaOutgoingParameter.Play, mediaProvider.State.IsPlaying);
        SendParameter(MediaOutgoingParameter.Shuffle, mediaProvider.State.IsShuffle);
        SendParameter(MediaOutgoingParameter.Repeat, (int)mediaProvider.State.RepeatMode);
    }

    private void sendVolumeParameters()
    {
        SendParameter(MediaOutgoingParameter.Volume, mediaProvider.GetVolume());
        SendParameter(MediaOutgoingParameter.Muted, mediaProvider.IsMuted());
    }

    private void display()
    {
        if (!GetSetting<bool>(MediaSetting.Display)) return;

        if (!mediaProvider.State.IsPlaying)
        {
            if (GetSetting<bool>(MediaSetting.ContinuousShow) && shouldClear) ChatBox.Clear();
            shouldClear = false;
            return;
        }

        shouldClear = true;

        var formattedText = GetSetting<string>(MediaSetting.ChatBoxFormat)
                            .Replace("%title%", mediaProvider.State.Title ?? "Unknown")
                            .Replace("%artist%", mediaProvider.State.Artist ?? "Unknown")
                            .Replace("%curtime%", mediaProvider.State.Position?.Position.ToString(@"mm\:ss") ?? "00:00")
                            .Replace("%duration%", mediaProvider.State.Position?.EndTime.ToString(@"mm\:ss") ?? "00:00");

        ChatBox.SetText(formattedText, true, ChatBoxPriority.Override, GetSetting<int>(MediaSetting.DisplayPeriod));
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

    private enum MediaIncomingParameter
    {
        Play,
        Next,
        Previous,
        Shuffle,
        Repeat,
        Volume,
        Muted,
    }

    private enum MediaOutgoingParameter
    {
        Play,
        Shuffle,
        Repeat,
        Volume,
        Muted
    }
}
