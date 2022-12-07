// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Control;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.Modules;

// Taken from https://github.com/DubyaDude/WindowsMediaController
// Applied .NET6 specific modifications

public class MediaManager : IDisposable
{
    public delegate void SessionChangeDelegate(MediaSession mediaSession);

    public delegate void PlaybackChangeDelegate(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo);

    public delegate void SongChangeDelegate(MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties);

    public event SessionChangeDelegate? OnAnySessionOpened;
    public event SessionChangeDelegate? OnAnySessionClosed;
    public event PlaybackChangeDelegate? OnAnyPlaybackStateChanged;
    public event SongChangeDelegate? OnAnyMediaPropertyChanged;

    public readonly Dictionary<string, MediaSession> CurrentMediaSessions = new();

    private bool isStarted { get; set; }

    private GlobalSystemMediaTransportControlsSessionManager? windowsSessionManager { get; set; }

    public async Task Start()
    {
        if (!isStarted)
        {
            windowsSessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            sessionsChanged(windowsSessionManager);
            windowsSessionManager.SessionsChanged += sessionsChanged;
            isStarted = true;
        }
        else
        {
            throw new InvalidOperationException("MediaManager already started");
        }
    }

    private void sessionsChanged(GlobalSystemMediaTransportControlsSessionManager winSessionManager, SessionsChangedEventArgs? args = null)
    {
        var controlSessionList = winSessionManager.GetSessions();

        foreach (var controlSession in controlSessionList)
        {
            if (!CurrentMediaSessions.ContainsKey(controlSession.SourceAppUserModelId))
            {
                var mediaSession = new MediaSession(controlSession, this);
                CurrentMediaSessions[controlSession.SourceAppUserModelId] = mediaSession;

                try { OnAnySessionOpened?.Invoke(mediaSession); }
                catch { }

                mediaSession.OnSongChange(controlSession);
            }
        }

        //Checking if a source fell off the session list without doing a proper Closed event (I.E. Spotify)
        var controlSessionIds = controlSessionList.Select(x => x.SourceAppUserModelId);
        (from session in CurrentMediaSessions where !controlSessionIds.Contains(session.Key) select session.Value).ForEach(x => x.Dispose());
    }

    private bool removeSource(MediaSession mediaSession)
    {
        if (CurrentMediaSessions.ContainsKey(mediaSession.Id))
        {
            CurrentMediaSessions.Remove(mediaSession.Id);

            try { OnAnySessionClosed?.Invoke(mediaSession); }
            catch { }

            return true;
        }

        return false;
    }

    public void Dispose()
    {
        OnAnySessionOpened = null;
        OnAnySessionClosed = null;
        OnAnyMediaPropertyChanged = null;
        OnAnyPlaybackStateChanged = null;

        var keys = CurrentMediaSessions.Keys.ToList();

        foreach (var key in keys)
        {
            CurrentMediaSessions[key].Dispose();
        }

        CurrentMediaSessions.Clear();

        isStarted = false;
        windowsSessionManager = null;

        GC.SuppressFinalize(this);
    }

    public class MediaSession
    {
        public event SessionChangeDelegate? OnSessionClosed;
        public event PlaybackChangeDelegate? OnPlaybackStateChanged;
        public event SongChangeDelegate? OnMediaPropertyChanged;
        public GlobalSystemMediaTransportControlsSession? ControlSession { get; private set; }

        public readonly string Id;

        private readonly MediaManager mediaManagerInstance;

        internal MediaSession(GlobalSystemMediaTransportControlsSession controlSession, MediaManager mediaMangerInstance)
        {
            mediaManagerInstance = mediaMangerInstance;
            ControlSession = controlSession;
            Id = ControlSession.SourceAppUserModelId;
            ControlSession.MediaPropertiesChanged += OnSongChange;
            ControlSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
        }

        private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession controlSession, PlaybackInfoChangedEventArgs? args = null)
        {
            var playbackInfo = controlSession.GetPlaybackInfo();

            if (playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed)
            {
                Dispose();
            }
            else
            {
                try { OnPlaybackStateChanged?.Invoke(this, playbackInfo); }
                catch { }

                try { mediaManagerInstance.OnAnyPlaybackStateChanged?.Invoke(this, playbackInfo); }
                catch { }
            }
        }

        internal async void OnSongChange(GlobalSystemMediaTransportControlsSession controlSession, MediaPropertiesChangedEventArgs? args = null)
        {
            var mediaProperties = await controlSession.TryGetMediaPropertiesAsync();

            try { OnMediaPropertyChanged?.Invoke(this, mediaProperties); }
            catch { }

            try { mediaManagerInstance.OnAnyMediaPropertyChanged?.Invoke(this, mediaProperties); }
            catch { }
        }

        internal void Dispose()
        {
            if (mediaManagerInstance.removeSource(this))
            {
                OnPlaybackStateChanged = null;
                OnMediaPropertyChanged = null;
                OnSessionClosed = null;
                ControlSession = null;

                try { OnSessionClosed?.Invoke(this); }
                catch { }
            }
        }
    }
}
