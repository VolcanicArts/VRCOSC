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

    public MediaState CurrentState => CurrentSession is not null ? States[CurrentSession.SourceAppUserModelId] : new MediaState();
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
        if (sessionManager is not null)
        {
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
    }

    private void onAnyPlaybackStateChanged(GlobalSystemMediaTransportControlsSession? session, PlaybackInfoChangedEventArgs? _)
    {
        try
        {
            if (session is not null)
            {
                var state = States[session.SourceAppUserModelId];
                var args = session.GetPlaybackInfo();

                state.IsShuffle = args.IsShuffleActive ?? default;
                state.RepeatMode = args.AutoRepeatMode ?? default;
                state.Status = args.PlaybackStatus;
            }
        }
        catch (Exception)
        {
        }

        OnPlaybackStateChanged?.Invoke();
    }

    private async void onAnyMediaPropertyChanged(GlobalSystemMediaTransportControlsSession? session, MediaPropertiesChangedEventArgs? _)
    {
        try
        {
            if (session is not null)
            {
                var state = States[session.SourceAppUserModelId];
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
        }
        catch (Exception)
        {
        }

        OnTrackChanged?.Invoke();
    }

    private void onAnyTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession? session, TimelinePropertiesChangedEventArgs? _)
    {
        try
        {
            if (session is not null)
            {
                var state = States[session.SourceAppUserModelId];
                var args = session.GetTimelineProperties();

                state.Timeline.Position = args.Position;
                state.Timeline.End = args.EndTime;
                state.Timeline.Start = args.StartTime;
            }
        }
        catch (Exception)
        {
        }

        OnPlaybackPositionChanged?.Invoke();
    }

    private void currentSessionChanged(GlobalSystemMediaTransportControlsSessionManager? sender, CurrentSessionChangedEventArgs? args)
    {
        var session = sessionManager?.GetCurrentSession();

        currentSessionId = session?.SourceAppUserModelId;
        onAnyPlaybackStateChanged(session, null);
        onAnyMediaPropertyChanged(session, null);
        onAnyTimelinePropertiesChanged(session, null);
    }

    private void sessionsChanged(GlobalSystemMediaTransportControlsSessionManager? _, SessionsChangedEventArgs? _2)
    {
        cachedSessions = sessionManager?.GetSessions().ToList() ?? new List<GlobalSystemMediaTransportControlsSession>();

        cachedSessions.Where(session => !Sessions.Contains(session.SourceAppUserModelId)).ForEach(session =>
        {
            session.PlaybackInfoChanged += onAnyPlaybackStateChanged;
            session.MediaPropertiesChanged += onAnyMediaPropertyChanged;
            session.TimelinePropertiesChanged += onAnyTimelinePropertiesChanged;

            Sessions.Add(session.SourceAppUserModelId);

            States.Add(session.SourceAppUserModelId, new MediaState
            {
                ID = session.SourceAppUserModelId
            });
        });

        Sessions.RemoveIf(session => !cachedSessions.Select(cachedSession => cachedSession.SourceAppUserModelId).Contains(session));

        if (focusedSessionId is not null && !Sessions.Contains(focusedSessionId))
        {
            SetFocusedSession(null);
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
