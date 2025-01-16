// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using Windows.Media;
using Windows.Media.Control;

namespace VRCOSC.App.SDK.Providers.Media;

public class MediaState
{
    public string ID { get; internal init; } = string.Empty;
    public string Title { get; internal set; } = string.Empty;
    public string Subtitle { get; internal set; } = string.Empty;
    public string Artist { get; internal set; } = string.Empty;
    public int TrackNumber { get; internal set; }
    public string AlbumTitle { get; internal set; } = string.Empty;
    public string AlbumArtist { get; internal set; } = string.Empty;
    public int AlbumTrackCount { get; internal set; }
    public List<string> Genres { get; internal set; } = new();
    public MediaPlaybackAutoRepeatMode RepeatMode { get; internal set; }
    public bool IsShuffle { get; internal set; }
    public GlobalSystemMediaTransportControlsSessionPlaybackStatus Status { get; internal set; } = GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped;
    public MediaTimelineProperties Timeline { get; } = new();

    public bool IsStopped => Status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped;
    public bool IsPlaying => Status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
    public bool IsPaused => Status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused;
}

public class MediaTimelineProperties
{
    public TimeSpan Start { get; internal set; } = TimeSpan.Zero;

    private TimeSpan end = TimeSpan.Zero;

    public TimeSpan End
    {
        get
        {
            if (Math.Abs(end.Ticks) == 0 || Position >= end) return Position;

            return end;
        }
        internal set => end = value;
    }

    public TimeSpan Position { get; internal set; } = TimeSpan.Zero;

    /// <summary>
    /// The progress of the song as a normalised percentage
    /// </summary>
    public float Progress => Math.Abs(End.Ticks) == 0 || Position >= End ? 1f : Position.Ticks / (float)End.Ticks;
}