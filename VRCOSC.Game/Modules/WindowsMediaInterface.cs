// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Media.Control;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.Modules;

public class WindowsMediaInterface
{
    public Action<Session>? OnSessionOpened;
    public Action<Session>? OnSessionClosed;
    public Action<Session?>? OnFocusedSessionChanged;
    public Action<Session, GlobalSystemMediaTransportControlsSessionPlaybackInfo>? OnAnyPlaybackStateChanged;
    public Action<Session, GlobalSystemMediaTransportControlsSessionMediaProperties>? OnAnyMediaPropertyChanged;
    public Action<Session, GlobalSystemMediaTransportControlsSessionTimelineProperties>? OnAnyTimelinePropertiesChanged;

    private readonly Dictionary<string, Session> currentMediaSessions = new();
    private GlobalSystemMediaTransportControlsSessionManager windowsSessionManager = null!;

    public Session? FocusedSession => getFocusedSession(windowsSessionManager);

    public async void Initialise()
    {
        windowsSessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
    }

    public void Hook()
    {
        // Call manually to update with already open sessions
        sessionsChanged(windowsSessionManager);
        focusedSessionChanged(windowsSessionManager);

        windowsSessionManager.SessionsChanged += sessionsChanged;
        windowsSessionManager.CurrentSessionChanged += focusedSessionChanged;
        OnAnyPlaybackStateChanged += onAnyPlaybackStateChanged;
    }

    public void UnHook()
    {
        windowsSessionManager.SessionsChanged -= sessionsChanged;
        windowsSessionManager.CurrentSessionChanged -= focusedSessionChanged;
        OnAnyPlaybackStateChanged -= onAnyPlaybackStateChanged;
    }

    private void onAnyPlaybackStateChanged(Session session, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        if (args.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed) removeSession(session);
    }

    private Session? getFocusedSession(GlobalSystemMediaTransportControlsSessionManager? sessionManager)
    {
        var currentSession = sessionManager?.GetCurrentSession();
        if (currentSession is null) return null;

        // sessionsChanged may not have been called before this gets called in rare cases so a check is needed
        return currentMediaSessions.TryGetValue(currentSession.SourceAppUserModelId, out Session? session) ? session : null;
    }

    private void focusedSessionChanged(GlobalSystemMediaTransportControlsSessionManager? sessionManager, CurrentSessionChangedEventArgs? args = null)
    {
        var focusedSession = getFocusedSession(sessionManager);
        OnFocusedSessionChanged?.Invoke(focusedSession);
    }

    private void sessionsChanged(GlobalSystemMediaTransportControlsSessionManager? sessionManager, SessionsChangedEventArgs? args = null)
    {
        var controlSessionList = sessionManager?.GetSessions();
        if (controlSessionList is null) return;

        controlSessionList.Where(session => !currentMediaSessions.ContainsKey(session.SourceAppUserModelId)).ForEach(session =>
        {
            var mediaSession = new Session(session, this);
            currentMediaSessions[session.SourceAppUserModelId] = mediaSession;
            OnSessionOpened?.Invoke(mediaSession);
        });

        // A source may have shutdown without doing a close event
        var sessionIds = controlSessionList.Select(session => session.SourceAppUserModelId);
        currentMediaSessions.Where(pair => !sessionIds.Contains(pair.Key)).Select(pair => pair.Value).ForEach(removeSession);
    }

    private void removeSession(Session session)
    {
        currentMediaSessions.Remove(session.Controller.SourceAppUserModelId);
        OnSessionClosed?.Invoke(session);
    }

    public class Session
    {
        private readonly WindowsMediaInterface windowsMediaInterface;
        public readonly GlobalSystemMediaTransportControlsSession Controller;

        public Session(GlobalSystemMediaTransportControlsSession controller, WindowsMediaInterface windowsMediaInterface)
        {
            Controller = controller;
            this.windowsMediaInterface = windowsMediaInterface;

            Controller.MediaPropertiesChanged += OnSongChangeAsync;
            Controller.PlaybackInfoChanged += OnPlaybackInfoChanged;
            Controller.TimelinePropertiesChanged += OnTimelinePropertiesChanged;
        }

        private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession controlSession, PlaybackInfoChangedEventArgs args)
        {
            var playbackInfo = controlSession.GetPlaybackInfo();
            windowsMediaInterface.OnAnyPlaybackStateChanged?.Invoke(this, playbackInfo);
        }

        private async void OnSongChangeAsync(GlobalSystemMediaTransportControlsSession controlSession, MediaPropertiesChangedEventArgs args)
        {
            var mediaProperties = await controlSession.TryGetMediaPropertiesAsync();
            windowsMediaInterface.OnAnyMediaPropertyChanged?.Invoke(this, mediaProperties);
        }

        private void OnTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
        {
            var timelineProperties = Controller.GetTimelineProperties();
            windowsMediaInterface.OnAnyTimelinePropertiesChanged?.Invoke(this, timelineProperties);
        }
    }
}
