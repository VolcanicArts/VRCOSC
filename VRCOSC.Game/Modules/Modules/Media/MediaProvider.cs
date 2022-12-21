// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Windows.Media;
using Windows.Media.Control;
using VRCOSC.Game.Processes;

namespace VRCOSC.Game.Modules.Modules.Media;

public class MediaProvider
{
    private readonly WindowsMediaInterface mediaInterface;

    public MediaState State { get; private set; } = null!;

    public GlobalSystemMediaTransportControlsSession? Controller => mediaInterface?.FocusedSession?.Controller;

    public Action? OnMediaUpdate;

    public MediaProvider()
    {
        mediaInterface = new WindowsMediaInterface();
        mediaInterface.OnAnyPlaybackStateChanged += OnAnyPlaybackStateChanged;
        mediaInterface.OnAnyMediaPropertyChanged += OnAnyMediaPropertyChanged;
        mediaInterface.OnAnyTimelinePropertiesChanged += OnAnyTimelinePropertiesChanged;
        mediaInterface.OnFocusedSessionChanged += OnFocusedSessionChanged;
        mediaInterface.Initialise();
    }

    public void StartMediaHook()
    {
        State = new MediaState();
        mediaInterface.Hook();
    }

    public void StopMediaHook()
    {
        mediaInterface.UnHook();
    }

    private void OnAnyPlaybackStateChanged(WindowsMediaInterface.Session session, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        State.IsShuffle = args.IsShuffleActive ?? false;
        State.RepeatMode = args.AutoRepeatMode ?? 0;
        State.Status = args.PlaybackStatus;

        OnMediaUpdate?.Invoke();
    }

    private void OnAnyMediaPropertyChanged(WindowsMediaInterface.Session session, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        State.Title = args.Title;
        State.Artist = args.Artist;

        OnMediaUpdate?.Invoke();
    }

    private void OnAnyTimelinePropertiesChanged(WindowsMediaInterface.Session session, GlobalSystemMediaTransportControlsSessionTimelineProperties args)
    {
        State.Position = args;

        OnMediaUpdate?.Invoke();
    }

    private async void OnFocusedSessionChanged(WindowsMediaInterface.Session? session)
    {
        if (session is null) return;

        OnAnyPlaybackStateChanged(session, session.Controller.GetPlaybackInfo());
        OnAnyMediaPropertyChanged(session, await session.Controller.TryGetMediaPropertiesAsync());
        OnAnyTimelinePropertiesChanged(session, session.Controller.GetTimelineProperties());
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
