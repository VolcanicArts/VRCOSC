// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Control;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Providers.Media;

public class WindowsMediaProvider
{
    private GlobalSystemMediaTransportControlsSessionManager? sessionManager;
    private List<GlobalSystemMediaTransportControlsSession> cachedSessions = new();

    public ObservableCollection<string> Sessions { get; } = new();
    public ConcurrentDictionary<string, MediaState> States { get; } = new();

    public MediaState CurrentState
    {
        get
        {
            if (CurrentSession is null) return new MediaState();
            if (States.TryGetValue(CurrentSession.SourceAppUserModelId, out var state)) return state;

            return new MediaState();
        }
    }

    public GlobalSystemMediaTransportControlsSession? CurrentSession => cachedSessions.FirstOrDefault(session => session.SourceAppUserModelId == (focusedSessionId ?? currentSessionId));

    private string? focusedSessionId;
    private string? currentSessionId;

    public Action? OnPlaybackStateChanged;
    public Action? OnTrackChanged;
    public Action? OnPlaybackPositionChanged;

    /// <summary>
    /// Sets a specific session ID to be the focus of this provider
    /// This is useful for when you don't want this provider following Windows auto-switching sources
    /// </summary>
    /// <remarks>Set to null to give switch control back to Windows</remarks>
    public void SetFocusedSession(string? sessionId)
    {
        focusedSessionId = sessionId;
        sessionsChanged(null, null);
        currentSessionChanged(null, null);
    }

    public async Task<bool> InitialiseAsync()
    {
        if (sessionManager is not null) throw new InvalidOperationException("Cannot initialise without terminating existing instance");

        Sessions.Clear();
        States.Clear();
        cachedSessions.Clear();
        focusedSessionId = null;

        sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        if (sessionManager is null)
        {
            Logger.Log($"{nameof(WindowsMediaProvider)} could not get the {nameof(GlobalSystemMediaTransportControlsSessionManager)}");
            return false;
        }

        sessionManager.SessionsChanged += sessionsChanged;
        sessionManager.CurrentSessionChanged += currentSessionChanged;

        sessionsChanged(null, null);
        currentSessionChanged(null, null);

        return true;
    }

    public void Update(TimeSpan delta)
    {
        foreach (var state in States.Values)
        {
            if (!state.IsPlaying) continue;

            state.Timeline.Position += delta;
        }
    }

    public void Terminate()
    {
        if (sessionManager is null) return;

        foreach (var session in sessionManager.GetSessions().Where(session => Sessions.Contains(session.SourceAppUserModelId)))
        {
            session.MediaPropertiesChanged -= onAnyMediaPropertyChanged;
            session.PlaybackInfoChanged -= onAnyPlaybackStateChanged;
            session.TimelinePropertiesChanged -= onAnyTimelinePropertiesChanged;
        }

        sessionManager.SessionsChanged -= sessionsChanged;
        sessionManager.CurrentSessionChanged -= currentSessionChanged;
        sessionManager = null;
    }

    private void updatePlaybackInfo(GlobalSystemMediaTransportControlsSession? session)
    {
        if (session is null || !States.TryGetValue(session.SourceAppUserModelId, out var state)) return;

        try
        {
            var args = session.GetPlaybackInfo();
            state.IsShuffle = args.IsShuffleActive ?? false;
            state.RepeatMode = args.AutoRepeatMode ?? default;
            state.Status = args.PlaybackStatus;
        }
        catch
        {
        }
    }

    private async Task updateMediaProperties(GlobalSystemMediaTransportControlsSession? session)
    {
        if (session is null || !States.TryGetValue(session.SourceAppUserModelId, out var state)) return;

        try
        {
            var args = await session.TryGetMediaPropertiesAsync();
            state.Title = args.Title;
            state.Subtitle = args.Subtitle;
            state.Artist = args.Artist;
            state.TrackNumber = args.TrackNumber;
            state.AlbumTitle = args.AlbumTitle;
            state.AlbumArtist = args.AlbumArtist;
            state.AlbumTrackCount = args.AlbumTrackCount;
            state.Genres = args.Genres.ToList();
        }
        catch
        {
        }
    }

    private void updateTimeline(GlobalSystemMediaTransportControlsSession? session)
    {
        if (session is null || !States.TryGetValue(session.SourceAppUserModelId, out var state)) return;

        try
        {
            var args = session.GetTimelineProperties();
            state.Timeline.Position = args.Position;
            state.Timeline.End = args.EndTime;
            state.Timeline.Start = args.StartTime;
        }
        catch
        {
        }
    }

    private void onAnyPlaybackStateChanged(GlobalSystemMediaTransportControlsSession? session, PlaybackInfoChangedEventArgs? _)
    {
        Logger.Log($"Playback state change: {session?.SourceAppUserModelId}", LoggingTarget.Information);

        updatePlaybackInfo(session);
        OnPlaybackStateChanged?.Invoke();
    }

    private void onAnyMediaPropertyChanged(GlobalSystemMediaTransportControlsSession? session, MediaPropertiesChangedEventArgs? _)
    {
        run().Forget();
        return;

        async Task run()
        {
            Logger.Log($"Media property change: {session?.SourceAppUserModelId}", LoggingTarget.Information);

            await updateMediaProperties(session);
            updateTimeline(session);
            OnTrackChanged?.Invoke();
            OnPlaybackPositionChanged?.Invoke();
        }
    }

    private void onAnyTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession? session, TimelinePropertiesChangedEventArgs? _)
    {
        Logger.Log($"Timeline property change: {session?.SourceAppUserModelId}", LoggingTarget.Information);

        updateTimeline(session);
        OnPlaybackPositionChanged?.Invoke();
    }

    private async Task updateCurrentSession()
    {
        var session = sessionManager?.GetCurrentSession();
        Logger.Log($"Session changed: {session?.SourceAppUserModelId}", LoggingTarget.Information);

        currentSessionId = session?.SourceAppUserModelId;
        await updateMediaProperties(session);
        updateTimeline(session);
        updatePlaybackInfo(session);
        OnTrackChanged?.Invoke();
        OnPlaybackPositionChanged?.Invoke();
        OnPlaybackStateChanged?.Invoke();
    }

    private void currentSessionChanged(GlobalSystemMediaTransportControlsSessionManager? sender, CurrentSessionChangedEventArgs? args)
    {
        updateCurrentSession().Forget();
    }

    private void sessionsChanged(GlobalSystemMediaTransportControlsSessionManager? _, SessionsChangedEventArgs? _2)
    {
        cachedSessions = sessionManager?.GetSessions().ToList() ?? new List<GlobalSystemMediaTransportControlsSession>();

        foreach (var session in cachedSessions.Where(session => !Sessions.Contains(session.SourceAppUserModelId)))
        {
            session.PlaybackInfoChanged += onAnyPlaybackStateChanged;
            session.MediaPropertiesChanged += onAnyMediaPropertyChanged;
            session.TimelinePropertiesChanged += onAnyTimelinePropertiesChanged;

            Sessions.Add(session.SourceAppUserModelId);

            Logger.Log($"Adding new state for {session.SourceAppUserModelId}", LoggingTarget.Information);

            States[session.SourceAppUserModelId] = new MediaState
            {
                ID = session.SourceAppUserModelId
            };
        }

        Sessions.RemoveIf(session => !cachedSessions.Select(cachedSession => cachedSession.SourceAppUserModelId).Contains(session));

        var statesToRemove = States.Where(pair => !Sessions.Contains(pair.Key));

        foreach (var pair in statesToRemove)
        {
            States.TryRemove(pair.Key, out var _);
        }

        if (focusedSessionId is not null && !Sessions.Contains(focusedSessionId))
        {
            SetFocusedSession(null);
        }

        if (currentSessionId is not null && !Sessions.Contains(currentSessionId))
        {
            currentSessionId = null;
        }
    }

    public void Play()
    {
        run().Forget();
        return;

        async Task run()
        {
            await CurrentSession?.TryPlayAsync();
        }
    }

    public void Pause()
    {
        run().Forget();
        return;

        async Task run()
        {
            await CurrentSession?.TryPauseAsync();
        }
    }

    public void SkipNext()
    {
        run().Forget();
        return;

        async Task run()
        {
            await CurrentSession?.TrySkipNextAsync();
        }
    }

    public void SkipPrevious()
    {
        run().Forget();
        return;

        async Task run()
        {
            await CurrentSession?.TrySkipPreviousAsync();
        }
    }

    public void ChangeShuffle(bool active)
    {
        run().Forget();
        return;

        async Task run()
        {
            await CurrentSession?.TryChangeShuffleActiveAsync(active);
        }
    }

    public void ChangePlaybackPosition(TimeSpan playbackPosition)
    {
        run().Forget();
        return;

        async Task run()
        {
            await CurrentSession?.TryChangePlaybackPositionAsync(playbackPosition.Ticks);
        }
    }

    public void ChangeRepeatMode(MediaPlaybackAutoRepeatMode mode)
    {
        run().Forget();
        return;

        async Task run()
        {
            await CurrentSession?.TryChangeAutoRepeatModeAsync(mode);
        }
    }

    public void TryChangeVolume(float percentage)
    {
        try
        {
            ProcessExtensions.SetProcessVolume(currentSessionId, percentage);
        }
        catch (Exception)
        {
        }
    }

    public float TryGetVolume()
    {
        try
        {
            return ProcessExtensions.RetrieveProcessVolume(currentSessionId);
        }
        catch (Exception)
        {
            return 1f;
        }
    }
}