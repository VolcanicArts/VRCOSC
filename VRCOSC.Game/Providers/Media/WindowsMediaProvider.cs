// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Control;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Processes;

namespace VRCOSC.Game.Providers.Media;

public class WindowsMediaProvider : MediaProvider
{
    private readonly List<GlobalSystemMediaTransportControlsSession> sessions = new();
    private GlobalSystemMediaTransportControlsSessionManager? sessionManager;
    private GlobalSystemMediaTransportControlsSession? controller => sessionManager?.GetCurrentSession();

    public override async Task<bool> InitialiseAsync()
    {
        State = new MediaState();
        sessionManager ??= await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        if (sessionManager is null) return false;

        sessionManager.CurrentSessionChanged += onCurrentSessionChanged;
        sessionManager.SessionsChanged += sessionsChanged;

        sessionsChanged(null, null);
        onCurrentSessionChanged(null, null);

        return true;
    }

    public override void Update(TimeSpan delta)
    {
        State.Timeline.Position += delta;
    }

    public override Task TerminateAsync()
    {
        if (sessionManager is null) return Task.CompletedTask;

        sessionManager.CurrentSessionChanged -= onCurrentSessionChanged;
        sessionManager.SessionsChanged -= sessionsChanged;
        sessions.Clear();

        return Task.CompletedTask;
    }

    private bool isFocusedSession(GlobalSystemMediaTransportControlsSession session) => session.SourceAppUserModelId == controller?.SourceAppUserModelId;

    private void onAnyPlaybackStateChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        if (!isFocusedSession(session)) return;

        State.IsShuffle = args.IsShuffleActive ?? default;
        State.RepeatMode = convertWindowsRepeatMode(args.AutoRepeatMode);
        State.Status = convertWindowsPlaybackStatus(args.PlaybackStatus);

        OnPlaybackStateChange?.Invoke();
    }

    private static MediaRepeatMode convertWindowsRepeatMode(MediaPlaybackAutoRepeatMode? mode) => mode switch
    {
        MediaPlaybackAutoRepeatMode.None => MediaRepeatMode.Off,
        MediaPlaybackAutoRepeatMode.Track => MediaRepeatMode.Single,
        MediaPlaybackAutoRepeatMode.List => MediaRepeatMode.Multiple,
        _ => MediaRepeatMode.Off
    };

    private static MediaPlaybackStatus convertWindowsPlaybackStatus(GlobalSystemMediaTransportControlsSessionPlaybackStatus status) => status switch
    {
        GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped => MediaPlaybackStatus.Stopped,
        GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing => MediaPlaybackStatus.Playing,
        GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused => MediaPlaybackStatus.Paused,
        _ => MediaPlaybackStatus.Stopped
    };

    private void onAnyMediaPropertyChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        if (!isFocusedSession(session)) return;

        State.Title = args.Title;
        State.Artist = args.Artist;
        State.TrackNumber = args.TrackNumber;
        State.AlbumTitle = args.AlbumTitle;
        State.AlbumArtist = args.AlbumArtist;
        State.AlbumTrackCount = args.AlbumTrackCount;

        OnTrackChange?.Invoke();
    }

    private void onAnyTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionTimelineProperties args)
    {
        if (!isFocusedSession(session)) return;

        State.Timeline.Position = args.Position;
        State.Timeline.End = args.EndTime;
        State.Timeline.Start = args.StartTime;

        OnPlaybackPositionChange?.Invoke();
    }

    private async void onCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager? _, CurrentSessionChangedEventArgs? _2)
    {
        if (controller is null)
        {
            State = new MediaState();
            return;
        }

        State.Identifier = controller.SourceAppUserModelId;

        onAnyPlaybackStateChanged(controller, controller.GetPlaybackInfo());

        try { onAnyMediaPropertyChanged(controller, await controller.TryGetMediaPropertiesAsync()); }
        catch (COMException) { }

        onAnyTimelinePropertiesChanged(controller, controller.GetTimelineProperties());
    }

    private void sessionsChanged(GlobalSystemMediaTransportControlsSessionManager? _, SessionsChangedEventArgs? _2)
    {
        var windowsSessions = sessionManager!.GetSessions();
        windowsSessions.Where(windowsSession => !sessions.Contains(windowsSession)).ForEach(addControlSession);
        sessions.RemoveAll(session => !windowsSessions.Contains(session));
    }

    private void addControlSession(GlobalSystemMediaTransportControlsSession controlSession)
    {
        controlSession.PlaybackInfoChanged += (_, _) => onAnyPlaybackStateChanged(controlSession, controlSession.GetPlaybackInfo());

        controlSession.MediaPropertiesChanged += async (_, _) =>
        {
            try { onAnyMediaPropertyChanged(controlSession, await controlSession.TryGetMediaPropertiesAsync()); }
            catch (COMException) { }
        };

        controlSession.TimelinePropertiesChanged += (_, _) => onAnyTimelinePropertiesChanged(controlSession, controlSession.GetTimelineProperties());

        sessions.Add(controlSession);
    }

    public override async void Play()
    {
        try
        {
            await controller?.TryPlayAsync();
        }
        catch (Exception)
        {
            OnLog?.Invoke("'Play' failed to execute");
        }
    }

    public override async void Pause()
    {
        try
        {
            await controller?.TryPauseAsync();
        }
        catch (Exception)
        {
            OnLog?.Invoke("'Pause' failed to execute");
        }
    }

    public override async void SkipNext()
    {
        try
        {
            await controller?.TrySkipNextAsync();
        }
        catch (Exception)
        {
            OnLog?.Invoke("'Next Track' failed to execute");
        }
    }

    public override async void SkipPrevious()
    {
        try
        {
            await controller?.TrySkipPreviousAsync();
        }
        catch (Exception)
        {
            OnLog?.Invoke("'Previous Track' failed to execute");
        }
    }

    public override async void ChangeShuffle(bool active)
    {
        try
        {
            await controller?.TryChangeShuffleActiveAsync(active);
        }
        catch (Exception)
        {
            OnLog?.Invoke("'Change Shuffle' failed to execute");
        }
    }

    public override async void ChangePlaybackPosition(TimeSpan playbackPosition)
    {
        try
        {
            await controller?.TryChangePlaybackPositionAsync(playbackPosition.Ticks);
        }
        catch (Exception)
        {
            OnLog?.Invoke("'Change Position' failed to execute");
        }
    }

    public override async void ChangeRepeatMode(MediaRepeatMode mode)
    {
        try
        {
            await controller?.TryChangeAutoRepeatModeAsync((MediaPlaybackAutoRepeatMode)(int)mode);
        }
        catch (Exception)
        {
            OnLog?.Invoke("'Change Repeat' failed to execute");
        }
    }

    public override void TryChangeVolume(float percentage)
    {
        try
        {
            ProcessExtensions.SetProcessVolume(State.Identifier, percentage);
        }
        catch (Exception)
        {
            OnLog?.Invoke("'Change Volume' failed to execute");
        }
    }

    public override float TryGetVolume()
    {
        try
        {
            return ProcessExtensions.RetrieveProcessVolume(State.Identifier);
        }
        catch (Exception)
        {
            OnLog?.Invoke($"Could not retrieve volume for process: {State.Identifier}");
            return 1f;
        }
    }
}
