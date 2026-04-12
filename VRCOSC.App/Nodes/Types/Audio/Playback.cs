// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using SoundFlow.Enums;
using SoundFlow.Interfaces;

namespace VRCOSC.App.Nodes.Types.Audio;

[Node("Player Play", "Audio")]
public sealed class AudioPlayerPlayNode : SimpleActionNode
{
    public ValueInput<ISoundPlayer?> Player = new();

    protected override void DoAction(PulseContext c)
    {
        var player = Player.Read(c);
        if (player is null) return;

        if (player.State == PlaybackState.Stopped && float.Abs(player.Time - player.Duration) < float.Epsilon)
            player.Seek(0f);

        player.Play();
    }
}

[Node("Player Stop", "Audio")]
public sealed class AudioPlayerStopNode : SimpleActionNode
{
    public ValueInput<ISoundPlayer?> Player = new();

    protected override void DoAction(PulseContext c) => Player.Read(c)?.Stop();
}

[Node("Player Pause", "Audio")]
public sealed class AudioPlayerPauseNode : SimpleActionNode
{
    public ValueInput<ISoundPlayer?> Player = new();

    protected override void DoAction(PulseContext c) => Player.Read(c)?.Pause();
}

[Node("Player Set Volume", "Audio")]
public sealed class AudioPlayerSetVolumeNode : SimpleActionNode
{
    public ValueInput<ISoundPlayer?> Player = new();
    public ValueInput<float> Volume = new(defaultValue: 1f);

    protected override void DoAction(PulseContext c) => Player.Read(c)?.Volume = Volume.Read(c);
}

[Node("Player Set Speed", "Audio")]
public sealed class AudioPlayerSetSpeedNode : SimpleActionNode
{
    public ValueInput<ISoundPlayer?> Player = new();
    public ValueInput<float> Speed = new(defaultValue: 1f);

    protected override void DoAction(PulseContext c) => Player.Read(c)?.PlaybackSpeed = Speed.Read(c);
}

[Node("Player Seek", "Audio")]
public sealed class AudioPlayerSeekNode : SimpleActionNode
{
    public ValueInput<ISoundPlayer?> Player = new();
    public ValueInput<TimeSpan> Time = new();
    public ValueInput<SeekOrigin> Origin = new();

    protected override void DoAction(PulseContext c) => Player.Read(c)?.Seek(Time.Read(c), Origin.Read(c));
}