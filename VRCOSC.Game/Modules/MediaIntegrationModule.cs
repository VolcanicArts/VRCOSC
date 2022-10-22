// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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
    protected GlobalSystemMediaTransportControlsSession MediaController => mediaManager.CurrentMediaSessions[lastSender].ControlSession;

    private string lastSender = string.Empty;
    private Process? trackedProcess;

    private int processId => trackedProcess?.Id ?? 0;

    private static readonly string[] process_exclusions = { "chrome" };

    protected void StartMediaHook()
    {
        mediaManager = new MediaManager();
        mediaManager.OnAnySessionOpened += MediaSession_OnAnySessionOpened;
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
        tokenSource.Cancel();
        mediaManager.Dispose();
    }

    protected virtual void OnMediaUpdate() { }

    private void MediaSession_OnAnySessionOpened(MediaManager.MediaSession sender)
    {
        if (!updateTrackedProcess(sender)) return;

        var playbackInfo = sender.ControlSession.GetPlaybackInfo();
        MediaState.IsShuffle = playbackInfo.IsShuffleActive!.Value;
        MediaState.RepeatMode = playbackInfo.AutoRepeatMode!.Value;
        MediaState.Status = playbackInfo.PlaybackStatus;
        MediaState.Position = sender.ControlSession.GetTimelineProperties();
        MediaState.Volume = getVolume();
        MediaState.Muted = isMuted();

        OnMediaUpdate();
    }

    private void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        if (!updateTrackedProcess(sender)) return;

        MediaState.IsShuffle = args.IsShuffleActive!.Value;
        MediaState.RepeatMode = args.AutoRepeatMode!.Value;
        MediaState.Status = args.PlaybackStatus;
        MediaState.Position = sender.ControlSession.GetTimelineProperties();
        MediaState.Volume = getVolume();
        MediaState.Muted = isMuted();

        OnMediaUpdate();
    }

    private void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        if (!updateTrackedProcess(sender)) return;

        var playbackInfo = sender.ControlSession.GetPlaybackInfo();
        MediaState.IsShuffle = playbackInfo.IsShuffleActive!.Value;
        MediaState.RepeatMode = playbackInfo.AutoRepeatMode!.Value;
        MediaState.Status = playbackInfo.PlaybackStatus;
        MediaState.Title = args.Title;
        MediaState.Artist = args.Artist;
        MediaState.Position = sender.ControlSession.GetTimelineProperties();
        MediaState.Volume = getVolume();
        MediaState.Muted = isMuted();

        OnMediaUpdate();
    }

    private bool updateTrackedProcess(MediaManager.MediaSession sender)
    {
        if (process_exclusions.Contains(sender.Id.Replace(".exe", string.Empty))) return false;

        if (lastSender != sender.Id)
        {
            trackedProcess = Process.GetProcessesByName(sender.Id.Replace(".exe", string.Empty)).FirstOrDefault()!;
            lastSender = sender.Id;
        }

        return true;
    }

    protected void SetVolume(float percentage)
    {
        ProcessExtensions.SetProcessVolume(processId, percentage);
    }

    private float getVolume()
    {
        return ProcessExtensions.RetrieveProcessVolume(processId);
    }

    protected void SetMuted(bool muted)
    {
        ProcessExtensions.SetProcessMuted(processId, muted);
    }

    private bool isMuted()
    {
        return ProcessExtensions.IsProcessMuted(processId);
    }
}

public class MediaState
{
    public string? Title;
    public string? Artist;
    public float Volume;
    public bool Muted;
    public MediaPlaybackAutoRepeatMode RepeatMode;
    public bool IsShuffle;
    public GlobalSystemMediaTransportControlsSessionPlaybackStatus Status;
    public GlobalSystemMediaTransportControlsSessionTimelineProperties Position;
    public bool IsPlaying => Status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
}
