// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Control;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Providers.Media;

public class WindowsMediaProvider
{
    private readonly ConcurrentBag<GlobalSystemMediaTransportControlsSession> cachedSessions = [];

    public readonly ConcurrentDictionary<string, MediaState> SessionStates = [];

    private GlobalSystemMediaTransportControlsSessionManager? sessionManager;
    private string? focusedSessionId;
    private string? currentSessionId;

    public Action? OnSessionsChanged;
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

        SessionStates.Clear();
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
        foreach (var state in SessionStates.Values)
        {
            if (!state.IsPlaying) continue;

            state.Timeline.Position += delta;
        }
    }

    public void Terminate()
    {
        if (sessionManager is null) return;

        foreach (var session in sessionManager.GetSessions().Where(session => SessionStates.ContainsKey(session.SourceAppUserModelId)))
        {
            session.MediaPropertiesChanged -= onAnyMediaPropertyChanged;
            session.PlaybackInfoChanged -= onAnyPlaybackStateChanged;
            session.TimelinePropertiesChanged -= onAnyTimelinePropertiesChanged;
        }

        sessionManager.SessionsChanged -= sessionsChanged;
        sessionManager.CurrentSessionChanged -= currentSessionChanged;
        sessionManager = null;
    }

    public MediaState GetCurrentState()
    {
        var currentSession = getCurrentSession();
        if (currentSession is null || !SessionStates.TryGetValue(currentSession.SourceAppUserModelId, out var state)) return new MediaState();

        return state;
    }

    private GlobalSystemMediaTransportControlsSession? getCurrentSession()
    {
        var localFocusedSessionId = focusedSessionId ?? currentSessionId;
        return cachedSessions.FirstOrDefault(session => session.SourceAppUserModelId == localFocusedSessionId);
    }

    private void updatePlaybackInfo(GlobalSystemMediaTransportControlsSession? session)
    {
        if (session is null || !SessionStates.TryGetValue(session.SourceAppUserModelId, out var state)) return;

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
        if (session is null || !SessionStates.TryGetValue(session.SourceAppUserModelId, out var state)) return;

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
        if (session is null || !SessionStates.TryGetValue(session.SourceAppUserModelId, out var state)) return;

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

    private async void sessionsChanged(GlobalSystemMediaTransportControlsSessionManager? _, SessionsChangedEventArgs? _2)
    {
        try
        {
            var localSessions = sessionManager?.GetSessions() ?? [];

            cachedSessions.Clear();

            foreach (var session in localSessions)
            {
                cachedSessions.Add(session);
            }

            foreach (var session in cachedSessions.Where(session => !SessionStates.ContainsKey(session.SourceAppUserModelId)))
            {
                session.PlaybackInfoChanged += onAnyPlaybackStateChanged;
                session.MediaPropertiesChanged += onAnyMediaPropertyChanged;
                session.TimelinePropertiesChanged += onAnyTimelinePropertiesChanged;

                SessionStates.TryAdd(session.SourceAppUserModelId, new MediaState
                {
                    ID = session.SourceAppUserModelId
                });

                updatePlaybackInfo(session);
                await updateMediaProperties(session);
                updateTimeline(session);

                Logger.Log($"Adding new state for {session.SourceAppUserModelId}", LoggingTarget.Information);
            }

            SessionStates.RemoveIf(pair => !cachedSessions.Select(cachedSession => cachedSession.SourceAppUserModelId).Contains(pair.Key));

            if (focusedSessionId is not null && !SessionStates.ContainsKey(focusedSessionId))
                SetFocusedSession(null);

            currentSessionId ??= sessionManager?.GetCurrentSession()?.SourceAppUserModelId;

            if (currentSessionId is not null && !SessionStates.ContainsKey(currentSessionId))
                currentSessionId = null;
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(WindowsMediaProvider)} has experienced an error on session change");
        }

        OnSessionsChanged?.Invoke();
    }

    public void Play()
    {
        run().Forget();
        return;

        async Task run()
        {
            try
            {
                await getCurrentSession()?.TryPlayAsync();
            }
            catch
            {
            }
        }
    }

    public void Pause()
    {
        run().Forget();
        return;

        async Task run()
        {
            try
            {
                await getCurrentSession()?.TryPauseAsync();
            }
            catch
            {
            }
        }
    }

    public void SkipNext()
    {
        run().Forget();
        return;

        async Task run()
        {
            try
            {
                await getCurrentSession()?.TrySkipNextAsync();
            }
            catch
            {
            }
        }
    }

    public void SkipPrevious()
    {
        run().Forget();
        return;

        async Task run()
        {
            try
            {
                await getCurrentSession()?.TrySkipPreviousAsync();
            }
            catch
            {
            }
        }
    }

    public void ChangeShuffle(bool active)
    {
        run().Forget();
        return;

        async Task run()
        {
            try
            {
                await getCurrentSession()?.TryChangeShuffleActiveAsync(active);
            }
            catch
            {
            }
        }
    }

    public void ChangePlaybackPosition(TimeSpan playbackPosition)
    {
        run().Forget();
        return;

        async Task run()
        {
            try
            {
                await getCurrentSession()?.TryChangePlaybackPositionAsync(playbackPosition.Ticks);
            }
            catch
            {
            }
        }
    }

    public void ChangeRepeatMode(MediaPlaybackAutoRepeatMode mode)
    {
        run().Forget();
        return;

        async Task run()
        {
            try
            {
                await getCurrentSession()?.TryChangeAutoRepeatModeAsync(mode);
            }
            catch
            {
            }
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