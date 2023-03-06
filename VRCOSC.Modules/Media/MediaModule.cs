// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using Windows.Media;
using osu.Framework.Bindables;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Providers.Media;

namespace VRCOSC.Modules.Media;

public sealed partial class MediaModule : ChatBoxModule
{
    public override string Title => "Media";
    public override string Description => "Integration with Windows Media";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Media";
    protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(2);
    public override ModuleType Type => ModuleType.Integrations;
    protected override int ChatBoxPriority => 2;

    protected override string DefaultChatBoxFormat => @"[%curtime%/%duration%]                            Now Playing: %artist% - %title%";
    protected override IEnumerable<string> ChatBoxFormatValues => new[] { @"%title%", @"%artist%", @"%curtime%", @"%duration%", @"%volume%" };

    private readonly WindowsMediaProvider mediaProvider = new();
    private readonly Bindable<bool> currentlySeeking = new();
    private TimeSpan targetPosition;

    public MediaModule()
    {
        mediaProvider.OnPlaybackStateUpdate += onPlaybackStateUpdate;
    }

    protected override void CreateAttributes()
    {
        CreateSetting(MediaSetting.PausedBehaviour, "Paused Behaviour", "When the media is paused, should the ChatBox be empty or display that it's paused?", MediaPausedBehaviour.Empty);
        CreateSetting(MediaSetting.PausedText, "Paused Text", $"The text to display when media is paused. Only applicable when Paused Behaviour is set to {MediaPausedBehaviour.Display}", "[Paused]", () => GetSetting<MediaPausedBehaviour>(MediaSetting.PausedBehaviour) == MediaPausedBehaviour.Display);
        CreateSetting(MediaSetting.StartList, "Start List", "A list of exe locations to start with this module. This is handy for starting media apps on module start. For example, Spotify", new[] { @$"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe" }, true);

        base.CreateAttributes();

        CreateParameter<bool>(MediaParameter.Play, ParameterMode.ReadWrite, @"VRCOSC/Media/Play", "Play/Pause", @"True for playing. False for paused");
        CreateParameter<float>(MediaParameter.Volume, ParameterMode.ReadWrite, @"VRCOSC/Media/Volume", "Volume", @"The volume of the process that is controlling the media");
        CreateParameter<int>(MediaParameter.Repeat, ParameterMode.ReadWrite, @"VRCOSC/Media/Repeat", "Repeat", @"0 for disabled. 1 for single. 2 for list");
        CreateParameter<bool>(MediaParameter.Shuffle, ParameterMode.ReadWrite, @"VRCOSC/Media/Shuffle", "Shuffle", @"True for enabled. False for disabled");
        CreateParameter<bool>(MediaParameter.Next, ParameterMode.Read, @"VRCOSC/Media/Next", "Next", @"Becoming true causes the next track to play");
        CreateParameter<bool>(MediaParameter.Previous, ParameterMode.Read, @"VRCOSC/Media/Previous", "Previous", @"Becoming true causes the previous track to play");
        CreateParameter<bool>(MediaParameter.Seeking, ParameterMode.Read, @"VRCOSC/Media/Seeking", "Seeking", "Whether the user is currently seeking");
        CreateParameter<float>(MediaParameter.Position, ParameterMode.ReadWrite, @"VRCOSC/Media/Position", "Position", "The position of the song as a percentage");
    }

    protected override string? GetChatBoxText()
    {
        if (mediaProvider.Controller is null) return null;

        if (!mediaProvider.State.IsPlaying)
        {
            if (GetSetting<MediaPausedBehaviour>(MediaSetting.PausedBehaviour) == MediaPausedBehaviour.Empty) return null;

            return GetSetting<string>(MediaSetting.PausedText)
                   .Replace(@"%title%", mediaProvider.State.Title)
                   .Replace(@"%artist%", mediaProvider.State.Artist)
                   .Replace(@"%volume%", (mediaProvider.State.Volume * 100).ToString("##0"));
        }

        var formattedText = GetSetting<string>(ChatBoxSetting.ChatBoxFormat)
                            .Replace(@"%title%", mediaProvider.State.Title)
                            .Replace(@"%artist%", mediaProvider.State.Artist)
                            .Replace(@"%curtime%", mediaProvider.State.Position?.Position.ToString(@"mm\:ss"))
                            .Replace(@"%duration%", mediaProvider.State.Position?.EndTime.ToString(@"mm\:ss"))
                            .Replace(@"%volume%", (mediaProvider.State.Volume * 100).ToString("##0"));

        return formattedText;
    }

    protected override void OnModuleStart()
    {
        base.OnModuleStart();
        mediaProvider.Hook();
        startProcesses();
    }

    private void startProcesses()
    {
        GetSetting<List<string>>(MediaSetting.StartList).ForEach(processExeLocation =>
        {
            if (File.Exists(processExeLocation))
            {
                var processName = new FileInfo(processExeLocation).Name.ToLowerInvariant().Replace(@".exe", string.Empty);
                if (!Process.GetProcessesByName(processName).Any()) Process.Start(processExeLocation);
            }
        });
    }

    protected override void OnModuleStop()
    {
        mediaProvider.UnHook();
    }

    protected override void OnAvatarChange()
    {
        sendUpdatableParameters();
        sendMediaParameters();
    }

    protected override void OnModuleUpdate()
    {
        if (mediaProvider.Controller is not null) sendUpdatableParameters();
    }

    private void onPlaybackStateUpdate()
    {
        sendMediaParameters();
    }

    private void sendMediaParameters()
    {
        SendParameter(MediaParameter.Play, mediaProvider.State.IsPlaying);
        SendParameter(MediaParameter.Shuffle, mediaProvider.State.IsShuffle);
        SendParameter(MediaParameter.Repeat, (int)mediaProvider.State.RepeatMode);
    }

    private void sendUpdatableParameters()
    {
        SendParameter(MediaParameter.Volume, mediaProvider.State.Volume);

        var position = mediaProvider.State.Position;

        if (position is not null && !currentlySeeking.Value)
        {
            var percentagePosition = position.Position.Ticks / (float)(position.EndTime.Ticks - position.StartTime.Ticks);
            SendParameter(MediaParameter.Position, percentagePosition);
        }
    }

    protected override void OnFloatParameterReceived(Enum key, float value)
    {
        switch (key)
        {
            case MediaParameter.Volume when mediaProvider.Controller is not null:
                mediaProvider.State.Volume = value;
                break;

            case MediaParameter.Position when mediaProvider.Controller is not null:

                if (!currentlySeeking.Value) return;

                var position = mediaProvider.State.Position;
                if (position is null) return;

                targetPosition = (position.EndTime - position.StartTime) * value;
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

            case MediaParameter.Next when value:
                mediaProvider.Controller?.TrySkipNextAsync();
                break;

            case MediaParameter.Previous when value:
                mediaProvider.Controller?.TrySkipPreviousAsync();
                break;

            case MediaParameter.Seeking:
                currentlySeeking.Value = value;
                if (!currentlySeeking.Value) mediaProvider.Controller?.TryChangePlaybackPositionAsync(targetPosition.Ticks);
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
        Seeking,
        Position
    }
}
