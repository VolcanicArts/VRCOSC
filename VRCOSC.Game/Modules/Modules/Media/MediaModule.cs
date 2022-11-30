// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media;

namespace VRCOSC.Game.Modules.Modules.Media;

public sealed class MediaModule : ChatBoxModule
{
    public override string Title => "Media";
    public override string Description => "Integration with Windows Media";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Media";
    protected override int DeltaUpdate => 2000;
    public override ModuleType ModuleType => ModuleType.Integrations;
    protected override int ChatBoxPriority => 3;

    protected override bool DefaultChatBoxDisplay => true;
    protected override string DefaultChatBoxFormat => "[%curtime%/%duration%]                            Now Playing: %artist% - %title%";
    protected override IEnumerable<string> ChatBoxFormatValues => new[] { "%title%", "%artist%", "%curtime%", "%duration%" };

    private readonly MediaProvider mediaProvider = new();

    protected override void CreateAttributes()
    {
        base.CreateAttributes();

        CreateSetting(MediaSetting.StartList, "Start List", "A list of exe locations to start with this module", new[] { @$"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe" }, true);

        CreateParameter<bool>(MediaParameter.Play, ParameterMode.ReadWrite, "VRCOSC/Media/Play", "True for playing. False for paused");
        CreateParameter<float>(MediaParameter.Volume, ParameterMode.ReadWrite, "VRCOSC/Media/Volume", "The volume of the process that is controlling the media", ActionMenu.Radial);
        CreateParameter<bool>(MediaParameter.Muted, ParameterMode.ReadWrite, "VRCOSC/Media/Muted", "True to mute. False to unmute");
        CreateParameter<int>(MediaParameter.Repeat, ParameterMode.ReadWrite, "VRCOSC/Media/Repeat", "0 for disabled. 1 for single. 2 for list");
        CreateParameter<bool>(MediaParameter.Shuffle, ParameterMode.ReadWrite, "VRCOSC/Media/Shuffle", "True for enabled. False for disabled");
        CreateParameter<bool>(MediaParameter.Next, ParameterMode.Read, "VRCOSC/Media/Next", "Becoming true causes the next track to play", ActionMenu.Button);
        CreateParameter<bool>(MediaParameter.Previous, ParameterMode.Read, "VRCOSC/Media/Previous", "Becoming true causes the previous track to play", ActionMenu.Button);
    }

    protected override string? GetChatBoxText()
    {
        // TODO Add setting to either show [Paused] when paused, or return null when paused
        // This should recreate functionality before of pausing turning off the ChatBox and allowing lower priorities
        if (!mediaProvider.State.IsPlaying) return string.Empty;

        mediaProvider.State.Position = mediaProvider.Controller?.GetTimelineProperties() ?? null;

        var formattedText = GetSetting<string>(ChatBoxSetting.ChatBoxFormat)
                            .Replace("%title%", mediaProvider.State.Title)
                            .Replace("%artist%", mediaProvider.State.Artist)
                            .Replace("%curtime%", mediaProvider.State.Position?.Position.ToString(@"mm\:ss") ?? "00:00")
                            .Replace("%duration%", mediaProvider.State.Position?.EndTime.ToString(@"mm\:ss") ?? "00:00");

        return formattedText;
    }

    protected override async Task OnStart(CancellationToken cancellationToken)
    {
        await base.OnStart(cancellationToken);
        mediaProvider.OnMediaSessionOpened += OnMediaSessionOpened;
        mediaProvider.OnMediaUpdate += OnMediaUpdate;
        await mediaProvider.StartMediaHook();
        startProcesses();
    }

    private void startProcesses()
    {
        GetSetting<List<string>>(MediaSetting.StartList).ForEach(processName =>
        {
            if (!Process.GetProcessesByName(processName).Any()) Process.Start(processName);
        });
    }

    protected override async Task OnStop()
    {
        await base.OnStop();
        mediaProvider.StopMediaHook();
        mediaProvider.OnMediaSessionOpened -= OnMediaSessionOpened;
        mediaProvider.OnMediaUpdate -= OnMediaUpdate;
    }

    protected override void OnAvatarChange()
    {
        sendVolumeParameters();
        sendMediaParameters();
    }

    protected override Task OnUpdate()
    {
        sendVolumeParameters();
        return Task.CompletedTask;
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

    private enum MediaSetting
    {
        StartList
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
