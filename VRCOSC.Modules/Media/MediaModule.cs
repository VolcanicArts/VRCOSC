// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.Providers.Media;

namespace VRCOSC.Modules.Media;

[ModuleTitle("Media")]
[ModuleDescription("Integration with Windows Media")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.Integrations)]
[ModulePrefab("VRCOSC-Media", "https://github.com/VolcanicArts/VRCOSC/releases/download/latest/VRCOSC-Media.unitypackage")]
public class MediaModule : ChatBoxModule
{
    private const char progress_line = '\u2501';
    private const char progress_dot = '\u25CF';
    private const char progress_start = '\u2523';
    private const char progress_end = '\u252B';
    private const int progress_resolution = 10;

    private readonly MediaProvider mediaProvider = new WindowsMediaProvider();
    private bool currentlySeeking;
    private TimeSpan targetPosition;

    public MediaModule()
    {
        mediaProvider.OnPlaybackStateChange += onPlaybackStateChange;
        mediaProvider.OnTrackChange += onTrackChange;
        mediaProvider.OnLog += Log;
    }

    protected override void CreateAttributes()
    {
        CreateSetting(MediaSetting.TruncateTitle, "Truncate Title", "Truncates the title if longer than the set value", 100);
        CreateSetting(MediaSetting.TruncateArtist, "Truncate Artist", "Truncates the artist if longer than the set value", 100);

        CreateParameter<bool>(MediaParameter.Play, ParameterMode.ReadWrite, "VRCOSC/Media/Play", "Play/Pause", "True for playing. False for paused");
        CreateParameter<float>(MediaParameter.Volume, ParameterMode.ReadWrite, "VRCOSC/Media/Volume", "Volume", "The volume of the process that is controlling the media");
        CreateParameter<int>(MediaParameter.Repeat, ParameterMode.ReadWrite, "VRCOSC/Media/Repeat", "Repeat", "0 for disabled. 1 for single. 2 for list");
        CreateParameter<bool>(MediaParameter.Shuffle, ParameterMode.ReadWrite, "VRCOSC/Media/Shuffle", "Shuffle", "True for enabled. False for disabled");
        CreateParameter<bool>(MediaParameter.Next, ParameterMode.Read, "VRCOSC/Media/Next", "Next", "Becoming true causes the next track to play");
        CreateParameter<bool>(MediaParameter.Previous, ParameterMode.Read, "VRCOSC/Media/Previous", "Previous", "Becoming true causes the previous track to play");
        CreateParameter<bool>(MediaParameter.Seeking, ParameterMode.Read, "VRCOSC/Media/Seeking", "Seeking", "Whether the user is currently seeking");
        CreateParameter<float>(MediaParameter.Position, ParameterMode.ReadWrite, "VRCOSC/Media/Position", "Position", "The position of the song as a percentage");

        CreateVariable(MediaVariable.Title, "Title", "title");
        CreateVariable(MediaVariable.Artist, "Artist", "artist");
        CreateVariable(MediaVariable.TrackNumber, "Track Number", "tracknumber");
        CreateVariable(MediaVariable.AlbumTitle, "Album Title", "albumtitle");
        CreateVariable(MediaVariable.AlbumArtist, "Album Artist", "albumartist");
        CreateVariable(MediaVariable.AlbumTrackCount, "Album Track Count", "albumtrackcount");
        CreateVariable(MediaVariable.Time, "Time", "time");
        CreateVariable(MediaVariable.TimeRemaining, "Time Remaining", "timeremaining");
        CreateVariable(MediaVariable.Duration, "Duration", "duration");
        CreateVariable(MediaVariable.ProgressVisual, "Progress Visual", "progressvisual");
        CreateVariable(MediaVariable.Volume, "Volume", "volume");

        CreateState(MediaState.Playing, "Playing", $"[{GetVariableFormat(MediaVariable.Time)}/{GetVariableFormat(MediaVariable.Duration)}]/v{GetVariableFormat(MediaVariable.Artist)} - {GetVariableFormat(MediaVariable.Title)}/v{GetVariableFormat(MediaVariable.ProgressVisual)}");
        CreateState(MediaState.Paused, "Paused", "[Paused]");

        CreateEvent(MediaEvent.NowPlaying, "Track Change", $"[Now Playing]/v{GetVariableFormat(MediaVariable.Artist)} - {GetVariableFormat(MediaVariable.Title)}", 5);
        CreateEvent(MediaEvent.Playing, "Playing", $"[Playing]/v{GetVariableFormat(MediaVariable.Artist)} - {GetVariableFormat(MediaVariable.Title)}", 5);
        CreateEvent(MediaEvent.Paused, "Paused", $"[Paused]/v{GetVariableFormat(MediaVariable.Artist)} - {GetVariableFormat(MediaVariable.Title)}", 5);
    }

    protected override void OnModuleStart()
    {
        hookIntoMedia();
    }

    private void hookIntoMedia() => Task.Run(async () =>
    {
        var result = await mediaProvider.InitialiseAsync();

        if (!result)
        {
            Log("Could not hook into Windows media");
            Log("Try restarting the modules\nIf this persists you will need to restart your PC as Windows has not initialised media correctly");
        }

        ChangeStateTo(mediaProvider.State.IsPlaying ? MediaState.Playing : MediaState.Paused);
    });

    protected override void OnModuleStop()
    {
        mediaProvider.TerminateAsync();
    }

    protected override void OnAvatarChange()
    {
        sendUpdatableParameters();
        sendMediaParameters();
    }

    [ModuleUpdate(ModuleUpdateMode.Custom, true, 1000)]
    private void sendUpdatableParameters()
    {
        SendParameter(MediaParameter.Volume, mediaProvider.TryGetVolume());

        if (!currentlySeeking)
        {
            SendParameter(MediaParameter.Position, mediaProvider.State.Timeline.PositionPercentage);
        }
    }

    [ModuleUpdate(ModuleUpdateMode.ChatBox)]
    private void updateVariables()
    {
        SetVariableValue(MediaVariable.Title, mediaProvider.State.Title.Truncate(GetSetting<int>(MediaSetting.TruncateTitle)));
        SetVariableValue(MediaVariable.Artist, mediaProvider.State.Artist.Truncate(GetSetting<int>(MediaSetting.TruncateArtist)));
        SetVariableValue(MediaVariable.TrackNumber, mediaProvider.State.TrackNumber.ToString());
        SetVariableValue(MediaVariable.AlbumTitle, mediaProvider.State.AlbumTitle);
        SetVariableValue(MediaVariable.AlbumArtist, mediaProvider.State.AlbumArtist);
        SetVariableValue(MediaVariable.AlbumTrackCount, mediaProvider.State.AlbumTrackCount.ToString());
        SetVariableValue(MediaVariable.Volume, (mediaProvider.TryGetVolume() * 100).ToString("##0"));
        SetVariableValue(MediaVariable.ProgressVisual, getProgressVisual());
        SetVariableValue(MediaVariable.Time, mediaProvider.State.Timeline.Position.Format());
        SetVariableValue(MediaVariable.TimeRemaining, (mediaProvider.State.Timeline.End - mediaProvider.State.Timeline.Position).Format());
        SetVariableValue(MediaVariable.Duration, mediaProvider.State.Timeline.End.Format());
    }

    private void onPlaybackStateChange()
    {
        updateVariables();
        sendMediaParameters();

        if (mediaProvider.State.IsPaused)
        {
            ChangeStateTo(MediaState.Paused);
            TriggerEvent(MediaEvent.Paused);
        }

        if (mediaProvider.State.IsPlaying)
        {
            ChangeStateTo(MediaState.Playing);
            TriggerEvent(MediaEvent.Playing);
        }
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

    private string getProgressVisual()
    {
        var progressPercentage = progress_resolution * mediaProvider.State.Timeline.PositionPercentage;
        var dotPosition = (int)(MathF.Floor(progressPercentage * 10f) / 10f);

        var visual = string.Empty;
        visual += progress_start;

        for (var i = 0; i < progress_resolution; i++)
        {
            visual += i == dotPosition ? progress_dot : progress_line;
        }

        visual += progress_end;

        return visual;
    }

    protected override void OnRegisteredParameterReceived(AvatarParameter parameter)
    {
        switch (parameter.Lookup)
        {
            case MediaParameter.Volume:
                mediaProvider.TryChangeVolume(parameter.ValueAs<float>());
                break;

            case MediaParameter.Position:
                if (!currentlySeeking) return;

                var position = mediaProvider.State.Timeline;
                targetPosition = (position.End - position.Start) * parameter.ValueAs<float>();
                break;

            case MediaParameter.Repeat:
                mediaProvider.ChangeRepeatMode((MediaRepeatMode)parameter.ValueAs<int>());
                break;

            case MediaParameter.Play:
                if (parameter.ValueAs<bool>())
                    mediaProvider.Play();
                else
                    mediaProvider.Pause();
                break;

            case MediaParameter.Shuffle:
                mediaProvider.ChangeShuffle(parameter.ValueAs<bool>());
                break;

            case MediaParameter.Next when parameter.ValueAs<bool>():
                mediaProvider.SkipNext();
                break;

            case MediaParameter.Previous when parameter.ValueAs<bool>():
                mediaProvider.SkipPrevious();
                break;

            case MediaParameter.Seeking:
                currentlySeeking = parameter.ValueAs<bool>();
                if (!currentlySeeking) mediaProvider.ChangePlaybackPosition(targetPosition);
                break;
        }
    }

    private enum MediaState
    {
        Playing,
        Paused
    }

    private enum MediaEvent
    {
        NowPlaying,
        Playing,
        Paused
    }

    private enum MediaVariable
    {
        Title,
        Artist,
        Time,
        TimeRemaining,
        Duration,
        Volume,
        TrackNumber,
        AlbumTitle,
        AlbumArtist,
        AlbumTrackCount,
        ProgressVisual
    }

    private enum MediaSetting
    {
        TruncateTitle,
        TruncateArtist
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
