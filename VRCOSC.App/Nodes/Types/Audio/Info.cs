// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using SoundFlow.Enums;
using SoundFlow.Interfaces;

namespace VRCOSC.App.Nodes.Types.Audio;

public abstract class SoundPlayerConsumeNode() : ValueConsumeNode<ISoundPlayer?>("Player"), IContinuousNode
{
    public int UpdateOffset => 0;

    protected override void ConsumeValue(ISoundPlayer? player, PulseContext c)
    {
        if (player is null) return;

        ConsumeSoundPlayer(player, c);
    }

    protected abstract void ConsumeSoundPlayer(ISoundPlayer player, PulseContext c);
}

[Node("Player State", "Audio/Info")]
public sealed class AudioPlayerStateNode : SoundPlayerConsumeNode
{
    public ValueOutput<PlaybackState> State = new();

    protected override void ConsumeSoundPlayer(ISoundPlayer player, PulseContext c) => State.Write(player.State, c);
}

[Node("Player Settings", "Audio/Info")]
public sealed class AudioPlayerSettingsNode : SoundPlayerConsumeNode
{
    public ValueOutput<float> Volume = new();
    public ValueOutput<float> Speed = new();
    public ValueOutput<bool> IsLooping = new();

    protected override void ConsumeSoundPlayer(ISoundPlayer player, PulseContext c)
    {
        Volume.Write(player.Volume, c);
        Speed.Write(player.PlaybackSpeed, c);
        IsLooping.Write(player.IsLooping, c);
    }
}

[Node("Player Time", "Audio/Info")]
public sealed class AudioPlayerTimeNode : SoundPlayerConsumeNode
{
    public ValueOutput<TimeSpan> Duration = new();
    public ValueOutput<TimeSpan> Current = new();
    public ValueOutput<float> Progress = new();

    protected override void ConsumeSoundPlayer(ISoundPlayer player, PulseContext c)
    {
        var progress = player.Duration == 0f ? 0f : player.Time / player.Duration;

        Duration.Write(TimeSpan.FromSeconds(player.Duration), c);
        Current.Write(TimeSpan.FromSeconds(player.Time), c);
        Progress.Write(progress, c);
    }
}