// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using SoundFlow.Abstracts.Devices;
using VRCOSC.App.Audio;

namespace VRCOSC.App.Nodes.Types.Audio;

[Node("Default Playback Device", "Audio/Devices")]
public sealed class AudioDefaultPlaybackDeviceNode : UpdateNode<AudioPlaybackDevice?>
{
    public ValueOutput<AudioPlaybackDevice?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AudioManager.GetInstance().GetDefaultPlaybackDevice(), c);
        return Task.CompletedTask;
    }

    protected override Task<AudioPlaybackDevice?> GetValue(PulseContext c) => Task.FromResult(AudioManager.GetInstance().GetDefaultPlaybackDevice());
}

[Node("Playback Device Source", "Audio/Devices")]
public sealed class AudioPlaybackDeviceSourceNode : UpdateNode<AudioPlaybackDevice?>, IHasTextProperty
{
    [NodeProperty("device_name")]
    public string Text { get; set; } = string.Empty;

    public ValueOutput<AudioPlaybackDevice?> Device = new();

    protected override Task Process(PulseContext c)
    {
        Device.Write(AudioManager.GetInstance().GetPlaybackDeviceByName(Text), c);
        return Task.CompletedTask;
    }

    protected override Task<AudioPlaybackDevice?> GetValue(PulseContext c) => Task.FromResult(AudioManager.GetInstance().GetPlaybackDeviceByName(Text));
}