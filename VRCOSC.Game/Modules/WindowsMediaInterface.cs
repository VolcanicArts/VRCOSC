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
    public Action<GlobalSystemMediaTransportControlsSession?>? OnCurrentSessionChanged;
    public Action<GlobalSystemMediaTransportControlsSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo>? OnAnyPlaybackInfoChanged;
    public Action<GlobalSystemMediaTransportControlsSession, GlobalSystemMediaTransportControlsSessionMediaProperties>? OnAnyMediaPropertiesChanged;
    public Action<GlobalSystemMediaTransportControlsSession, GlobalSystemMediaTransportControlsSessionTimelineProperties>? OnAnyTimelinePropertiesChanged;

    private readonly List<GlobalSystemMediaTransportControlsSession> currentSessions = new();
    private GlobalSystemMediaTransportControlsSessionManager sessionManager = null!;

    public GlobalSystemMediaTransportControlsSession? CurrentSession => sessionManager.GetCurrentSession();

    public async void Initialise()
    {
        sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
    }

    public void Hook()
    {
        sessionManager.CurrentSessionChanged += currentSessionChanged;
        sessionManager.SessionsChanged += sessionsChanged;

        auditSessions();
        OnCurrentSessionChanged?.Invoke(CurrentSession);
    }

    public void UnHook()
    {
        sessionManager.CurrentSessionChanged -= currentSessionChanged;
        sessionManager.SessionsChanged -= sessionsChanged;

        currentSessions.Clear();
    }

    private void currentSessionChanged(GlobalSystemMediaTransportControlsSessionManager _, CurrentSessionChangedEventArgs _2)
    {
        OnCurrentSessionChanged?.Invoke(CurrentSession);
    }

    private void sessionsChanged(GlobalSystemMediaTransportControlsSessionManager _, SessionsChangedEventArgs _2)
    {
        auditSessions();

        // Session changes can occur due to a session closing
        // To avoid repeat code, we can just detect what may have closed rather than listening for the closed event and checking for closed sessions
        removeClosedSessions();
    }

    private void auditSessions()
    {
        sessionManager.GetSessions().Where(controlSession => !currentSessions.Contains(controlSession)).ForEach(addControlSession);
    }

    private void removeClosedSessions()
    {
        var sessions = sessionManager.GetSessions();
        var closedSessions = currentSessions.Where(session => !sessions.Contains(session)).ToList();
        if (closedSessions.Any()) currentSessions.Remove(closedSessions.First());
    }

    private void addControlSession(GlobalSystemMediaTransportControlsSession controlSession)
    {
        controlSession.PlaybackInfoChanged += (_, _) => OnAnyPlaybackInfoChanged?.Invoke(controlSession, controlSession.GetPlaybackInfo());
        controlSession.MediaPropertiesChanged += async (_, _) => OnAnyMediaPropertiesChanged?.Invoke(controlSession, await controlSession.TryGetMediaPropertiesAsync());
        controlSession.TimelinePropertiesChanged += (_, _) => OnAnyTimelinePropertiesChanged?.Invoke(controlSession, controlSession.GetTimelineProperties());
        currentSessions.Add(controlSession);
    }
}
