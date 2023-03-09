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

public class WindowsMediaProvider
{
    private readonly List<GlobalSystemMediaTransportControlsSession> sessions = new();
    private GlobalSystemMediaTransportControlsSessionManager? sessionManager;

    public GlobalSystemMediaTransportControlsSession? Controller => sessionManager?.GetCurrentSession();

    public Action? OnPlaybackStateUpdate;
    public MediaState State { get; private set; } = null!;

    public async Task<bool> Hook()
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

    public void UnHook()
    {
        sessionManager!.CurrentSessionChanged -= onCurrentSessionChanged;
        sessionManager.SessionsChanged -= sessionsChanged;

        sessions.Clear();
    }

    private bool isFocusedSession(GlobalSystemMediaTransportControlsSession session) => session.SourceAppUserModelId == Controller?.SourceAppUserModelId;

    private void onAnyPlaybackStateChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        if (!isFocusedSession(session)) return;

        State.IsShuffle = args.IsShuffleActive ?? default;
        State.RepeatMode = args.AutoRepeatMode ?? default;
        State.Status = args.PlaybackStatus;

        OnPlaybackStateUpdate?.Invoke();
    }

    private void onAnyMediaPropertyChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        if (!isFocusedSession(session)) return;

        State.Title = args.Title;
        State.Artist = args.Artist;
    }

    private void onAnyTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionTimelineProperties args)
    {
        if (!isFocusedSession(session)) return;

        State.Position = args;
    }

    private async void onCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager? _, CurrentSessionChangedEventArgs? _2)
    {
        if (Controller is null)
        {
            State = new MediaState();
            return;
        }

        State.ProcessId = Controller.SourceAppUserModelId;

        onAnyPlaybackStateChanged(Controller, Controller.GetPlaybackInfo());

        try { onAnyMediaPropertyChanged(Controller, await Controller.TryGetMediaPropertiesAsync()); }
        catch (COMException) { }

        onAnyTimelinePropertiesChanged(Controller, Controller.GetTimelineProperties());
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
}

public class MediaState
{
    public string? ProcessId;
    public string Title = string.Empty;
    public string Artist = string.Empty;
    public MediaPlaybackAutoRepeatMode RepeatMode;
    public bool IsShuffle;
    public GlobalSystemMediaTransportControlsSessionPlaybackStatus Status;
    public GlobalSystemMediaTransportControlsSessionTimelineProperties? Position;

    public bool IsPlaying => Status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

    public float Volume
    {
        set => ProcessExtensions.SetProcessVolume(ProcessId, value);
        get => ProcessExtensions.RetrieveProcessVolume(ProcessId);
    }

    public bool Muted
    {
        set => ProcessExtensions.SetProcessMuted(ProcessId, value);
        get => ProcessExtensions.IsProcessMuted(ProcessId);
    }
}
