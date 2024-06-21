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
    private readonly object sessionsChangedLock = new();

    public ObservableCollection<GlobalSystemMediaTransportControlsSession> Sessions { get; } = new();
    public Dictionary<string, MediaState> States { get; } = new();

    public string? CurrentSessionId => focusedSessionId ?? sessionManager?.GetCurrentSession()?.SourceAppUserModelId;
    public MediaState CurrentState => getStateForSession(getCurrentSession());

    private string? focusedSessionId;

    public Action? OnPlaybackStateChanged;
    public Action? OnTrackChanged;
    public Action? OnPlaybackPositionChanged;

    private GlobalSystemMediaTransportControlsSession? getCurrentSession()
    {
        lock (sessionsChangedLock)
        {
            try
            {
                return Sessions.FirstOrDefault(session => session.SourceAppUserModelId == CurrentSessionId);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not retrieve current session");
                return null;
            }
        }
    }

    /// <summary>
    /// Sets a specific session ID to be the focus of this provider
    /// This is useful for when you don't want VRCOSC following Windows auto-switching sources
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
        States.Values.ForEach(state => state.Timeline.Position += delta);
    }

    public Task TerminateAsync()
    {
        if (sessionManager is null) return Task.CompletedTask;

        sessionManager.SessionsChanged -= sessionsChanged;
        sessionManager.CurrentSessionChanged -= currentSessionChanged;
        sessionManager = null;

        Sessions.Clear();
        States.Clear();
        focusedSessionId = null;

        return Task.CompletedTask;
    }

    private MediaState getStateForSession(GlobalSystemMediaTransportControlsSession? session)
    {
        if (session is null) return new MediaState();

        if (States.TryGetValue(session.SourceAppUserModelId, out var state)) return state;

        state = new MediaState
        {
            ID = session.SourceAppUserModelId
        };

        States.Add(session.SourceAppUserModelId, state);

        return state;
    }

    private void onAnyPlaybackStateChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        var state = getStateForSession(session);

        state.IsShuffle = args.IsShuffleActive ?? default;
        state.RepeatMode = args.AutoRepeatMode ?? default;
        state.Status = args.PlaybackStatus;

        OnPlaybackStateChanged?.Invoke();
    }

    private void onAnyMediaPropertyChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        var state = getStateForSession(session);

        state.Title = args.Title;
        state.Artist = args.Artist;
        state.TrackNumber = args.TrackNumber;
        state.AlbumTitle = args.AlbumTitle;
        state.AlbumArtist = args.AlbumArtist;
        state.AlbumTrackCount = args.AlbumTrackCount;

        OnTrackChanged?.Invoke();
    }

    private void onAnyTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionTimelineProperties args)
    {
        var state = getStateForSession(session);

        state.Timeline.Position = args.Position;
        state.Timeline.End = args.EndTime;
        state.Timeline.Start = args.StartTime;

        OnPlaybackPositionChanged?.Invoke();
    }

    private async void currentSessionChanged(GlobalSystemMediaTransportControlsSessionManager? sender, CurrentSessionChangedEventArgs? args)
    {
        var session = getCurrentSession();
        if (session is null) return;

        onAnyPlaybackStateChanged(session, session.GetPlaybackInfo());
        onAnyMediaPropertyChanged(session, await session.TryGetMediaPropertiesAsync());
        onAnyTimelinePropertiesChanged(session, session.GetTimelineProperties());

        OnPlaybackStateChanged?.Invoke();
        OnTrackChanged?.Invoke();
        OnPlaybackPositionChanged?.Invoke();
    }

    private void sessionsChanged(GlobalSystemMediaTransportControlsSessionManager? _, SessionsChangedEventArgs? _2)
    {
        lock (sessionsChangedLock)
        {
            var windowsSessions = sessionManager!.GetSessions();
            windowsSessions.Where(windowsSession => !Sessions.Contains(windowsSession)).ForEach(addControlSession);
            Sessions.RemoveIf(session => !windowsSessions.Contains(session));

            if (focusedSessionId is not null && Sessions.All(session => session.SourceAppUserModelId != focusedSessionId))
            {
                focusedSessionId = null;
            }
        }
    }

    private void addControlSession(GlobalSystemMediaTransportControlsSession controlSession)
    {
        controlSession.PlaybackInfoChanged += (_, _) => onAnyPlaybackStateChanged(controlSession, controlSession.GetPlaybackInfo());

        controlSession.MediaPropertiesChanged += async (_, _) =>
        {
            try
            {
                onAnyMediaPropertyChanged(controlSession, await controlSession.TryGetMediaPropertiesAsync());
            }
            catch (Exception)
            {
            }
        };

        controlSession.TimelinePropertiesChanged += (_, _) => onAnyTimelinePropertiesChanged(controlSession, controlSession.GetTimelineProperties());

        Sessions.Add(controlSession);
    }

    public async void Play()
    {
        try
        {
            await getCurrentSession()?.TryPlayAsync();
        }
        catch (Exception)
        {
        }
    }

    public async void Pause()
    {
        try
        {
            await getCurrentSession()?.TryPauseAsync();
        }
        catch (Exception)
        {
        }
    }

    public async void SkipNext()
    {
        try
        {
            await getCurrentSession()?.TrySkipNextAsync();
        }
        catch (Exception)
        {
        }
    }

    public async void SkipPrevious()
    {
        try
        {
            await getCurrentSession()?.TrySkipPreviousAsync();
        }
        catch (Exception)
        {
        }
    }

    public async void ChangeShuffle(bool active)
    {
        try
        {
            await getCurrentSession()?.TryChangeShuffleActiveAsync(active);
        }
        catch (Exception)
        {
        }
    }

    public async void ChangePlaybackPosition(TimeSpan playbackPosition)
    {
        try
        {
            await getCurrentSession()?.TryChangePlaybackPositionAsync(playbackPosition.Ticks);
        }
        catch (Exception)
        {
        }
    }

    public async void ChangeRepeatMode(MediaPlaybackAutoRepeatMode mode)
    {
        try
        {
            await getCurrentSession()?.TryChangeAutoRepeatModeAsync(mode);
        }
        catch (Exception)
        {
        }
    }

    public void TryChangeVolume(float percentage)
    {
        try
        {
            ProcessExtensions.SetProcessVolume(CurrentState.ID, percentage);
        }
        catch (Exception)
        {
        }
    }

    public float TryGetVolume()
    {
        try
        {
            return ProcessExtensions.RetrieveProcessVolume(CurrentState.ID);
        }
        catch (Exception)
        {
            return 1f;
        }
    }
}
