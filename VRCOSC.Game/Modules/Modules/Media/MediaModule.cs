// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Media;

namespace VRCOSC.Game.Modules.Modules.Media;

public sealed partial class MediaModule : ChatBoxModule
{
    public override string Title => "Media";
    public override string Description => "Integration with Windows Media";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Media";
    protected override int DeltaUpdate => 2000;
    public override ModuleType Type => ModuleType.Integrations;
    protected override int ChatBoxPriority => 2;

    protected override bool DefaultChatBoxDisplay => true;
    protected override string DefaultChatBoxFormat => "[%curtime%/%duration%]                            Now Playing: %artist% - %title%";
    protected override IEnumerable<string> ChatBoxFormatValues => new[] { "%title%", "%artist%", "%curtime%", "%duration%" };

    private readonly MediaProvider mediaProvider = new();

    protected override void CreateAttributes()
    {
        CreateSetting(MediaSetting.PausedBehaviour, "Paused Behaviour", "When the media is paused, should the ChatBox be empty or display that it's paused?", MediaPausedBehaviour.Empty);
        CreateSetting(MediaSetting.PausedText, "Paused Text", $"The text to display when media is paused. Only applicable when Paused Behaviour is set to {MediaPausedBehaviour.Display}", "[Paused]",
            () => GetSetting<MediaPausedBehaviour>(MediaSetting.PausedBehaviour) == MediaPausedBehaviour.Display);
        CreateSetting(MediaSetting.StartList, "Start List", "A list of exe locations to start with this module. This is handy for starting media apps on module start. For example, Spotify", new[] { @$"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe" }, true);

        base.CreateAttributes();

        CreateParameter<bool>(MediaParameter.Play, ParameterMode.ReadWrite, "VRCOSC/Media/Play", "True for playing. False for paused");
        CreateParameter<float>(MediaParameter.Volume, ParameterMode.ReadWrite, "VRCOSC/Media/Volume", "The volume of the process that is controlling the media");
        CreateParameter<bool>(MediaParameter.Muted, ParameterMode.ReadWrite, "VRCOSC/Media/Muted", "True to mute. False to unmute");
        CreateParameter<int>(MediaParameter.Repeat, ParameterMode.ReadWrite, "VRCOSC/Media/Repeat", "0 for disabled. 1 for single. 2 for list");
        CreateParameter<bool>(MediaParameter.Shuffle, ParameterMode.ReadWrite, "VRCOSC/Media/Shuffle", "True for enabled. False for disabled");
        CreateParameter<bool>(MediaParameter.Next, ParameterMode.Read, "VRCOSC/Media/Next", "Becoming true causes the next track to play");
        CreateParameter<bool>(MediaParameter.Previous, ParameterMode.Read, "VRCOSC/Media/Previous", "Becoming true causes the previous track to play");
    }

    protected override string? GetChatBoxText()
    {
        if (mediaProvider.Controller is null) return null;

        if (!mediaProvider.State.IsPlaying)
            return GetSetting<MediaPausedBehaviour>(MediaSetting.PausedBehaviour) == MediaPausedBehaviour.Empty ? null : GetSetting<string>(MediaSetting.PausedText);

        mediaProvider.State.Position = mediaProvider.Controller?.GetTimelineProperties();

        var formattedText = GetSetting<string>(ChatBoxSetting.ChatBoxFormat)
                            .Replace("%title%", mediaProvider.State.Title)
                            .Replace("%artist%", mediaProvider.State.Artist)
                            .Replace("%curtime%", mediaProvider.State.Position?.Position.ToString(@"mm\:ss") ?? "00:00")
                            .Replace("%duration%", mediaProvider.State.Position?.EndTime.ToString(@"mm\:ss") ?? "00:00");

        return formattedText;
    }

    protected override void OnModuleStart()
    {
        base.OnModuleStart();
        mediaProvider.OnMediaUpdate += OnMediaUpdate;
        mediaProvider.StartMediaHook();
        startProcesses();
    }

    private void startProcesses()
    {
        GetSetting<List<string>>(MediaSetting.StartList).ForEach(processExeLocation =>
        {
            if (File.Exists(processExeLocation))
            {
                var processName = new FileInfo(processExeLocation).Name.ToLowerInvariant().Replace(".exe", string.Empty);
                if (!Process.GetProcessesByName(processName).Any()) Process.Start(processExeLocation);
            }
        });
    }

    protected override void OnModuleStop()
    {
        mediaProvider.StopMediaHook();
        mediaProvider.OnMediaUpdate -= OnMediaUpdate;
    }

    protected override void OnAvatarChange()
    {
        sendVolumeParameters();
        sendMediaParameters();
    }

    protected override void OnModuleUpdate()
    {
        sendVolumeParameters();
    }

    protected override void OnFloatParameterReceived(Enum key, float value)
    {
        switch (key)
        {
            case MediaParameter.Volume when mediaProvider.Controller is not null:
                mediaProvider.State.Volume = value;
                break;
        }
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case MediaParameter.Play when value:
                mediaProvider.Controller?.TryPlayAsync();
                break;

            case MediaParameter.Play when !value:
                mediaProvider.Controller?.TryPauseAsync();
                break;

            case MediaParameter.Shuffle:
                mediaProvider.Controller?.TryChangeShuffleActiveAsync(value);
                break;

            case MediaParameter.Muted when mediaProvider.Controller is not null:
                mediaProvider.State.Muted = value;
                break;

            case MediaParameter.Next when value:
                mediaProvider.Controller?.TrySkipNextAsync();
                break;

            case MediaParameter.Previous when value:
                mediaProvider.Controller?.TrySkipPreviousAsync();
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
        SendParameter(MediaParameter.Volume, mediaProvider.State.Volume);
        SendParameter(MediaParameter.Muted, mediaProvider.State.Muted);
    }

    private enum MediaSetting
    {
        PausedBehaviour,
        PausedText,
        StartList
    }

    private enum MediaPausedBehaviour
    {
        Empty,
        Display
    }

    private enum MediaParameter
    {
        Play,
        Next,
        Previous,
        Shuffle,
        Repeat,
        Volume,
        Muted
    }
}
