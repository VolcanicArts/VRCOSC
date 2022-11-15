// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Media;
using Windows.Media.Control;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules.Modules.Media;

public class MediaProvider
{
    private MediaManager? mediaManager;
    private Process? trackedProcess;
    private string? lastSender;

    private int trackedProcessId => trackedProcess?.Id ?? -1;

    public MediaState State { get; private set; } = null!;

    public GlobalSystemMediaTransportControlsSession? Controller
        => mediaManager?.CurrentMediaSessions.ContainsKey(lastSender ?? string.Empty) ?? false ? mediaManager.CurrentMediaSessions[lastSender!].ControlSession : null;

    public IEnumerable<string> ProcessExclusions = Array.Empty<string>();

    public Action? OnMediaSessionOpened;
    public Action? OnMediaUpdate;

    public void StartMediaHook()
    {
        State = new MediaState();

        mediaManager = new MediaManager();
        mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
        mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
        mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
        mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;
        _ = mediaManager.Start();
    }

    public void StopMediaHook()
    {
        mediaManager?.Dispose();
        mediaManager = null;

        lastSender = null;
        trackedProcess = null;
    }

    private void MediaManager_OnAnySessionOpened(MediaManager.MediaSession sender)
    {
        if (!updateTrackedProcess(sender)) return;

        State.Position = sender.ControlSession?.GetTimelineProperties() ?? null;

        OnMediaSessionOpened?.Invoke();
    }

    private void MediaManager_OnAnySessionClosed(MediaManager.MediaSession sender)
    {
        lastSender = null;
        trackedProcess = null;
    }

    private void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        if (!updateTrackedProcess(sender)) return;

        State.IsShuffle = args.IsShuffleActive ?? false;
        State.RepeatMode = args.AutoRepeatMode ?? 0;
        State.Status = args.PlaybackStatus;

        OnMediaUpdate?.Invoke();
    }

    private void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        if (!updateTrackedProcess(sender)) return;

        var playbackInfo = sender.ControlSession?.GetPlaybackInfo();
        if (playbackInfo is null) return;

        State.IsShuffle = playbackInfo.IsShuffleActive ?? false;
        State.RepeatMode = playbackInfo.AutoRepeatMode ?? 0;
        State.Status = playbackInfo.PlaybackStatus;
        State.Title = args.Title;
        State.Artist = args.Artist;

        OnMediaUpdate?.Invoke();
    }

    private bool updateTrackedProcess(MediaManager.MediaSession sender)
    {
        if (ProcessExclusions.Contains(sender.Id.Replace(".exe", string.Empty))) return false;

        if (lastSender != sender.Id)
        {
            trackedProcess = Process.GetProcessesByName(sender.Id.Replace(".exe", string.Empty)).FirstOrDefault();
            lastSender = trackedProcess is null ? null : sender.Id;
        }

        return true;
    }

    public void SetVolume(float percentage) => ProcessExtensions.SetProcessVolume(trackedProcessId, percentage);
    public void SetMuted(bool muted) => ProcessExtensions.SetProcessMuted(trackedProcessId, muted);

    public float GetVolume() => ProcessExtensions.RetrieveProcessVolume(trackedProcessId);
    public bool IsMuted() => ProcessExtensions.IsProcessMuted(trackedProcessId);
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
