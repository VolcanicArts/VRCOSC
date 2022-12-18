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
        State = new MediaState();

        mediaManager = new MediaManager();
        mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
        mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;
        mediaManager.Start();
    }

    public void StopMediaHook()
    {
        mediaManager?.Dispose();
        mediaManager = null;
    }

    private void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        var mediaProperties = sender.ControlSession?.TryGetMediaPropertiesAsync().GetResults();

        if (mediaProperties is not null)
        {
            State.Title = mediaProperties.Title;
            State.Artist = mediaProperties.Artist;
        }

        State.IsShuffle = args.IsShuffleActive ?? false;
        State.RepeatMode = args.AutoRepeatMode ?? 0;
        State.Status = args.PlaybackStatus;

        OnMediaUpdate?.Invoke();
    }

    private void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        var playbackInfo = sender.ControlSession?.GetPlaybackInfo();
        if (playbackInfo is null) return;

        State.IsShuffle = playbackInfo.IsShuffleActive ?? false;
        State.RepeatMode = playbackInfo.AutoRepeatMode ?? 0;
        State.Status = playbackInfo.PlaybackStatus;
        State.Title = args.Title;
        State.Artist = args.Artist;

        OnMediaUpdate?.Invoke();
    }

    public void SetVolume(float percentage) => ProcessExtensions.SetProcessVolume(Controller?.SourceAppUserModelId ?? string.Empty, percentage);
    public void SetMuted(bool muted) => ProcessExtensions.SetProcessMuted(Controller?.SourceAppUserModelId ?? string.Empty, muted);

    public float GetVolume() => ProcessExtensions.RetrieveProcessVolume(Controller?.SourceAppUserModelId ?? string.Empty);
    public bool IsMuted() => ProcessExtensions.IsProcessMuted(Controller?.SourceAppUserModelId ?? string.Empty);
}

public class MediaState
{
    public string Title = string.Empty;
    public string Artist = string.Empty;
    public MediaPlaybackAutoRepeatMode RepeatMode;
    public bool IsShuffle;
    public GlobalSystemMediaTransportControlsSessionPlaybackStatus Status;
    public GlobalSystemMediaTransportControlsSessionTimelineProperties? Position;
    public bool IsPlaying => Status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
}
