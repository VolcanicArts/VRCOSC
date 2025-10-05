// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
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
    public Dictionary<string, MediaState> States { get; } = new();

    public MediaState CurrentState => CurrentSession is not null ? States.TryGetValue(CurrentSession.SourceAppUserModelId, out var session) ? session : new MediaState() : new MediaState();
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
        if (session is null) return;

        var state = States[session.SourceAppUserModelId];
        var args = session.GetPlaybackInfo();

        try
        {
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
        if (session is null) return;

        var state = States[session.SourceAppUserModelId];

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
        if (session is null) return;

        try
        {
            var state = States[session.SourceAppUserModelId];

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

    private async void onAnyMediaPropertyChanged(GlobalSystemMediaTransportControlsSession? session, MediaPropertiesChangedEventArgs? _)
    {
        Logger.Log($"Media property change: {session?.SourceAppUserModelId}", LoggingTarget.Information);

        await updateMediaProperties(session);
        updateTimeline(session);
        OnTrackChanged?.Invoke();
        OnPlaybackPositionChanged?.Invoke();
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

    private async void currentSessionChanged(GlobalSystemMediaTransportControlsSessionManager? sender, CurrentSessionChangedEventArgs? args)
    {
        await updateCurrentSession();
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
        States.RemoveIf(pair => !Sessions.Contains(pair.Key));

        if (focusedSessionId is not null && !Sessions.Contains(focusedSessionId))
        {
            SetFocusedSession(null);
        }

        if (currentSessionId is not null && !Sessions.Contains(currentSessionId))
        {
            currentSessionId = null;
        }
    }

    public async void Play()
    {
        try
        {
            await CurrentSession?.TryPlayAsync();
        }
        catch (Exception)
        {
        }
    }

    public async void Pause()
    {
        try
        {
            await CurrentSession?.TryPauseAsync();
        }
        catch (Exception)
        {
        }
    }

    public async void SkipNext()
    {
        try
        {
            await CurrentSession?.TrySkipNextAsync();
        }
        catch (Exception)
        {
        }
    }

    public async void SkipPrevious()
    {
        try
        {
            await CurrentSession?.TrySkipPreviousAsync();
        }
        catch (Exception)
        {
        }
    }

    public async void ChangeShuffle(bool active)
    {
        try
        {
            await CurrentSession?.TryChangeShuffleActiveAsync(active);
        }
        catch (Exception)
        {
        }
    }

    public async void ChangePlaybackPosition(TimeSpan playbackPosition)
    {
        try
        {
            await CurrentSession?.TryChangePlaybackPositionAsync(playbackPosition.Ticks);
        }
        catch (Exception)
        {
        }
    }

    public async void ChangeRepeatMode(MediaPlaybackAutoRepeatMode mode)
    {
        try
        {
            await CurrentSession?.TryChangeAutoRepeatModeAsync(mode);
        }
        catch (Exception)
        {
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