// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Windows.Media;
using Windows.Media.Control;
using VRCOSC.Game.Processes;
using WindowsMediaController;

namespace VRCOSC.Game.Modules.Modules.Media;

public class MediaProvider
{
    private MediaManager? mediaManager;

    public MediaState State { get; private set; } = null!;

    public GlobalSystemMediaTransportControlsSession? Controller => mediaManager?.GetFocusedSession()?.ControlSession;

    public Action? OnMediaUpdate;

    public void StartMediaHook()
    {
        State = new MediaState
        {
            Volume = 1
        };

        mediaManager = new MediaManager();
        mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
        mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;
        mediaManager.OnFocusedSessionChanged += MediaManager_OnFocusedSessionChanged;
        mediaManager.Start();
    }

    public void StopMediaHook()
    {
        mediaManager?.Dispose();
        mediaManager = null;
    }

    private void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        State.ProcessId = Controller?.SourceAppUserModelId;
        State.IsShuffle = args.IsShuffleActive ?? false;
        State.RepeatMode = args.AutoRepeatMode ?? 0;
        State.Status = args.PlaybackStatus;

        OnMediaUpdate?.Invoke();
    }

    private void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        State.ProcessId = Controller?.SourceAppUserModelId;
        State.Title = args.Title;
        State.Artist = args.Artist;

        OnMediaUpdate?.Invoke();
    }

    private async void MediaManager_OnFocusedSessionChanged(MediaManager.MediaSession? sender)
    {
        if (sender is null) return;

        var properties = await sender.ControlSession.TryGetMediaPropertiesAsync();
        var playbackInfo = sender.ControlSession.GetPlaybackInfo();

        State.ProcessId = Controller?.SourceAppUserModelId;
        State.Title = properties.Title;
        State.Artist = properties.Artist;
        State.IsShuffle = playbackInfo.IsShuffleActive ?? false;
        State.RepeatMode = playbackInfo.AutoRepeatMode ?? 0;
        State.Status = playbackInfo.PlaybackStatus;

        OnMediaUpdate?.Invoke();
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
