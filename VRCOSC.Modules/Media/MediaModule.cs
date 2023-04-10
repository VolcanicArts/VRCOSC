// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using Windows.Media;
using osu.Framework.Bindables;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.ChatBox;
using VRCOSC.Game.Providers.Media;

namespace VRCOSC.Modules.Media;

public class MediaModule : ChatBoxModule
{
    public override string Title => "Media";
    public override string Description => "Integration with Windows Media";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Media";
    protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(1);
    public override ModuleType Type => ModuleType.Integrations;

    private readonly WindowsMediaProvider mediaProvider = new();
    private readonly Bindable<bool> currentlySeeking = new();
    private TimeSpan targetPosition;

    public MediaModule()
    {
        mediaProvider.OnPlaybackStateUpdate += onPlaybackStateUpdate;
        mediaProvider.OnTrackChange += onTrackChange;
    }

    protected override void CreateAttributes()
    {
        // TODO - Remove into a separate screen that allows for start options
        CreateSetting(MediaSetting.StartList, "Start List", "A list of exe locations to start with this module. This is handy for starting media apps on module start. For example, Spotify", new[] { @$"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe" }, true);

        CreateParameter<bool>(MediaParameter.Play, ParameterMode.ReadWrite, @"VRCOSC/Media/Play", "Play/Pause", @"True for playing. False for paused");
        CreateParameter<float>(MediaParameter.Volume, ParameterMode.ReadWrite, @"VRCOSC/Media/Volume", "Volume", @"The volume of the process that is controlling the media");
        CreateParameter<int>(MediaParameter.Repeat, ParameterMode.ReadWrite, @"VRCOSC/Media/Repeat", "Repeat", @"0 for disabled. 1 for single. 2 for list");
        CreateParameter<bool>(MediaParameter.Shuffle, ParameterMode.ReadWrite, @"VRCOSC/Media/Shuffle", "Shuffle", @"True for enabled. False for disabled");
        CreateParameter<bool>(MediaParameter.Next, ParameterMode.Read, @"VRCOSC/Media/Next", "Next", @"Becoming true causes the next track to play");
        CreateParameter<bool>(MediaParameter.Previous, ParameterMode.Read, @"VRCOSC/Media/Previous", "Previous", @"Becoming true causes the previous track to play");
        CreateParameter<bool>(MediaParameter.Seeking, ParameterMode.Read, @"VRCOSC/Media/Seeking", "Seeking", "Whether the user is currently seeking");
        CreateParameter<float>(MediaParameter.Position, ParameterMode.ReadWrite, @"VRCOSC/Media/Position", "Position", "The position of the song as a percentage");

        CreateState(MediaState.Playing, "Playing", @"[{time}/{duration}]                            Now Playing: {artist} - {title}");
        CreateState(MediaState.Paused, "Paused", @"[Paused]");

        CreateEvent(MediaEvent.NowPlaying, "Now Playing", @"[Now Playing]                            {artist} - {title}", 5);

        CreateVariable(MediaVariable.Title, @"Title", @"{title}");
        CreateVariable(MediaVariable.Artist, @"Artist", @"{artist}");
        CreateVariable(MediaVariable.Time, @"Time", @"{time}");
        CreateVariable(MediaVariable.Duration, @"Duration", @"{duration}");
        CreateVariable(MediaVariable.Volume, @"Volume", @"{volume}");
    }

    protected override async void OnModuleStart()
    {
        var result = await mediaProvider.Hook();

        if (!result)
        {
            Log("Could not hook into Windows media");
            Log("Try restarting the modules\nIf this persists you will need to restart your PC as Windows has not initialised media correctly");
        }

        ChangeStateTo(mediaProvider.State.IsPlaying ? MediaState.Playing : MediaState.Paused);

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

        updateVariables();
    }

    private void updateVariables()
    {
        SetVariableValue(MediaVariable.Title, mediaProvider.State.Title);
        SetVariableValue(MediaVariable.Artist, mediaProvider.State.Artist);
        SetVariableValue(MediaVariable.Time, mediaProvider.State.Position?.Position.ToString(@"mm\:ss"));
        SetVariableValue(MediaVariable.Duration, mediaProvider.State.Position?.EndTime.ToString(@"mm\:ss"));
        SetVariableValue(MediaVariable.Volume, (mediaProvider.State.Volume * 100).ToString("##0"));
    }

    private void onPlaybackStateUpdate()
    {
        sendMediaParameters();
        ChangeStateTo(mediaProvider.State.IsPlaying ? MediaState.Playing : MediaState.Paused);
    }

    private void onTrackChange()
    {
        updateVariables();
        TriggerEvent(MediaEvent.NowPlaying);
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
        StartList
    }

    private enum MediaState
    {
        Playing,
        Paused
    }

    private enum MediaEvent
    {
        NowPlaying
    }

    private enum MediaVariable
    {
        Title,
        Artist,
        Time,
        Duration,
        Volume
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
