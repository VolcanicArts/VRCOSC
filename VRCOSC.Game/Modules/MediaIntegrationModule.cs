// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Control;
using VRCOSC.Game.Util;
using WindowsMediaController;

namespace VRCOSC.Game.Modules;

public abstract class MediaIntegrationModule : Module
{
    private MediaManager mediaManager = null!;
    private CancellationTokenSource tokenSource = null!;

    protected MediaState MediaState = new();

    protected GlobalSystemMediaTransportControlsSession? MediaController
    {
        get
        {
            try
            {
                return mediaManager.CurrentMediaSessions[lastSender].ControlSession;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
    }

    private string lastSender = string.Empty;
    private Process? trackedProcess;

    private int processId => trackedProcess?.Id ?? -1;

    protected virtual IEnumerable<string> ProcessExclusions => Array.Empty<string>();

    protected override void OnStart()
    {
        lastSender = string.Empty;
        trackedProcess = null;
    }

    protected void StartMediaHook()
    {
        mediaManager = new MediaManager();
        mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
        mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
        mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
        mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;

        tokenSource = new CancellationTokenSource();

        Task.Run(() =>
        {
            mediaManager.Start();
            tokenSource.Token.WaitHandle.WaitOne();
        });
    }

    protected void StopMediaHook()
    {
        mediaManager.Dispose();
        tokenSource.Cancel();
    }

    protected virtual void OnMediaSessionOpened() { }

    protected virtual void OnMediaUpdate() { }

    private void MediaManager_OnAnySessionOpened(MediaManager.MediaSession sender)
    {
        if (!updateTrackedProcess(sender)) return;

        OnMediaSessionOpened();
    }

    private void MediaManager_OnAnySessionClosed(MediaManager.MediaSession sender)
    {
        lastSender = string.Empty;
        trackedProcess = null;
    }

    private void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        if (!updateTrackedProcess(sender)) return;

        MediaState.IsShuffle = args.IsShuffleActive ?? false;
        MediaState.RepeatMode = args.AutoRepeatMode ?? 0;
        MediaState.Status = args.PlaybackStatus;

        OnMediaUpdate();
    }

    private void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        if (!updateTrackedProcess(sender)) return;

        var playbackInfo = sender.ControlSession.GetPlaybackInfo();
        MediaState.IsShuffle = playbackInfo.IsShuffleActive ?? false;
        MediaState.RepeatMode = playbackInfo.AutoRepeatMode ?? 0;
        MediaState.Status = playbackInfo.PlaybackStatus;
        MediaState.Title = args.Title;
        MediaState.Artist = args.Artist;

        OnMediaUpdate();
    }

    private bool updateTrackedProcess(MediaManager.MediaSession sender)
    {
        if (ProcessExclusions.Contains(sender.Id.Replace(".exe", string.Empty))) return false;

        if (lastSender != sender.Id)
        {
            trackedProcess = Process.GetProcessesByName(sender.Id.Replace(".exe", string.Empty)).FirstOrDefault();
            lastSender = trackedProcess is null ? string.Empty : sender.Id;
        }

        return true;
    }

    protected void SetVolume(float percentage)
    {
        if (!ensureProcessExists()) return;

        ProcessExtensions.SetProcessVolume(processId, percentage);
    }

    protected float GetVolume()
    {
        return ensureProcessExists() ? ProcessExtensions.RetrieveProcessVolume(processId) : 0f;
    }

    protected void SetMuted(bool muted)
    {
        if (!ensureProcessExists()) return;

        ProcessExtensions.SetProcessMuted(processId, muted);
    }

    protected bool IsMuted()
    {
        return ensureProcessExists() ? ProcessExtensions.IsProcessMuted(processId) : false;
    }

    private bool ensureProcessExists()
    {
        try
        {
            Process.GetProcessById(processId);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

public class MediaState
{
    public string? Title;
    public string? Artist;
    public MediaPlaybackAutoRepeatMode RepeatMode;
    public bool IsShuffle;
    public GlobalSystemMediaTransportControlsSessionPlaybackStatus Status;
    public GlobalSystemMediaTransportControlsSessionTimelineProperties Position;
    public bool IsPlaying => Status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
}
