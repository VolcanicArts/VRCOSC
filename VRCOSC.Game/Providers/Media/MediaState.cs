// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Providers.Media;

public class MediaState
{
    public string? Identifier { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public string Artist { get; internal set; } = string.Empty;
    public int TrackNumber { get; internal set; }
    public string AlbumTitle { get; internal set; } = string.Empty;
    public string AlbumArtist { get; internal set; } = string.Empty;
    public int AlbumTrackCount { get; internal set; }
    public MediaRepeatMode RepeatMode { get; internal set; }
    public bool IsShuffle { get; internal set; }
    public MediaPlaybackStatus Status { get; internal set; }
    public MediaTimelineProperties Timeline { get; } = new();

    public bool IsStopped => Status == MediaPlaybackStatus.Stopped;
    public bool IsPlaying => Status == MediaPlaybackStatus.Playing;
    public bool IsPaused => Status == MediaPlaybackStatus.Paused;
}

public enum MediaRepeatMode
{
    Off,
    Single,
    Multiple
}

public enum MediaPlaybackStatus
{
    Stopped,
    Playing,
    Paused
}

public class MediaTimelineProperties
{
    public TimeSpan Start { get; internal set; } = TimeSpan.Zero;
    public TimeSpan End { get; internal set; } = TimeSpan.FromSeconds(1);
    public TimeSpan Position { get; internal set; } = TimeSpan.Zero;

    public float PositionPercentage => Position.Ticks / (float)End.Ticks;
}
